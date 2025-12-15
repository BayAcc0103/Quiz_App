import numpy as np
import pandas as pd
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.neighbors import NearestNeighbors
from sqlalchemy import create_engine, text
import os
import json
from datetime import datetime


class QuizRecommendationEngine:
    """
    Recommendation engine using Item-based Collaborative Filtering + KNN algorithm
    """
    
    def __init__(self, db_connection_string):
        """
        Initialize the recommendation engine
        :param db_connection_string: SQL Server connection string
        """
        self.db_connection_string = db_connection_string
        self.engine = None
        self.rating_matrix = None
        self.quiz_features = None
        self.user_profiles = None
        
    def connect_to_db(self):
        """Establish connection to the database"""
        self.engine = create_engine(self.db_connection_string, echo=False)
        
    def load_quiz_feedback_data(self):
        """
        Load quiz feedback data from database
        Returns a DataFrame with columns: student_id, quiz_id, score
        """
        query = """
        SELECT 
            CAST(StudentId AS INTEGER) as student_id,
            CAST(QuizId AS VARCHAR) as quiz_id,
            CAST(Score AS FLOAT) as score
        FROM QuizFeedbacks 
        WHERE Score IS NOT NULL AND Score > 0
        """
        
        df = pd.read_sql(query, self.engine)
        return df
    
    def build_rating_matrix(self):
        """
        Build user-quiz rating matrix where rows are users and columns are quizzes
        """
        feedback_df = self.load_quiz_feedback_data()
        
        # Create pivot table for rating matrix
        self.rating_matrix = feedback_df.pivot(
            index='student_id',
            columns='quiz_id',
            values='score'
        ).fillna(0)
        
        print(f"Rating matrix shape: {self.rating_matrix.shape}")
        return self.rating_matrix
    
    def compute_item_similarity(self, k_neighbors=10):
        """
        Compute item-to-item similarity using cosine similarity and KNN
        """
        if self.rating_matrix is None:
            self.build_rating_matrix()
        
        # Transpose matrix to get quiz-based view (each row is a quiz with ratings from all users)
        quiz_rating_matrix = self.rating_matrix.T  # Shape: (n_quizzes, n_users)
        
        # Calculate cosine similarity between quizzes
        similarity_matrix = cosine_similarity(quiz_rating_matrix)
        
        # Create a DataFrame with the similarity matrix
        self.similarity_df = pd.DataFrame(
            similarity_matrix,
            index=self.rating_matrix.columns,
            columns=self.rating_matrix.columns
        )
        
        # Find k most similar quizzes for each quiz using NearestNeighbors
        knn_model = NearestNeighbors(n_neighbors=min(k_neighbors+1, len(self.rating_matrix.columns)), 
                                    metric='cosine', algorithm='brute')
        knn_model.fit(similarity_matrix)
        
        distances, indices = knn_model.kneighbors(similarity_matrix)
        
        # Store the K nearest neighbors (excluding the item itself)
        self.knn_similarities = {}
        for i, quiz_id in enumerate(self.rating_matrix.columns):
            similar_indices = indices[i][1:]  # Exclude the item itself (first neighbor)
            similar_quizzes = [self.rating_matrix.columns[idx] for idx in similar_indices]
            similarity_values = 1 - distances[i][1:]  # Convert distance to similarity (1 - distance)
            
            self.knn_similarities[quiz_id] = list(zip(similar_quizzes, similarity_values))
        
        return self.similarity_df
    
    def get_user_profile(self, user_id):
        """
        Calculate user profile based on their ratings
        """
        if self.rating_matrix is None:
            self.build_rating_matrix()
        
        if user_id not in self.rating_matrix.index:
            return None
            
        user_ratings = self.rating_matrix.loc[user_id]
        rated_quizzes = user_ratings[user_ratings > 0].index
        ratings_vector = user_ratings[rated_quizzes]
        
        # Create user profile based on ratings
        user_profile = {}
        for quiz_id in rated_quizzes:
            user_profile[quiz_id] = float(ratings_vector[quiz_id])
        
        return user_profile
    
    def predict_rating_for_user(self, user_id, quiz_id_to_predict, top_k_similar=5):
        """
        Predict rating for a specific user and quiz using item-based filtering
        """
        if self.rating_matrix is None or self.similarity_df is None:
            self.compute_item_similarity()
        
        if user_id not in self.rating_matrix.index:
            # Cold start problem: new user, return average rating
            all_ratings = self.rating_matrix.values.flatten()
            all_ratings = all_ratings[all_ratings > 0]  # Only consider non-zero ratings
            if len(all_ratings) > 0:
                return float(np.mean(all_ratings))
            else:
                return 3.0  # Default rating
        
        if quiz_id_to_predict not in self.rating_matrix.columns:
            # New quiz - cannot predict
            return None
        
        user_ratings = self.rating_matrix.loc[user_id]
        
        # Get similar quizzes and their similarities
        if quiz_id_to_predict in self.knn_similarities:
            similar_quizzes = self.knn_similarities[quiz_id_to_predict][:top_k_similar]
        else:
            return None
        
        # Calculate weighted prediction
        weighted_sum = 0
        similarity_sum = 0
        
        for similar_quiz_id, similarity in similar_quizzes:
            if similar_quiz_id in user_ratings.index and user_ratings[similar_quiz_id] > 0:
                weighted_sum += similarity * user_ratings[similar_quiz_id]
                similarity_sum += abs(similarity)
        
        if similarity_sum == 0:
            # If no rated similar quizzes, return user's average rating
            user_rated_items = user_ratings[user_ratings > 0]
            if len(user_rated_items) > 0:
                return float(user_rated_items.mean())
            else:
                return 3.0  # Default rating
        
        predicted_rating = weighted_sum / similarity_sum
        # Clip the rating to be within [1, 5] range
        predicted_rating = max(1.0, min(5.0, predicted_rating))
        
        return predicted_rating
    
    def get_recommendations_for_user(self, user_id, num_recommendations=10, min_rating_threshold=3.0):
        """
        Generate quiz recommendations for a specific user
        """
        if self.rating_matrix is None or self.similarity_df is None:
            self.compute_item_similarity()
        
        # Get all quizzes not yet rated by the user
        user_ratings = self.rating_matrix.loc[user_id] if user_id in self.rating_matrix.index else pd.Series([], dtype=float)
        unrated_quizzes = self.rating_matrix.columns[~self.rating_matrix.columns.isin(user_ratings[user_ratings > 0].index)].tolist()
        
        # Predict ratings for all unrated quizzes
        predictions = []
        for quiz_id in unrated_quizzes:
            predicted_rating = self.predict_rating_for_user(user_id, quiz_id)
            if predicted_rating is not None and predicted_rating >= min_rating_threshold:
                predictions.append((quiz_id, predicted_rating))
        
        # Sort by predicted rating in descending order
        predictions.sort(key=lambda x: x[1], reverse=True)
        
        # Return top N recommendations
        recommendations = predictions[:num_recommendations]
        
        return recommendations
    
    def update_user_ratings(self):
        """
        Recompute the rating matrix to include the latest feedback
        """
        self.build_rating_matrix()
        self.compute_item_similarity()
        
    def update_recommendations_for_user(self, user_id):
        """
        Update recommendations for a specific user
        """
        recommendations = self.get_recommendations_for_user(user_id)
        
        # Save recommendations to database
        self.save_recommendations_to_db(user_id, recommendations)
        
        return recommendations
    
    def save_recommendations_to_db(self, user_id, recommendations):
        """
        Save recommendations to RecommendedQuiz table in database
        """
        # Clear previous recommendations for the user
        clear_query = text("""
        IF OBJECT_ID('RecommendedQuizzes', 'U') IS NOT NULL
        DELETE FROM RecommendedQuizzes WHERE UserId = :user_id
        """)
        
        with self.engine.connect() as conn:
            trans = conn.begin()
            try:
                conn.execute(clear_query, {"user_id": int(user_id)})
                
                # Insert new recommendations
                if recommendations:
                    insert_query = text("""
                    INSERT INTO RecommendedQuizzes (UserId, QuizId, PredictedRating, CreatedAt)
                    VALUES (:user_id, :quiz_id, :predicted_rating, :created_at)
                    """)
                    
                    for quiz_id, predicted_rating in recommendations:
                        conn.execute(insert_query, {
                            "user_id": int(user_id),
                            "quiz_id": str(quiz_id),
                            "predicted_rating": float(predicted_rating),
                            "created_at": datetime.utcnow()
                        })
                
                trans.commit()
            except Exception as e:
                trans.rollback()
                print(f"Error saving recommendations: {e}")


def initialize_database():
    """
    Initialize the RecommendedQuizzes table if it doesn't exist
    """
    engine = create_engine(os.environ.get('DB_CONNECTION_STRING'))
    
    create_table_query = text("""
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RecommendedQuizzes' AND xtype='U')
    BEGIN
        CREATE TABLE RecommendedQuizzes (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            UserId INT NOT NULL,
            QuizId VARCHAR(255) NOT NULL,
            PredictedRating DECIMAL(3,2) NOT NULL,
            CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
            FOREIGN KEY (UserId) REFERENCES Users(Id)
        );
    END
    """)
    
    with engine.connect() as conn:
        conn.execute(create_table_query)


def update_recommendations_after_rating(user_id, db_connection_string=None):
    """
    Main function to be called after a user rates a quiz
    This updates the user's recommendation list based on their new rating
    """
    if db_connection_string is None:
        db_connection_string = os.environ.get('DB_CONNECTION_STRING')
    
    if not db_connection_string:
        raise ValueError("Database connection string is required")
    
    # Initialize the recommendation engine
    recommender = QuizRecommendationEngine(db_connection_string)
    recommender.connect_to_db()
    
    # Update rating matrix with latest data
    recommender.update_user_ratings()
    
    # Update recommendations for the specific user
    recommendations = recommender.update_recommendations_for_user(user_id)
    
    print(f"Updated recommendations for user {user_id}: {len(recommendations)} items")
    
    return recommendations


if __name__ == "__main__":
    # Example usage
    # Get connection string from environment or use default
    connection_string = os.environ.get('DB_CONNECTION_STRING', 
                                      'mssql+pyodbc:///?odbc_connect=your_connection_string_here')
    
    recommender = QuizRecommendationEngine(connection_string)
    recommender.connect_to_db()
    
    # Build rating matrix and compute similarities
    rating_matrix = recommender.build_rating_matrix()
    similarity_matrix = recommender.compute_item_similarity()
    
    # Example: Get recommendations for a user (replace with actual user ID)
    # user_id = 1
    # recommendations = recommender.get_recommendations_for_user(user_id, num_recommendations=5)
    # print(f"Recommendations for user {user_id}: {recommendations}")