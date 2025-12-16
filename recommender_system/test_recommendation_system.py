"""
Test script for the Quiz Recommendation System
"""
import os
import sys
import requests
import time
import threading

# Add the recommender system to the path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from quiz_recommendation_system import QuizRecommendationSystem

def test_with_mock_data():
    """
    Test the recommendation system with mock database connection
    This is for testing purposes; you'll need a real database connection for actual use
    """
    print("Testing Quiz Recommendation System...")
    
    # Mock connection string (this is just for testing the structure)
    # In real usage, connect to your actual database
    connection_string = os.getenv('DATABASE_CONNECTION_STRING', 'sqlite:///quiz_app_test.db')
    
    # Initialize the recommendation system
    rec_system = QuizRecommendationSystem(connection_string, k_neighbors=3)
    
    print("Initialized recommendation system")
    
    # Try to load data (will fail without real database)
    try:
        df = rec_system.load_data()
        print(f"Loaded {len(df)} ratings")
        
        if not df.empty:
            # Test building user-item matrix
            rec_system.build_user_item_matrix(df)
            print(f"Built user-item matrix: {rec_system.user_item_matrix.shape}")
            
            # Test computing similarities
            rec_system.compute_item_similarity()
            print("Computed item similarities")
            
            # Test KNN model
            rec_system.fit_knn_model(df)
            print("Trained KNN model")
            
            # Try to get recommendations for a user (if any exist)
            if not rec_system.user_item_matrix.empty:
                sample_user = rec_system.user_item_matrix.index[0] if len(rec_system.user_item_matrix.index) > 0 else 1
                recommendations = rec_system.recommend_for_user(sample_user, 5)
                print(f"Sample recommendations for user {sample_user}: {recommendations}")
        else:
            print("No ratings data found in database")
            
    except Exception as e:
        print(f"Expected error with mock database: {e}")
        print("This is expected when no actual database connection exists")

def test_api_endpoints():
    """
    Test the API endpoints if the Flask server is running
    """
    base_url = "http://localhost:5000"
    
    # Test health endpoint
    try:
        response = requests.get(f"{base_url}/health")
        if response.status_code == 200:
            print("✓ Health check passed")
            print(f"Response: {response.json()}")
        else:
            print(f"✗ Health check failed with status {response.status_code}")
    except requests.exceptions.ConnectionError:
        print("✗ Flask server not running. Start the server to test API endpoints.")
    
    # Test recommendations endpoint (this will return empty without real data)
    try:
        response = requests.get(f"{base_url}/api/recommendations/1?n=3")
        if response.status_code == 200:
            print("✓ Recommendations endpoint accessible")
            data = response.json()
            print(f"Recommendations: {data}")
        else:
            print(f"✗ Recommendations endpoint failed with status {response.status_code}")
    except requests.exceptions.ConnectionError:
        print("✗ Flask server not running. Start the server to test recommendations endpoint.")

if __name__ == "__main__":
    print("=== Quiz Recommendation System Test ===")
    
    # Test the system structure
    test_with_mock_data()
    
    print("\n=== API Testing ===")
    # Test API endpoints
    test_api_endpoints()
    
    print("\n=== Test Completed ===")
    print("Note: For full functionality, you need to:")
    print("1. Set up the correct DATABASE_CONNECTION_STRING environment variable")
    print("2. Ensure your database contains Quiz and QuizFeedback tables with data")
    print("3. Run the main application using: python quiz_recommendation_system.py")