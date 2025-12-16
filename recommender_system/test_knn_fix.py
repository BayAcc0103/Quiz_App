import sys
import os
import logging
logging.basicConfig(level=logging.INFO)

sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from quiz_recommendation_system import QuizRecommendationSystem

print('Testing KNN model initialization...')

# Initialize the recommendation system
rec_system = QuizRecommendationSystem('sqlite:///quiz_app.db', k_neighbors=3)

# Load data and update the system
df = rec_system.load_data()
print(f'Loaded {len(df)} ratings')

if not df.empty:
    print(f'Sample of loaded data:')
    print(df.head())
    
    # Build user-item matrix
    rec_system.build_user_item_matrix(df)
    print(f'User-item matrix: {rec_system.user_item_matrix.shape}')
    
    # Compute item similarities
    rec_system.compute_item_similarity()
    print('Item similarities computed')
    
    # Fit KNN model - this is where the error was occurring
    try:
        print('Attempting to fit KNN model...')
        rec_system.fit_knn_model(df)
        print('KNN model fitted successfully!')
        
        # Check if the attributes exist
        if hasattr(rec_system, 'knn_model'):
            print('OK - knn_model attribute exists')
        else:
            print('ERROR - knn_model attribute missing')

        if hasattr(rec_system, 'scaler'):
            print('OK - scaler attribute exists')
        else:
            print('ERROR - scaler attribute missing')

        if hasattr(rec_system, 'quiz_ids'):
            print(f'OK - quiz_ids attribute exists ({len(rec_system.quiz_ids)} quizzes)')
        else:
            print('ERROR - quiz_ids attribute missing')

        if hasattr(rec_system, 'quiz_features'):
            print(f'OK - quiz_features attribute exists')
        else:
            print('ERROR - quiz_features attribute missing')
    except Exception as e:
        print(f'Error in fit_knn_model: {e}')
        import traceback
        traceback.print_exc()
else:
    print('No data available for testing')