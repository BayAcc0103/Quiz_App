import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from quiz_recommendation_system import QuizRecommendationSystem

print('Testing the specific method that was failing...')

# Initialize the recommendation system
rec_system = QuizRecommendationSystem('sqlite:///quiz_app.db', k_neighbors=3)

# Update the system to ensure everything is initialized
rec_system.update_recommendations()
print('Recommendation system updated')

# Now test the method that was failing
try:
    recommendations = rec_system.get_user_recommendations_with_knn(2, 10)
    print(f'Success! Got recommendations for user 2:')
    print(f'Number of recommendations: {len(recommendations["recommendations"])}')
    print(f'Sample recommendation: {recommendations["recommendations"][0] if recommendations["recommendations"] else "No recommendations"}')
    print('API endpoint should now work properly!')
except Exception as e:
    import traceback
    print(f'Error: {e}')
    traceback.print_exc()