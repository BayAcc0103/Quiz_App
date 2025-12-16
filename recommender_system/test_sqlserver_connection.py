import sys
import os
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

# Try to import the config
try:
    from config import DATABASE_CONNECTION_STRING
    print(f"Using configured connection string: {DATABASE_CONNECTION_STRING}")
except ImportError:
    print("Config not available, using environment or default")
    DATABASE_CONNECTION_STRING = (
        "mssql+pyodbc://@./BlazingQuiz"
        "?driver=ODBC+Driver+17+for+SQL+Server"
        "&trusted_connection=yes"
    )

from quiz_recommendation_system import QuizRecommendationSystem

print('Testing connection to SQL Server database...')

try:
    # Initialize the recommendation system with SQL Server
    rec_system = QuizRecommendationSystem(DATABASE_CONNECTION_STRING, k_neighbors=3)
    print("QuizRecommendationSystem initialized successfully with SQL Server connection string")
    
    # Test that the engine was created properly
    print(f"Engine created with connection: {str(rec_system.engine.url).replace('@./BlazingQuiz', '@[SERVER_NAME]')}")
    
    # Try to load data (this will test the connection and queries)
    print("Attempting to load data...")
    df = rec_system.load_data()
    print(f"Data loading completed. Shape: {df.shape if not df.empty else '(empty)'}")
    
    if not df.empty:
        print("Data successfully loaded from SQL Server!")
        print(f"Sample data: {df.head()}")
    else:
        print("No data found (which is fine if the database is empty)")
        
    print("SQL Server configuration test completed successfully!")
    
except ImportError as e:
    print(f"Import error: {e}")
except Exception as e:
    print(f"Error connecting to SQL Server: {e}")
    import traceback
    traceback.print_exc()