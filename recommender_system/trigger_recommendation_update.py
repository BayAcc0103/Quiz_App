"""
Utility script to trigger recommendation updates
This script can be called from the .NET application after a user rates a quiz
"""
import sys
import os
import argparse
import requests
from typing import Optional

# Add the parent directory to the path to import the recommendation engine
sys.path.append(os.path.dirname(__file__))

from recommendation_engine import update_recommendations_after_rating

def trigger_local_recommendation_update(user_id: int, db_connection_string: Optional[str] = None):
    """
    Trigger recommendation update locally using the Python engine
    """
    if not db_connection_string:
        db_connection_string = os.environ.get('DB_CONNECTION_STRING')
    
    if not db_connection_string:
        raise ValueError("Database connection string is required")
    
    try:
        recommendations = update_recommendations_after_rating(user_id, db_connection_string)
        print(f"Successfully updated recommendations for user {user_id}. Found {len(recommendations)} recommendations.")
        return recommendations
    except Exception as e:
        print(f"Error updating recommendations: {e}")
        return None


def trigger_api_recommendation_update(user_id: int, api_url: str = "http://localhost:8000"):
    """
    Trigger recommendation update via API call
    """
    try:
        url = f"{api_url}/rating-submitted/{user_id}"
        response = requests.post(url, timeout=30)
        
        if response.status_code == 200:
            result = response.json()
            print(f"API call successful: {result}")
            return result
        else:
            print(f"API call failed with status {response.status_code}: {response.text}")
            return None
    except Exception as e:
        print(f"Error calling recommendation API: {e}")
        return None


def main():
    parser = argparse.ArgumentParser(description="Trigger recommendation update after user rating")
    parser.add_argument("--user-id", type=int, required=True, help="User ID")
    parser.add_argument("--method", choices=["local", "api"], default="local", 
                       help="Method to use for updating recommendations")
    parser.add_argument("--api-url", default="http://localhost:8000", 
                       help="API URL if using API method")
    parser.add_argument("--connection-string", 
                       help="Database connection string (for local method)")
    
    args = parser.parse_args()
    
    if args.method == "local":
        trigger_local_recommendation_update(args.user_id, args.connection_string)
    elif args.method == "api":
        trigger_api_recommendation_update(args.user_id, args.api_url)


if __name__ == "__main__":
    main()