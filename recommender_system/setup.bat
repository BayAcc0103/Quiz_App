@echo off
REM Setup script for Quiz Recommendation System

echo Setting up Quiz Recommendation System...

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo Python is not installed. Please install Python first.
    pause
    exit /b 1
)

REM Check if requirements are installed
echo Installing required packages...
pip install -r requirements.txt

REM Instructions for user
echo.
echo Setup complete!
echo.
echo To run the recommendation system:
echo   1. Set your DATABASE_CONNECTION_STRING environment variable
echo      Example: set DATABASE_CONNECTION_STRING=sqlite:///quiz_app.db
echo   2. Run: python quiz_recommendation_system.py
echo.
echo To test the system:
echo   Run: python test_recommendation_system.py
echo.

pause