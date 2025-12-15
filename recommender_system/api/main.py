from fastapi import FastAPI, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
import os
import sys
import logging

# Add the parent directory to the path to import the recommendation engine
sys.path.append(os.path.dirname(os.path.dirname(__file__)))

from recommendation_engine import (
    QuizRecommendationEngine,
    update_recommendations_after_rating,
    initialize_database
)

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="Quiz Recommendation API", version="1.0.0")

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Adjust this in production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic models
class RatingUpdateRequest(BaseModel):
    user_id: int
    quiz_id: str  # GUID as string
    score: Optional[int] = None  # Rating score (1-5) or None if only updating

class RecommendationResponse(BaseModel):
    quiz_id: str
    predicted_rating: float
    rank: int

class RecommendationsResponse(BaseModel):
    user_id: int
    recommendations: List[RecommendationResponse]

class QuizDetailResponse(BaseModel):
    quiz_id: str
    name: str
    description: Optional[str] = None
    category_id: Optional[int] = None
    total_questions: int
    time_in_minutes: int
    level: Optional[str] = None
    predicted_rating: float

class DetailedRecommendationsResponse(BaseModel):
    user_id: int
    recommendations: List[QuizDetailResponse]

# Configuration
DB_CONNECTION_STRING = os.getenv('DB_CONNECTION_STRING')

@app.on_event("startup")
async def startup_event():
    """Initialize the database when the app starts"""
    if DB_CONNECTION_STRING:
        initialize_database()
        logger.info("Database initialized")

@app.post("/update-recommendations/{user_id}", response_model=RecommendationsResponse)
async def update_user_recommendations(user_id: int):
    """
    Trigger recommendation update for a specific user after they have rated a quiz
    This endpoint should be called after a user submits a rating
    """
    if not DB_CONNECTION_STRING:
        raise HTTPException(status_code=500, detail="Database connection string not configured")
    
    try:
        recommendations = update_recommendations_after_rating(user_id, DB_CONNECTION_STRING)
        
        # Format the response
        formatted_recs = [
            RecommendationResponse(
                quiz_id=rec[0],
                predicted_rating=rec[1],
                rank=i+1
            ) for i, rec in enumerate(recommendations)
        ]
        
        return RecommendationsResponse(
            user_id=user_id,
            recommendations=formatted_recs
        )
    except Exception as e:
        logger.error(f"Error updating recommendations for user {user_id}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error updating recommendations: {str(e)}")

@app.get("/recommendations/{user_id}", response_model=List[QuizDetailResponse])
async def get_user_recommendations(user_id: int):
    """
    Get the current recommendations for a specific user
    """
    if not DB_CONNECTION_STRING:
        raise HTTPException(status_code=500, detail="Database connection string not configured")
    
    try:
        # Connect to the database
        recommender = QuizRecommendationEngine(DB_CONNECTION_STRING)
        recommender.connect_to_db()
        
        # Get recommendations from the RecommendedQuizzes table
        query = """
        SELECT 
            r.QuizId,
            r.PredictedRating,
            q.Name,
            q.Description,
            q.CategoryId,
            q.TotalQuestions,
            q.TimeInMinutes,
            q.Level
        FROM RecommendedQuizzes r
        LEFT JOIN Quizzes q ON r.QuizId = q.Id
        WHERE r.UserId = :user_id
        ORDER BY r.PredictedRating DESC
        """
        
        with recommender.engine.connect() as conn:
            results = conn.execute(query, {"user_id": user_id}).fetchall()
        
        recommendations = []
        for i, row in enumerate(results):
            recommendations.append(QuizDetailResponse(
                quiz_id=row[0],
                predicted_rating=float(row[1]) if row[1] else 0.0,
                name=row[2] or "Unnamed Quiz",
                description=row[3],
                category_id=row[4],
                total_questions=row[5] or 0,
                time_in_minutes=row[6] or 0,
                level=row[7]
            ))
        
        return recommendations
    except Exception as e:
        logger.error(f"Error getting recommendations for user {user_id}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error retrieving recommendations: {str(e)}")

@app.post("/rating-submitted/{user_id}")
async def handle_rating_submission(user_id: int, request: RatingUpdateRequest):
    """
    This endpoint is called when a user submits a rating for a quiz.
    It triggers the entire flow: storing the feedback and updating recommendations.
    """
    try:
        # Update recommendations based on the new rating
        recommendations = update_recommendations_after_rating(user_id, DB_CONNECTION_STRING)
        logger.info(f"Successfully updated recommendations for user {user_id}")
        
        return {
            "message": f"Rating processed successfully for user {user_id}",
            "recommendations_count": len(recommendations),
            "status": "success"
        }
    except Exception as e:
        logger.error(f"Error processing rating for user {user_id}: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error processing rating: {str(e)}")

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "Quiz Recommendation API"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)