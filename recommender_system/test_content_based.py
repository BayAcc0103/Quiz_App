"""
Test script to verify the content-based filtering implementation
"""
import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from quiz_recommendation_system import QuizRecommendationSystem
import pandas as pd

def test_content_based_filtering():
    """
    Test the content-based filtering implementation
    """
    # Use a test database connection string (you may need to adjust this)
    connection_string = 'sqlite:///quiz_app.db'  # Adjust to your database connection
    
    # Initialize the recommendation system
    rec_system = QuizRecommendationSystem(connection_string=connection_string)
    
    try:
        # Test loading data
        print("Testing data loading...")
        df = rec_system.load_data()
        print(f"Loaded {len(df)} rating records")
        
        # Test content-based recommendations
        print("\nTesting content-based recommendations...")
        user_id = 1  # Test with a sample user ID
        content_recs = rec_system.content_based_recommend(user_id, n_recommendations=5)
        print(f"Generated {len(content_recs)} content-based recommendations")
        for rec in content_recs:
            print(f"  Quiz ID: {rec['quiz_id']}, Rating: {rec['predicted_rating']}, Method: {rec['method']}")
        
        # Test hybrid recommendations
        print("\nTesting hybrid recommendations...")
        hybrid_recs = rec_system.hybrid_recommend(user_id, n_recommendations=5)
        print(f"Generated {len(hybrid_recs['recommendations'])} hybrid recommendations")
        for rec in hybrid_recs['recommendations']:
            print(f"  Quiz ID: {rec['quiz_id']}, Rating: {rec['predicted_rating']}, Method: {rec['method']}")
        
        print("\nAll tests passed successfully!")
        return True
        
    except Exception as e:
        print(f"Test failed with error: {str(e)}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    test_content_based_filtering()