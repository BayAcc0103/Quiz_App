# Quiz Recommendation System

This system implements a quiz recommendation engine using Item-Based Collaborative Filtering with KNN to suggest quizzes to users based on their ratings and preferences.

## Features

- Item-Based Collaborative Filtering algorithm to recommend quizzes based on user ratings
- KNN (K-Nearest Neighbors) integration to enhance recommendations using quiz content features
- Continuous learning with updates every 5 minutes
- RESTful API endpoints for integration with frontend applications
- Support for individual and batch recommendations

## Architecture

The system combines two recommendation approaches:

1. **Item-Based Collaborative Filtering**: Analyzes user-item rating matrix to find similarities between quizzes
2. **KNN with Content Features**: Uses quiz attributes (category, difficulty level, time, etc.) to find similar quizzes

## API Endpoints

### Get Recommendations for a User
```
GET /api/recommendations/<user_id>?n=<number_of_recommendations>
```
- `user_id`: The ID of the user to recommend quizzes for
- `n`: Optional, number of recommendations to return (default: 5)

### Get Batch Recommendations
```
POST /api/recommendations/batch
```
Request body:
```json
{
  "user_ids": [1, 2, 3],
  "n": 5
}
```

### Health Check
```
GET /health
```

## Installation

1. Install dependencies:
```bash
pip install -r requirements.txt
```

2. Set up environment variables:
```bash
export DATABASE_CONNECTION_STRING="your_database_connection_string"
export PORT=5000
```

## Running the System

```bash
python quiz_recommendation_system.py
```

The system will automatically:
- Load existing ratings from the database
- Build user-item matrix and compute item similarities
- Train KNN models on quiz features
- Start continuous updates every 5 minutes
- Start the Flask API server

## Configuration

- `DATABASE_CONNECTION_STRING`: Connection string for the database (supports SQL Server, PostgreSQL, MySQL)
- `PORT`: Port for the Flask server (default: 5000)
- `K_NEIGHBORS`: Number of neighbors for KNN algorithm (in code, default: 5)

## Response Format

A typical recommendation response includes:

```json
{
  "user_id": 123,
  "recommendations": [
    {
      "quiz_id": "quiz-guid-here",
      "predicted_rating": 4.5,
      "similar_rated_quiz_ids": [
        {
          "quiz_id": "rated-quiz-guid",
          "similarity": 0.85
        }
      ],
      "quiz_details": {
        "name": "Sample Quiz",
        "description": "Description of the quiz",
        "total_questions": 10,
        "time_in_minutes": 15,
        "level": "Medium",
        "category_id": 1
      }
    }
  ],
  "timestamp": "2025-12-16T10:30:00.123456"
}
```

## Database Schema Requirements

The system requires these tables:
- `Quiz` table with columns: Id (GUID), Name, Description, CategoryId, TotalQuestions, TimeInMinutes, Level, CreatedAt
- `QuizFeedback` table with columns: Id, StudentId, QuizId (GUID), Score, Comment, CreatedOn

## Algorithm Details

1. **Data Loading**: Fetches user ratings from QuizFeedback table
2. **User-Item Matrix**: Creates matrix of users vs quizzes with ratings
3. **Item Similarity**: Computes cosine similarities between quizzes based on user ratings
4. **KNN Features**: Uses quiz attributes to find similar content
5. **Recommendation**: Combines collaborative filtering and content-based approaches
6. **Continuous Learning**: Updates the model every 5 minutes with new data