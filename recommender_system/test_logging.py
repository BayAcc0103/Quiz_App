import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from config import DATABASE_CONNECTION_STRING
from quiz_recommendation_system import QuizRecommendationSystem

print('Testing recommendation logging...')

# Initialize the recommendation system
rec_system = QuizRecommendationSystem(DATABASE_CONNECTION_STRING, k_neighbors=3)

# Update the system to ensure everything is initialized
rec_system.update_recommendations()
print('Recommendation system updated')

# Test getting recommendations for a user
recommendations = rec_system.get_user_recommendations_with_knn(2, 5)
print(f'Got {len(recommendations["recommendations"])} recommendations')

# Test getting recommendations for user 1 as well
recommendations_user1 = rec_system.get_user_recommendations_with_knn(1, 5)
print(f'Got {len(recommendations_user1["recommendations"])} recommendations for user 1')