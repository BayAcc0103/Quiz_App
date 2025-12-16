import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from config import DATABASE_CONNECTION_STRING
from quiz_recommendation_system import QuizRecommendationSystem

print('Testing full recommendation system with SQL Server...')

try:
    # Initialize the recommendation system
    rec_system = QuizRecommendationSystem(DATABASE_CONNECTION_STRING, k_neighbors=3)
    print("OK - System initialized with SQL Server connection")
    
    # Load and process data
    df = rec_system.load_data()
    print(f"OK - Loaded {len(df)} ratings from SQL Server")

    if not df.empty:
        # Build user-item matrix
        rec_system.build_user_item_matrix(df)
        print(f"OK - Built user-item matrix: {rec_system.user_item_matrix.shape}")

        # Compute item similarities
        rec_system.compute_item_similarity()
        print("OK - Item similarities computed")

        # Fit KNN model - this uses the fixed SQL Server compatible query
        rec_system.fit_knn_model(df)
        print("OK - KNN model fitted successfully")

        # Update the entire system
        rec_system.update_recommendations()
        print("OK - Recommendation system updated successfully")

        # Test getting recommendations
        if not rec_system.user_item_matrix.empty:
            sample_user_id = rec_system.user_item_matrix.index[0] if len(rec_system.user_item_matrix.index) > 0 else 2
            recommendations = rec_system.get_user_recommendations_with_knn(sample_user_id, 5)
            print(f"OK - Generated {len(recommendations.get('recommendations', []))} recommendations for user {sample_user_id}")
            print("OK - Full system test completed successfully!")
        else:
            print("INFO - No users in matrix to test recommendations")
    else:
        print("INFO - No data loaded from SQL Server")
        
except Exception as e:
    print(f"ERROR: {e}")
    import traceback
    traceback.print_exc()