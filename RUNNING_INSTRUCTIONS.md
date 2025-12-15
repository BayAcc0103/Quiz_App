# Quiz Application with Recommendation System - Running Instructions

This guide provides step-by-step instructions to run the complete quiz application with the integrated recommendation system.

## Prerequisites

- .NET 8 SDK or later
- Python 3.8 or later
- SQL Server (or SQL Server Express)
- Node.js (for any frontend dependencies, if needed)

## Setup Instructions

### 1. Database Setup

1. Update your connection string in `appsettings.json` in the `BlazingQuiz.Api` project:
   ```json
   {
     "ConnectionStrings": {
       "QuizApp": "Server=YOUR_SERVER_NAME;Database=QuizAppDb;Trusted_Connection=true;TrustServerCertificate=true;"
     },
     "Jwt": {
       "Secret": "your-super-secret-jwt-key-here",
       "Issuer": "QuizApp",
       "Audience": "QuizAppUser"
     }
   }
   ```

2. Run database migrations:
   - Open a terminal/command prompt in the `BlazingQuiz.Web\BlazingQuiz.Api` directory
   - Execute: `dotnet ef database update`

### 2. Python Environment Setup

1. Navigate to the `recommender_system` directory
2. Create a virtual environment (recommended):
   ```bash
   python -m venv venv
   ```
3. Activate the virtual environment:
   - On Windows: `venv\Scripts\activate`
   - On macOS/Linux: `source venv/bin/activate`
4. Install required Python packages:
   ```bash
   pip install -r requirements.txt
   ```

5. Set the database connection string as an environment variable:
   ```bash
   # On Windows Command Prompt:
   set DB_CONNECTION_STRING="Driver={ODBC Driver 17 for SQL Server};Server=YOUR_SERVER_NAME;Database=QuizAppDb;Trusted_Connection=yes;TrustServerCertificate=yes;"

   # On Windows PowerShell:
   $env:DB_CONNECTION_STRING="Driver={ODBC Driver 17 for SQL Server};Server=YOUR_SERVER_NAME;Database=QuizAppDb;Trusted_Connection=yes;TrustServerCertificate=yes;"

   # On macOS/Linux:
   export DB_CONNECTION_STRING="Driver={ODBC Driver 17 for SQL Server};Server=YOUR_SERVER_NAME;Database=QuizAppDb;Trusted_Connection=yes;TrustServerCertificate=yes;"
   ```

### 3. Running the Applications

#### Option A: IDE-based (Recommended for development)

1. **Start the Python Recommendation API**:
   - Open a new terminal/command prompt
   - Navigate to the `recommender_system\api` directory
   - Activate your Python virtual environment:
     ```bash
     # On Windows
     venv\Scripts\activate

     # On macOS/Linux
     source venv/bin/activate
     ```
   - Run the FastAPI application:
     ```bash
     python main.py
     ```
   - The recommendation API will start on `http://localhost:8000`

2. **Start the .NET API Backend**:
   - Open another terminal/command prompt
   - Navigate to `BlazingQuiz.Web\BlazingQuiz.Api`
   - Run:
     ```bash
     dotnet run
     ```
   - The API will start on `https://localhost:7048` and `http://localhost:5048`

3. **Start the Blazor WebAssembly Frontend**:
   - Open another terminal/command prompt
   - Navigate to `BlazingQuiz.Web\BlazingQuiz.Web`
   - Run:
     ```bash
     dotnet run
     ```
   - The web application will start (typically on `https://localhost:7082`)

#### Option B: Using Visual Studio

1. Open the `BlazingQuiz.Web\BlazingQuiz.Web.sln` solution file in Visual Studio
2. Set multiple startup projects:
   - Right-click the solution → Properties → Startup Projects
   - Select "Multiple startup projects"
   - Set `BlazingQuiz.Api` and `BlazingQuiz.Web` to "Start"
3. Make sure the Python API is running separately (see step 1 above)

### 4. Environment Variables

For the application to work properly, you need to set the following environment variables:

- `DB_CONNECTION_STRING`: Connection string for your SQL Server database
- (Optional) Database connection can also be set in `appsettings.json`

### 8. Python Path Configuration

The Python recommendation system is correctly configured to find the required modules:
- `recommender_system/api/main.py` imports from the parent directory
- `recommender_system/trigger_recommendation_update.py` imports from the same directory
- Both files have the appropriate sys.path configuration

### 9. Testing the Recommendation System

1. Start all three components (Python API, .NET API, Blazor frontend)
2. Register a new user or log in with an existing account
3. Create some quizzes or use existing ones
4. Complete a few quizzes and rate them
5. The recommendation system will automatically update when you submit ratings
6. Navigate to "Recommended Quizzes" page to see personalized quiz suggestions

### 6. API Endpoints

- **.NET Backend API**: `https://localhost:7048` (or `http://localhost:5048`)
- **Blazor Frontend**: `https://localhost:7082` (or `http://localhost:5260`)
- **Python Recommendation API**: `http://localhost:8000`

### 7. Key Features

- **Quiz Taking**: Students can take quizzes and see detailed results
- **Rating System**: Students can rate quizzes they've taken
- **Recommendation Engine**: Python-based system using Item-based Collaborative Filtering + KNN
- **Recommendation Page**: View personalized quiz recommendations
- **Result Page Integration**: Direct link from quiz results to recommendations

### 8. Troubleshooting

**If Python API is not connecting to the database:**
- Verify your database connection string is correct
- Ensure SQL Server is running
- Check that you have the required ODBC drivers installed

**If recommendation button doesn't work:**
- Verify that the Python API is running on `http://localhost:8000`
- Check browser console for any JavaScript errors

**If the Python script fails to import modules:**
- Make sure to run Python scripts from the correct directory
- The recommendation_engine.py should be in the recommender_system directory alongside trigger_recommendation_update.py
- The API main.py in the api subdirectory is configured to import from the parent directory

**If migrations fail:**
- Ensure your connection string is correct in `appsettings.json`
- Verify you have the Entity Framework tools installed (`dotnet tool install --global dotnet-ef`)

### 9. Development Notes

- The recommendation system is triggered automatically when a user submits a rating for a quiz
- The system uses Item-based Collaborative Filtering with KNN to find similar quizzes
- Quiz recommendations are stored in the `RecommendedQuizzes` table in the database
- The Blazor frontend fetches recommendations from the .NET API, which can retrieve from the database

---

For any issues not covered in this guide, please check the application logs for more detailed error information.