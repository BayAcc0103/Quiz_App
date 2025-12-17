# Quiz Application with AI Recommendation System - Setup and Running Guide

## Overview
This guide provides step-by-step instructions to set up and run the quiz application with the new AI-based recommendation system.

## Project Structure
- BlazingQuiz.Web: Main Blazor WebAssembly application
- BlazingQuiz.Api: Backend API server
- recommender_system: Python recommendation service
- BlazingQuiz.Shared: Shared components and DTOs

## Prerequisites

### For .NET Application
- .NET 8 SDK
- Visual Studio 2022 or Visual Studio Code
- SQL Server (or SQL Server Express)

### For Python Recommendation System
- Python 3.8 or higher
- pip package manager

## Setup Instructions

### 1. Setting up the .NET Application

#### Step 1: Database Setup
1. Navigate to the `BlazingQuiz.Api` project
2. Open `appsettings.json` and configure your database connection string if needed
3. Run the Entity Framework migrations:
   ```bash
   cd BlazingQuiz.Web
   dotnet ef database update --project BlazingQuiz.Api
   ```

#### Step 2: Configure API Connection
1. Open `BlazingQuiz.Web\Program.cs`
2. Check and update the `ApiBaseUrl` variable if needed (default: `https://localhost:7048`)

#### Step 3: Install Dependencies
```bash
cd BlazingQuiz.Web
dotnet restore
```

#### Step 4: Run the Backend API
```bash
cd BlazingQuiz.Api
dotnet run
```
The API should start on `https://localhost:7048`

### 2. Setting up the Python Recommendation System

#### Step 1: Install Python Dependencies
Navigate to the `recommender_system` folder and run:
```bash
cd recommender_system
pip install -r requirements.txt
```

If there's no requirements.txt, install the needed packages:
```bash
pip install Flask flask-cors numpy pandas scikit-learn sqlalchemy pyodbc psycopg2-binary mysql-connector-python requests
```

#### Step 2: Configure Database Connection
1. Create a `config.py` file in the `recommender_system` folder:
```python
# recommender_system/config.py
DATABASE_CONNECTION_STRING = "your_database_connection_string_here"

# Example connection strings:
# SQL Server: "mssql+pyodbc://username:password@server:port/database?driver=ODBC+Driver+17+for+SQL+Server"
# PostgreSQL: "postgresql+psycopg2://username:password@host:port/database"
# MySQL: "mysql+pymysql://username:password@host:port/database"
# SQLite: "sqlite:///path/to/database.db"

FLASK_PORT = 5000
```

#### Step 3: Run the Python Recommendation Service
```bash
cd recommender_system
python quiz_recommendation_system.py
```
The service will start on `http://localhost:5000`

### 3. Running the Blazor Frontend

#### Step 1: Navigate to Frontend
```bash
cd BlazingQuiz.Web\BlazingQuiz.Web
```

#### Step 2: Run the Frontend Application
```bash
dotnet run
```
The frontend will start on `https://localhost:5001` or `http://localhost:5000`

## Complete Running Sequence

### Option 1: Running in Separate Terminals

1. **Terminal 1 - Database and API**:
```bash
# Run the API (includes database)
cd BlazingQuiz.Api
dotnet run
```

2. **Terminal 2 - Python Recommendation Service**:
```bash
cd recommender_system
python quiz_recommendation_system.py
```

3. **Terminal 3 - Frontend**:
```bash
cd BlazingQuiz.Web\BlazingQuiz.Web
dotnet run
```

### Option 2: Running with a Script

Create a `run_all.bat` script in the project root:

```batch
@echo off
echo Starting Quiz Application with Recommendation System...

echo Starting API Server...
start cmd /k "cd BlazingQuiz.Api && dotnet run"

timeout /t 5 /nobreak

echo Starting Python Recommendation Service...
start cmd /k "cd recommender_system && python quiz_recommendation_system.py"

timeout /t 5 /nobreak

echo Starting Frontend Application...
start cmd /k "cd BlazingQuiz.Web\BlazingQuiz.Web && dotnet run"

echo All services started!
pause
```

## Using the Recommendation System

### 1. User Flow
1. Login to the quiz application
2. Complete some quizzes to generate rating data
3. Navigate to a quiz result page
4. Click "View Recommended Quizzes" button
5. View AI-powered quiz recommendations

### 2. Recommendation System Features
- Item-Based Collaborative Filtering algorithm
- KNN (K-Nearest Neighbors) integration
- Personalized quiz recommendations
- Performance analysis dashboard
- Continuous learning (updates every 5 minutes)

## Troubleshooting

### Common Issues

#### Issue: Python Service Connection Refused
- **Symptom**: `net::ERR_CONNECTION_REFUSED` when loading recommendations
- **Solution**: Make sure the Python service is running on `http://localhost:5000`

#### Issue: Database Connection
- **Symptom**: Database connection errors
- **Solution**: Verify connection string in both the API and Python service

#### Issue: Frontend Cannot Connect to API
- **Symptom**: API calls fail
- **Solution**: Verify `ApiBaseUrl` in `Program.cs` matches running API server

#### Issue: CORS Errors
- **Symptom**: Cross-origin errors in browser console
- **Solution**: Ensure the Python service has CORS enabled (it should be in the code already)

## Environment Configuration

### Database Connection String Examples
- **SQL Server**: `Server=localhost;Database=QuizApp;Trusted_Connection=true;TrustServerCertificate=true;`
- **SQL Server with credentials**: `Server=localhost;Database=QuizApp;User Id=username;Password=password;TrustServerCertificate=true;`
- **SQLite**: `Data Source=quizapp.db`

### Port Configuration
- API Server: `https://localhost:7048` (default)
- Frontend: `https://localhost:5001` (default)
- Python Service: `http://localhost:5000` (default)

## Important Files
- `recommender_system/quiz_recommendation_system.py` - Main Python recommendation service
- `BlazingQuiz.Web/BlazingQuiz.Web/Pages/Student/QuizRecommendations.razor` - Recommendations page
- `BlazingQuiz.Web/BlazingQuiz.Web/Pages/Student/QuizResult.razor` - Results page with recommendation button

## API Endpoints

### Python Recommendation Service
- `GET /api/recommendations/{user_id}?n=5` - Get recommendations for user
- `GET /health` - Health check
- `POST /api/recommendations/batch` - Batch recommendations

### .NET Backend API
- The existing API endpoints for quiz management
- The Python service connects to the database to get ratings data