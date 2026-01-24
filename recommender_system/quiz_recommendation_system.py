import os
import sys
import threading
import time
from datetime import datetime
import numpy as np
import pandas as pd
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.neighbors import NearestNeighbors
from sklearn.feature_extraction.text import TfidfVectorizer
from sqlalchemy import create_engine, text
from flask import Flask, jsonify, request, abort
from flask_cors import CORS
import logging
from collections import defaultdict

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class QuizRecommendationSystem:
    """
    Recommendation system using Collaborative Filtering with KNN and Content-Based Filtering
    """
    
    def __init__(self, connection_string: str, k_neighbors: int = 5):
        self.connection_string = connection_string
        self.k_neighbors = k_neighbors
        # Initialize engine
        self.engine = create_engine(connection_string)
        
        # Store user-item matrix and item similarity matrix
        self.user_item_matrix = None
        self.item_similarity_matrix = None
        self.quiz_features = None
        
        logger.info("Quiz Recommendation System initialized")
    
    def load_data(self):
        """
        Load quiz ratings data from the database
        """
        logger.info("Loading quiz ratings data from database...")
        
        # Query to get all quiz ratings with student and quiz IDs
        query = """
        SELECT 
            rf.StudentId as student_id,
            rf.QuizId as quiz_id,
            rf.Score as rating
        FROM QuizFeedbacks rf
        WHERE rf.Score IS NOT NULL
        ORDER BY rf.StudentId, rf.QuizId
        """
        
        # Execute query and load data into DataFrame
        df = pd.read_sql(query, self.engine)
        
        if df.empty:
            logger.warning("No rating data found in the database")
            return pd.DataFrame()
        
        logger.info(f"Loaded {len(df)} rating records from database")
        logger.info(f"Unique students: {df['student_id'].nunique()}, Unique quizzes: {df['quiz_id'].nunique()}")
        
        return df
    
    def build_user_item_matrix(self, df):
        """
        Build user-item matrix for collaborative filtering
        """
        if df.empty:
            self.user_item_matrix = pd.DataFrame()
            return
        
        # Create pivot table for user-item matrix
        self.user_item_matrix = df.pivot_table(
            index='student_id',
            columns='quiz_id',
            values='rating',
            fill_value=0
        )
        
        logger.info(f"Built user-item matrix: {self.user_item_matrix.shape}")
        
    def compute_item_similarity(self):
        """
        Compute item-to-item similarity matrix using cosine similarity
        """
        if self.user_item_matrix is None or self.user_item_matrix.empty:
            logger.warning("User-item matrix is empty, cannot compute similarity")
            return
        
        # Transpose the user-item matrix so items become rows
        item_matrix = self.user_item_matrix.T  # Shape: (num_items, num_users)
        
        # Calculate cosine similarity between items
        logger.info("Computing item similarities using cosine similarity...")
        self.item_similarity_matrix = cosine_similarity(item_matrix)
        
        # Convert to DataFrame with quiz IDs as index/columns
        self.item_similarity_matrix = pd.DataFrame(
            self.item_similarity_matrix,
            index=self.user_item_matrix.columns,
            columns=self.user_item_matrix.columns
        )
        
        logger.info(f"Computed item similarity matrix: {self.item_similarity_matrix.shape}")
    
    def fit_knn_model(self, df):
        """
        Train KNN model on the quiz features for enhanced recommendations
        """
        if df.empty:
            return

        # Get unique quizzes with their features - using the correct table name 'Quizzes'
        # Primary query for SQL Server (with CAST for GUID handling)
        quiz_query = """
        SELECT
            q.Id as quiz_id,
            q.CategoryId as category_id,
            q.TotalQuestions,
            q.TimeInMinutes,
            q.Level,
            COALESCE(AVG(rf.Score), 0) as avg_rating,
            COUNT(rf.Score) as rating_count
        FROM Quizzes q
        LEFT JOIN QuizFeedbacks rf ON CAST(q.Id AS VARCHAR(MAX)) = rf.QuizId
        GROUP BY q.Id, q.CategoryId, q.TotalQuestions, q.TimeInMinutes, q.Level
        """

        # Alternative query for SQLite and other databases that handle GUIDs properly as text
        quiz_query_alt = """
        SELECT
            q.Id as quiz_id,
            q.CategoryId as category_id,
            q.TotalQuestions,
            q.TimeInMinutes,
            q.Level,
            COALESCE(AVG(rf.Score), 0) as avg_rating,
            COUNT(rf.Score) as rating_count
        FROM Quizzes q
        LEFT JOIN QuizFeedbacks rf ON q.Id = rf.QuizId
        GROUP BY q.Id, q.CategoryId, q.TotalQuestions, q.TimeInMinutes, q.Level
        """

        try:
            quiz_df = pd.read_sql(quiz_query, self.engine)
        except:
            try:
                quiz_df = pd.read_sql(quiz_query_alt, self.engine)
            except Exception as e:
                logger.error(f"Failed to load quiz data: {str(e)}")
                quiz_df = pd.DataFrame(columns=['quiz_id', 'category_id', 'TotalQuestions', 'TimeInMinutes', 'Level', 'avg_rating', 'rating_count'])

        # Only continue if we have quiz data
        if quiz_df.empty:
            logger.warning("No quiz data found for KNN model")
            return

        # Prepare features for KNN
        features_df = quiz_df.copy()

        # Convert categorical variables to numeric
        level_mapping = {'Easy': 1, 'Medium': 2, 'Hard': 3, 'easy': 1, 'medium': 2, 'hard': 3}
        features_df['level_encoded'] = features_df['Level'].map(level_mapping).fillna(0)

        # Select numeric features
        feature_columns = ['category_id', 'TotalQuestions', 'TimeInMinutes', 'avg_rating', 'level_encoded', 'rating_count']
        features_df = features_df[feature_columns].fillna(0)

        # Normalize features
        from sklearn.preprocessing import StandardScaler
        self.scaler = StandardScaler()
        features_scaled = self.scaler.fit_transform(features_df)

        # Fit KNN model
        n_neighbors = min(self.k_neighbors, len(features_df))
        if n_neighbors < 2:
            n_neighbors = 2  # Need at least 2 neighbors for KNN to work properly

        self.knn_model = NearestNeighbors(n_neighbors=n_neighbors, metric='cosine')
        self.knn_model.fit(features_scaled)

        # Store quiz IDs for reference
        self.quiz_ids = quiz_df['quiz_id'].tolist()
        self.quiz_features = features_scaled

        logger.info(f"Trained KNN model with {len(self.quiz_ids)} quizzes")

    def recommend_for_user(self, user_id: int, n_recommendations: int = 5) -> list:
        """
        Recommend quizzes for a specific user using Item-Based CF + KNN
        """
        if self.user_item_matrix is None or self.user_item_matrix.empty:
            logger.warning("No user-item matrix available for recommendations")
            return []
        
        if user_id not in self.user_item_matrix.index:
            logger.info(f"User {user_id} not found in the dataset, returning popular quizzes")
            return self.get_popular_quizzes(n_recommendations)
        
        # Get user's ratings
        user_ratings = self.user_item_matrix.loc[user_id]
        
        # Find rated quizzes
        rated_mask = user_ratings != 0
        rated_quiz_ids = user_ratings[rated_mask].index.tolist()
        
        # If user has not rated anything, return popular quizzes
        if not rated_quiz_ids:
            logger.info(f"User {user_id} has not rated any quizzes, returning popular quizzes")
            return self.get_popular_quizzes(n_recommendations)
        
        # Calculate predicted ratings for unrated quizzes
        recommendations = {}
        
        for quiz_id in self.user_item_matrix.columns:
            if user_ratings[quiz_id] == 0:  # Only for unrated quizzes
                # Get similar items
                if self.item_similarity_matrix is not None:
                    similar_items = self.item_similarity_matrix[quiz_id].sort_values(ascending=False)
                    
                    # Calculate weighted sum of ratings from similar items user has rated
                    weighted_sum = 0
                    similarity_sum = 0
                    
                    for sim_item_id in similar_items.index:
                        if sim_item_id in rated_quiz_ids:
                            similarity_score = similar_items[sim_item_id]
                            user_rating = user_ratings[sim_item_id]
                            
                            weighted_sum += similarity_score * user_rating
                            similarity_sum += abs(similarity_score)
                    
                    if similarity_sum > 0:
                        predicted_rating = weighted_sum / similarity_sum
                        recommendations[quiz_id] = predicted_rating
        
        # Sort recommendations by predicted rating
        sorted_recommendations = sorted(recommendations.items(), key=lambda x: x[1], reverse=True)
        
        # Take top N recommendations
        top_recommendations = sorted_recommendations[:min(n_recommendations, len(sorted_recommendations))]
        
        final_recommendations = []
        for quiz_id, predicted_rating in top_recommendations:
            final_recommendations.append({
                'quiz_id': str(quiz_id),
                'predicted_rating': round(predicted_rating, 2)
            })
        
        # Print the predicted ratings for debugging/output
        print(f"\nCollaborative Filtering Recommendations for User {user_id}:")
        for i, rec in enumerate(final_recommendations, 1):
            print(f"  {i}. Quiz ID: {rec['quiz_id']} | Predicted Rating: {rec['predicted_rating']}")

        # Log the recommended quiz IDs
        recommended_quiz_ids = [rec['quiz_id'] for rec in final_recommendations]
        logger.info(f"Generated {len(final_recommendations)} recommendations for user {user_id}: {recommended_quiz_ids}")
        print(f"Generated {len(final_recommendations)} recommendations for user {user_id}: {recommended_quiz_ids}")
        return final_recommendations
    
    def get_popular_quizzes(self, n_recommendations: int = 5) -> list:
        """
        Get popular quizzes based on average rating and number of ratings
        """
        # Query to get quiz popularity metrics
        query = """
        SELECT
            rf.QuizId as quiz_id,
            AVG(rf.Score) as avg_rating,
            COUNT(rf.Score) as rating_count
        FROM QuizFeedbacks rf
        WHERE rf.Score IS NOT NULL
        GROUP BY rf.QuizId
        HAVING COUNT(rf.Score) >= 1
        ORDER BY AVG(rf.Score) DESC, COUNT(rf.Score) DESC
        """

        popular_df = pd.read_sql(query, self.engine)

        # Take top N most popular
        top_n = min(n_recommendations, len(popular_df))
        top_quizzes = popular_df.head(top_n)

        recommendations = []
        for _, row in top_quizzes.iterrows():
            recommendations.append({
                'quiz_id': str(row['quiz_id']),
                'predicted_rating': round(float(row['avg_rating']), 2),
                'rating_count': int(row['rating_count']),
                'method': 'popular'  # Add method field for consistency
            })

        # Print the predicted ratings for popular quizzes
        print(f"\nPopular Quizzes Recommendations:")
        for i, rec in enumerate(recommendations, 1):
            print(f"  {i}. Quiz ID: {rec['quiz_id']} | Predicted Rating: {rec['predicted_rating']} | Rating Count: {rec.get('rating_count', 0)}")

        # Log the popular quiz IDs
        popular_quiz_ids = [rec['quiz_id'] for rec in recommendations]
        logger.info(f"Popular quizzes recommended: {popular_quiz_ids}")
        print(f"Popular quiz IDs from database: {popular_quiz_ids}")
        return recommendations

    def get_quiz_details(self, quiz_ids):
        """
        Get detailed information about specific quizzes
        """
        if not quiz_ids:
            return []

        # Create parameterized query to get quiz details
        placeholders = ','.join([f"'{qid}'" for qid in quiz_ids])
        query = f"""
        SELECT
            Id as quiz_id,
            Name as name,
            Description as description,
            CategoryId as category_id,
            TotalQuestions as total_questions,
            TimeInMinutes as time_in_minutes,
            Level as level,
            CreatedAt as created_at
        FROM Quizzes
        WHERE CAST(Id AS VARCHAR(MAX)) IN ({placeholders})
        """

        try:
            quiz_details_df = pd.read_sql(query, self.engine)
            return quiz_details_df.to_dict('records')
        except Exception as e:
            logger.error(f"Error fetching quiz details: {str(e)}")
            return []
    
    def update_recommendations(self):
        """
        Update the recommendation system with latest data
        """
        logger.info("Updating recommendation system...")
        
        # Load latest data
        df = self.load_data()
        
        if not df.empty:
            # Build user-item matrix
            self.build_user_item_matrix(df)
            
            # Compute item similarities
            self.compute_item_similarity()
            
            # Fit KNN model on quiz features
            self.fit_knn_model(df)
            
            logger.info("Recommendation system updated successfully")
        else:
            logger.warning("No data available to update the recommendation system")
    
    def get_user_recommendations_with_knn(self, user_id: int, n_recommendations: int = 5) -> dict:
        """
        Enhanced recommendation combining Item-Based CF and KNN
        """
        cf_recommendations = self.recommend_for_user(user_id, n_recommendations * 2)  # Get more candidates
        
        if not cf_recommendations and self.knn_model is not None:
            # If collaborative filtering yields no results, use KNN on features
            logger.info(f"No CF recommendations for user {user_id}, falling back to KNN")
            return self._recommend_by_knn_features(user_id, n_recommendations)
        
        # Enhance with KNN for similar items
        enhanced_recommendations = []
        user_quiz_ids = []
        
        if user_id in self.user_item_matrix.index:
            user_ratings = self.user_item_matrix.loc[user_id]
            user_quiz_ids = user_ratings[user_ratings > 0].index.tolist()
        
        for rec in cf_recommendations:
            quiz_id = rec['quiz_id']
            predicted_rating = rec['predicted_rating']
            
            # Add context based on similar quizzes user liked
            similar_to_user_rated = []
            for rated_quiz_id in user_quiz_ids:
                if (self.item_similarity_matrix is not None and 
                    rated_quiz_id in self.item_similarity_matrix.columns and 
                    quiz_id in self.item_similarity_matrix.index):
                    similarity = self.item_similarity_matrix.loc[quiz_id, rated_quiz_id]
                    if similarity > 0.1:  # Only if similarity is meaningful
                        similar_to_user_rated.append({
                            'quiz_id': str(rated_quiz_id),
                            'similarity': round(similarity, 3)
                        })
            
            enhanced_recommendations.append({
                'quiz_id': quiz_id,
                'predicted_rating': predicted_rating,
                'similar_rated_quiz_ids': similar_to_user_rated
            })
        
        # Print the predicted ratings for enhanced recommendations
        print(f"\nEnhanced Collaborative Filtering Recommendations for User {user_id}:")
        for i, rec in enumerate(enhanced_recommendations[:n_recommendations], 1):
            print(f"  {i}. Quiz ID: {rec['quiz_id']} | Predicted Rating: {rec['predicted_rating']}")

        # Log the recommended quiz IDs
        recommended_quiz_ids = [rec['quiz_id'] for rec in enhanced_recommendations[:n_recommendations]]
        logger.info(f"Recommended quizzes for user {user_id}: {recommended_quiz_ids}")
        print(f"Recommended quiz IDs from database for user {user_id}: {recommended_quiz_ids}")

        # Limit to required number
        return {
            'user_id': user_id,
            'recommendations': enhanced_recommendations[:n_recommendations],
            'timestamp': datetime.now().isoformat()
        }
    
    def _recommend_by_knn_features(self, user_id: int, n_recommendations: int) -> dict:
        """
        Recommend using KNN based on quiz features when CF is not applicable
        """
        recommendations = []

        # First, get user's rated quizzes if possible
        user_rated_quiz_ids = []
        if user_id in self.user_item_matrix.index:
            user_ratings = self.user_item_matrix.loc[user_id]
            user_rated_quiz_ids = user_ratings[user_ratings > 0].index.tolist()

        # If we have the KNN model and quiz features, recommend based on user's preferences
        if hasattr(self, 'knn_model') and hasattr(self, 'quiz_features') and self.quiz_features is not None:
            # If user has rated some quizzes, find similar quizzes to those
            if user_rated_quiz_ids:
                # Find features for quizzes the user has rated
                user_rated_indices = [
                    i for i, quiz_id in enumerate(self.quiz_ids)
                    if str(quiz_id) in user_rated_quiz_ids
                ]

                # Get average profile of quizzes user likes (only highly rated ones)
                high_rated_indices = [
                    i for i, quiz_id in enumerate(self.quiz_ids)
                    if str(quiz_id) in user_rated_quiz_ids and
                       user_ratings[str(quiz_id)] >= 4  # Only highly rated
                ]

                if high_rated_indices:
                    # Use the average feature vector of highly rated quizzes as the basis
                    avg_features = np.mean([self.quiz_features[i] for i in high_rated_indices], axis=0)

                    # Find nearest neighbors to this averaged profile
                    distances, indices = self.knn_model.kneighbors(
                        [avg_features],
                        n_neighbors=min(len(self.quiz_features), n_recommendations * 3)
                    )

                    # Get recommendations from the nearest neighbors that user hasn't rated
                    for neighbor_idx in indices[0]:
                        neighbor_quiz_id = self.quiz_ids[neighbor_idx]
                        neighbor_quiz_str = str(neighbor_quiz_id)

                        # Only recommend if user hasn't rated this quiz and not already in recommendations
                        if (neighbor_quiz_str not in user_rated_quiz_ids and
                           neighbor_quiz_str not in [r['quiz_id'] for r in recommendations]):

                            # Estimate rating based on similarity
                            estimated_rating = self._estimate_rating_from_similarity(
                                user_rated_quiz_ids, neighbor_quiz_str
                            )

                            recommendations.append({
                                'quiz_id': neighbor_quiz_str,
                                'predicted_rating': round(estimated_rating, 2),
                                'similarity_basis': 'feature_similarity'
                            })

                            if len(recommendations) >= n_recommendations:
                                break
                elif user_rated_indices:
                    # If no high ratings, use all rated quizzes as basis
                    avg_features = np.mean([self.quiz_features[i] for i in user_rated_indices], axis=0)

                    distances, indices = self.knn_model.kneighbors(
                        [avg_features],
                        n_neighbors=min(len(self.quiz_features), n_recommendations * 2)
                    )

                    for neighbor_idx in indices[0]:
                        neighbor_quiz_id = self.quiz_ids[neighbor_idx]
                        neighbor_quiz_str = str(neighbor_quiz_id)

                        if (neighbor_quiz_str not in user_rated_quiz_ids and
                           neighbor_quiz_str not in [r['quiz_id'] for r in recommendations]):

                            estimated_rating = self._estimate_rating_from_similarity(
                                user_rated_quiz_ids, neighbor_quiz_str
                            )

                            recommendations.append({
                                'quiz_id': neighbor_quiz_str,
                                'predicted_rating': round(estimated_rating, 2),
                                'similarity_basis': 'feature_similarity'
                            })

                            if len(recommendations) >= n_recommendations:
                                break
            else:
                # If user has not rated any quizzes, return popular ones based on overall ratings
                recommendations = [r for r in self.get_popular_quizzes(n_recommendations)]

        # Print the predicted ratings for KNN-based recommendations
        print(f"\nKNN-Based Recommendations for User {user_id}:")
        for i, rec in enumerate(recommendations[:n_recommendations], 1):
            print(f"  {i}. Quiz ID: {rec['quiz_id']} | Predicted Rating: {rec['predicted_rating']}")

        # Log the recommended quiz IDs
        recommended_quiz_ids = [rec['quiz_id'] for rec in recommendations[:n_recommendations]]
        logger.info(f"Recommended quizzes for user {user_id} (KNN features): {recommended_quiz_ids}")
        print(f"Recommended quiz IDs from database for user {user_id} (KNN features): {recommended_quiz_ids}")

        return {
            'user_id': user_id,
            'recommendations': recommendations[:n_recommendations],
            'timestamp': datetime.now().isoformat()
        }

    def _estimate_rating_from_similarity(self, rated_quiz_ids, target_quiz_id):
        """
        Estimate a rating for a target quiz based on similarity to user's rated quizzes
        """
        # This is a simplified estimation - in practice, you might want to implement
        # a more sophisticated approach considering actual similarities
        if not rated_quiz_ids:
            return 3.0  # Default neutral rating

        # Calculate average rating from user's high ratings
        if self.user_item_matrix is not None:
            # Get the first user ID that appears in the matrix to reference for ratings
            # Actually, we should focus on the user's own ratings passed in
            # But for this function, we'll just return an average of high ratings
            high_ratings = []
            for quiz_id in rated_quiz_ids:
                # Since we already have the user_ratings from the calling function
                # We can just use them or return a baseline
                pass

        # Default fallback
        return 4.0

    def get_user_played_quizzes(self, user_id: int) -> list:
        """
        Get quizzes that the user has played from the StudentQuizzes table
        """
        logger.info(f"Getting played quizzes for user {user_id}")

        # Query to get quizzes the user has started or completed
        query = """
        SELECT DISTINCT
            sq.QuizId as quiz_id
        FROM StudentQuizzes sq
        WHERE sq.StudentId = ?
        AND sq.Status IN ('Completed', 'Started', 'Exited', 'AutoSubmitted')
        """

        try:
            # Use pandas' parameter format which works with both SQLite and SQL Server
            df = pd.read_sql_query(query, self.engine, params=(user_id,))
            played_quiz_ids = df['quiz_id'].tolist() if not df.empty else []
            logger.info(f"User {user_id} has played {len(played_quiz_ids)} quizzes: {played_quiz_ids}")
            return played_quiz_ids
        except Exception as e:
            logger.error(f"Error getting played quizzes for user {user_id}: {str(e)}")
            return []

    def get_quiz_categories(self, quiz_ids: list) -> dict:
        """
        Get categories for given quiz IDs from QuizCategory and Categories tables
        """
        if not quiz_ids:
            return {}

        # Create parameterized query to get quiz categories
        # Use a more SQL-agnostic approach since we can't easily parameterize IN clauses
        placeholders = ','.join([f"'{qid}'" for qid in quiz_ids])
        query = f"""
        SELECT
            qc.QuizId as quiz_id,
            c.Id as category_id,
            c.Name as category_name
        FROM QuizCategories qc
        JOIN Categories c ON qc.CategoryId = c.Id
        WHERE qc.QuizId IN ({placeholders})
        """

        try:
            df = pd.read_sql(text(query), self.engine)
            quiz_categories = {}
            for _, row in df.iterrows():
                quiz_id = str(row['quiz_id'])
                if quiz_id not in quiz_categories:
                    quiz_categories[quiz_id] = []
                quiz_categories[quiz_id].append({
                    'id': row['category_id'],
                    'name': row['category_name']
                })
            return quiz_categories
        except Exception as e:
            logger.error(f"Error getting quiz categories: {str(e)}")
            return {}

    def get_all_quizzes_with_content(self) -> pd.DataFrame:
        """
        Get all quizzes with their content features for vectorization
        """
        logger.info("Loading all quizzes with content features...")

        query = """
        SELECT
            q.Id as quiz_id,
            q.Name as name,
            q.Description as description,
            q.Level as level,
            q.TotalQuestions,
            q.TimeInMinutes,
            q.CreatedAt
        FROM Quizzes q
        WHERE q.IsActive = 1
        """

        try:
            quizzes_df = pd.read_sql(text(query), self.engine)
            logger.info(f"Loaded {len(quizzes_df)} quizzes with content features")
            return quizzes_df
        except Exception as e:
            logger.error(f"Error loading quizzes with content: {str(e)}")
            return pd.DataFrame()

    def get_all_quiz_categories(self) -> dict:
        """
        Get all quiz categories efficiently in one query
        """
        query = """
        SELECT
            qc.QuizId as quiz_id,
            c.Id as category_id,
            c.Name as category_name
        FROM QuizCategories qc
        JOIN Categories c ON qc.CategoryId = c.Id
        """

        try:
            df = pd.read_sql_query(query, self.engine)
            quiz_categories = {}
            for _, row in df.iterrows():
                quiz_id = str(row['quiz_id'])
                if quiz_id not in quiz_categories:
                    quiz_categories[quiz_id] = []
                quiz_categories[quiz_id].append({
                    'id': row['category_id'],
                    'name': row['category_name']
                })
            return quiz_categories
        except Exception as e:
            logger.error(f"Error getting all quiz categories: {str(e)}")
            return {}

    def vectorize_quizzes_content(self, quizzes_df: pd.DataFrame) -> tuple:
        """
        Create content-based feature vectors for quizzes based on their content using ML vectorization
        """
        if quizzes_df.empty:
            return np.array([]), []

        # Get all categories efficiently in one query
        all_quiz_categories = self.get_all_quiz_categories()

        # Create a combined content string for each quiz
        content_strings = []
        quiz_ids = []

        for _, row in quizzes_df.iterrows():
            quiz_id = str(row['quiz_id'])
            quiz_ids.append(quiz_id)

            # Combine all content features into a single string for TF-IDF vectorization
            name = str(row['name']) if row['name'] is not None else ''
            description = str(row['description']) if row['description'] is not None else ''
            level = str(row['level']) if row['level'] is not None else ''
            total_questions = str(row['TotalQuestions']) if row['TotalQuestions'] is not None else ''
            time_minutes = str(row['TimeInMinutes']) if row['TimeInMinutes'] is not None else ''

            # Get categories for this quiz from pre-loaded data
            category_names = [cat['name'] for cat in all_quiz_categories.get(quiz_id, [])]
            categories_str = ' '.join(category_names)

            # Combine all features with emphasis on different aspects
            content = f"{name} {name} {name} {description} {level} {level} {total_questions} {time_minutes} {categories_str} {categories_str}".lower()
            content_strings.append(content)

        # Use TF-IDF to vectorize the content with more features for better ML performance
        if content_strings:
            # Increase max_features to capture more nuanced content differences
            tfidf = TfidfVectorizer(
                stop_words='english',
                max_features=200,  # Increased for better representation
                ngram_range=(1, 2),  # Include bigrams for better context
                lowercase=True,
                strip_accents='unicode'
            )
            content_matrix = tfidf.fit_transform(content_strings)
            return content_matrix.toarray(), quiz_ids
        else:
            return np.array([]), []


    def content_based_recommend_with_clustering(self, user_id: int, n_recommendations: int = 5) -> list:
        """
        Category-based content filtering using TF-IDF and similarity calculation based only on quiz categories
        """
        logger.info(f"Generating category-based content recommendations for user {user_id}")

        # 1. Get quizzes the user has played from StudentQuizzes table
        played_quiz_ids = self.get_user_played_quizzes(user_id)
        if not played_quiz_ids:
            logger.info(f"User {user_id} has not played any quizzes, returning popular quizzes")
            return self.get_popular_quizzes(n_recommendations)

        # 2. Get all quiz-category relationships from QuizCategories and Categories tables
        all_quiz_categories = self.get_all_quiz_categories()
        if not all_quiz_categories:
            logger.warning("No quiz-category relationships found for content-based recommendation")
            return self.get_popular_quizzes(n_recommendations)

        # 3. Create a list of all unique quiz IDs that have categories
        all_quiz_ids = list(all_quiz_categories.keys())

        # 4. Create content strings based only on categories for each quiz
        content_strings = []
        quiz_ids = []

        for quiz_id in all_quiz_ids:
            # Get all category names for this quiz
            categories = all_quiz_categories.get(quiz_id, [])
            category_names = [cat['name'] for cat in categories]

            # Create a content string with repeated category names to emphasize importance
            category_string = ' '.join(category_names) if category_names else 'uncategorized'
            content_string = f"{category_string} {category_string} {category_string}"  # Repeat to emphasize

            content_strings.append(content_string.lower())
            quiz_ids.append(quiz_id)

        # 5. Use TF-IDF to vectorize the category-based content
        if content_strings:
            # Use TF-IDF to create feature vectors based only on categories
            tfidf = TfidfVectorizer(
                stop_words='english',
                max_features=100,
                ngram_range=(1, 1),
                lowercase=True,
                strip_accents='unicode'
            )
            content_matrix = tfidf.fit_transform(content_strings)
            content_vectors = content_matrix.toarray()
        else:
            logger.warning("No content vectors generated for category-based recommendation")
            return self.get_popular_quizzes(n_recommendations)

        # 6. Get vectors for played quizzes
        played_quiz_indices = []
        played_quiz_vectors = []
        for i, quiz_id in enumerate(quiz_ids):
            if quiz_id in played_quiz_ids:
                played_quiz_indices.append(i)
                played_quiz_vectors.append(content_vectors[i])

        if not played_quiz_vectors:
            logger.warning("No category vectors found for played quizzes")
            return self.get_popular_quizzes(n_recommendations)

        # 7. Use K-means clustering to group quizzes by category similarity
        from sklearn.cluster import KMeans

        # Determine optimal number of clusters (based on number of unique categories)
        n_clusters = min(max(3, len(set(content_strings)) // 2), 10)
        kmeans = KMeans(n_clusters=n_clusters, random_state=42, n_init=10)
        cluster_labels = kmeans.fit_predict(content_vectors)

        # 8. Find which clusters the user's played quizzes belong to
        user_cluster_indices = set()
        for idx in played_quiz_indices:
            user_cluster_indices.add(cluster_labels[idx])

        # 9. Calculate user profile as the average of played quiz vectors based on categories
        user_profile = np.mean(played_quiz_vectors, axis=0).reshape(1, -1)

        # 10. Calculate similarities within user's preferred clusters
        recommendations = []
        for cluster_idx in user_cluster_indices:
            # Get all quizzes in this cluster
            cluster_quiz_indices = [i for i, label in enumerate(cluster_labels) if label == cluster_idx]

            # Calculate similarities for quizzes in this cluster
            cluster_vectors = content_vectors[cluster_quiz_indices]
            cluster_quiz_ids = [quiz_ids[i] for i in cluster_quiz_indices]

            # Calculate similarity to user profile
            cluster_similarities = cosine_similarity(user_profile, cluster_vectors)[0]

            # Create similarity pairs
            cluster_similarities_pairs = [(cluster_quiz_ids[i], cluster_similarities[i]) for i in range(len(cluster_quiz_ids))]

            # Sort by similarity
            cluster_similarities_pairs.sort(key=lambda x: x[1], reverse=True)

            # Add top similar quizzes from this cluster that user hasn't played
            for quiz_id, similarity_score in cluster_similarities_pairs:
                if (quiz_id not in played_quiz_ids and
                    len(recommendations) < n_recommendations and
                    quiz_id not in [rec['quiz_id'] for rec in recommendations]):

                    recommendations.append({
                        'quiz_id': quiz_id,
                        'predicted_rating': round(similarity_score * 5.0, 2),
                        'similarity_score': round(similarity_score, 3),
                        'method': 'category_based_content_filtering'
                    })

                    if len(recommendations) >= n_recommendations:
                        break

            if len(recommendations) >= n_recommendations:
                break

        # 11. If we don't have enough recommendations, fall back to general similarity approach
        if len(recommendations) < n_recommendations:
            # Calculate overall similarities for remaining slots
            similarities = cosine_similarity(user_profile, content_vectors)[0]
            quiz_similarities = [(quiz_ids[i], similarities[i]) for i in range(len(quiz_ids))]
            quiz_similarities.sort(key=lambda x: x[1], reverse=True)

            for quiz_id, similarity_score in quiz_similarities:
                if (quiz_id not in played_quiz_ids and
                    quiz_id not in [rec['quiz_id'] for rec in recommendations]):

                    recommendations.append({
                        'quiz_id': quiz_id,
                        'predicted_rating': round(similarity_score * 5.0, 2),
                        'similarity_score': round(similarity_score, 3),
                        'method': 'category_based_content_filtering_fallback'
                    })

                    if len(recommendations) >= n_recommendations:
                        break

        # Print the predicted ratings for debugging/output
        print(f"\nCategory-Based Content Recommendations for User {user_id}:")
        for i, rec in enumerate(recommendations, 1):
            print(f"  {i}. Quiz ID: {rec['quiz_id']} | Predicted Rating: {rec['predicted_rating']} | Similarity Score: {rec['similarity_score']} | Method: {rec['method']}")

        logger.info(f"Generated {len(recommendations)} category-based content recommendations for user {user_id}")
        return recommendations



# Flask app setup
app = Flask(__name__)
CORS(app)

# Configuration
try:
    from config import DATABASE_CONNECTION_STRING
except ImportError:
    # Fallback to environment variable or default
    DATABASE_CONNECTION_STRING = os.getenv('DATABASE_CONNECTION_STRING',
        'mssql+pyodbc://@./BlazingQuiz'
        '?driver=ODBC+Driver+17+for+SQL+Server'
        '&trusted_connection=yes')  # Default SQL Server connection

# Initialize recommendation system
rec_system = QuizRecommendationSystem(
    connection_string=DATABASE_CONNECTION_STRING,
    k_neighbors=5
)


@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        'status': 'healthy',
        'timestamp': datetime.now().isoformat()
    })


@app.route('/api/recommendations/<int:user_id>', methods=['GET'])
def get_recommendations(user_id):
    """Get quiz recommendations for a specific user"""
    try:
        n_recommendations = int(request.args.get('n', 5))  # Number of recommendations, default 5
        method = request.args.get('method', 'collaborative_filtering')  # Method: content_based_advanced, collaborative_filtering

        # Use the appropriate recommendation method
        if method == 'content_based_advanced':
            recommendations_data = rec_system.content_based_recommend_with_clustering(user_id, n_recommendations)
            # Format the response to match expected structure
            recommendations_data = {
                'user_id': user_id,
                'recommendations': recommendations_data,
                'timestamp': datetime.now().isoformat()
            }
        elif method == 'collaborative_filtering':
            recommendations_data = rec_system.get_user_recommendations_with_knn(user_id, n_recommendations)
        else:
            # Default to collaborative filtering method
            recommendations_data = rec_system.get_user_recommendations_with_knn(user_id, n_recommendations)

        # Add detailed quiz information to recommendations
        if 'recommendations' in recommendations_data:
            quiz_ids = [rec['quiz_id'] for rec in recommendations_data['recommendations']]
            quiz_details = rec_system.get_quiz_details(quiz_ids)

            # Create a map of quiz_id to details
            details_map = {str(detail['quiz_id']): detail for detail in quiz_details}

            # Add details to each recommendation
            for rec in recommendations_data['recommendations']:
                quiz_id = rec['quiz_id']
                if quiz_id in details_map:
                    rec['quiz_details'] = details_map[quiz_id]
                else:
                    rec['quiz_details'] = None

        return jsonify(recommendations_data)

    except Exception as e:
        logger.error(f"Error getting recommendations for user {user_id}: {str(e)}")
        return jsonify({'error': str(e)}), 500


@app.route('/api/recommendations/batch', methods=['POST'])
def get_batch_recommendations():
    """Get quiz recommendations for multiple users"""
    try:
        data = request.get_json()
        user_ids = data.get('user_ids', [])
        n_recommendations = data.get('n', 5)
        method = data.get('method', 'collaborative_filtering')  # Method: content_based_advanced, collaborative_filtering

        batch_recommendations = {}
        for user_id in user_ids:
            # Use the appropriate recommendation method
            if method == 'content_based_advanced':
                recommendations = rec_system.content_based_recommend_with_clustering(user_id, n_recommendations)
                # Format the response to match expected structure
                recommendations = {
                    'user_id': user_id,
                    'recommendations': recommendations,
                    'timestamp': datetime.now().isoformat()
                }
            elif method == 'collaborative_filtering':
                recommendations = rec_system.get_user_recommendations_with_knn(user_id, n_recommendations)
            else:
                # Default to collaborative filtering method
                recommendations = rec_system.get_user_recommendations_with_knn(user_id, n_recommendations)

            # Add detailed quiz information to recommendations
            if 'recommendations' in recommendations:
                quiz_ids = [rec['quiz_id'] for rec in recommendations['recommendations']]
                quiz_details = rec_system.get_quiz_details(quiz_ids)

                # Create a map of quiz_id to details
                details_map = {str(detail['quiz_id']): detail for detail in quiz_details}

                # Add details to each recommendation
                for rec in recommendations['recommendations']:
                    quiz_id = rec['quiz_id']
                    if quiz_id in details_map:
                        rec['quiz_details'] = details_map[quiz_id]
                    else:
                        rec['quiz_details'] = None

            batch_recommendations[user_id] = recommendations

        return jsonify({
            'batch_recommendations': batch_recommendations,
            'timestamp': datetime.now().isoformat()
        })

    except Exception as e:
        logger.error(f"Error getting batch recommendations: {str(e)}")
        return jsonify({'error': str(e)}), 500


def continuous_update():
    """
    Function to continuously update recommendations every 5 minutes
    """
    global rec_system

    # Update recommendations initially
    try:
        logger.info("Performing initial recommendation system update...")
        rec_system.update_recommendations()
    except Exception as e:
        logger.error(f"Error during initial update: {str(e)}")

    while True:
        try:
            # Wait 5 minutes before next update
            time.sleep(5 * 60)

            # Update the recommendation system with latest data
            logger.info("Starting periodic recommendation system update...")
            rec_system.update_recommendations()
            logger.info("Periodic recommendation system update completed.")

        except Exception as e:
            logger.error(f"Error in continuous update process: {str(e)}")
            # Sleep for 1 minute before retrying if there's an error
            time.sleep(60)


def start_continuous_updater():
    """
    Start the continuous update process in a separate thread
    """
    updater_thread = threading.Thread(target=continuous_update, daemon=True)
    updater_thread.start()

    logger.info("Continuous updater thread started.")


if __name__ == '__main__':
    # Start the continuous updater in a background thread
    start_continuous_updater()

    # Start Flask app
    port = int(os.getenv('PORT', 5000))
    logger.info(f"Starting Flask app on port {port}")
    app.run(host='0.0.0.0', port=port, debug=False, threaded=True)