import sqlite3
import os

def create_required_tables():
    """
    Creates the missing tables required by the recommender system
    """
    db_path = 'quiz_app.db'

    # Connect to the database
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Create the Users table (referenced by foreign key in QuizFeedbacks)
    create_users_table_sql = '''
    CREATE TABLE IF NOT EXISTS Users (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        Email TEXT UNIQUE NOT NULL,
        Phone TEXT,
        PasswordHash TEXT NOT NULL,
        Role TEXT,
        IsApproved BOOLEAN DEFAULT 1,
        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
    );
    '''

    # Create the Quiz table (referenced by foreign key in QuizFeedbacks and used in other queries)
    create_quiz_table_sql = '''
    CREATE TABLE IF NOT EXISTS Quiz (
        Id TEXT PRIMARY KEY,  -- Using TEXT since it's a GUID in .NET
        Name TEXT NOT NULL,
        Description TEXT,
        CategoryId INTEGER,
        TotalQuestions INTEGER NOT NULL DEFAULT 0,
        TimeInMinutes INTEGER NOT NULL DEFAULT 0,
        IsActive BOOLEAN DEFAULT 1,
        CreatedBy INTEGER,
        ImagePath TEXT,
        AudioPath TEXT,
        Level TEXT,  -- Level of the quiz (e.g., Easy, Medium, Hard)
        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,

        FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
    );
    '''

    # Create the QuizFeedbacks table based on the .NET entity model
    create_quizfeedbacks_table_sql = '''
    CREATE TABLE IF NOT EXISTS QuizFeedbacks (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        StudentId INTEGER NOT NULL,
        QuizId TEXT NOT NULL,  -- Using TEXT since it's a GUID in .NET
        Score INTEGER,
        Comment TEXT,
        IsCommentDeleted BOOLEAN DEFAULT 0,
        CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,

        FOREIGN KEY (StudentId) REFERENCES Users(Id),
        FOREIGN KEY (QuizId) REFERENCES Quiz(Id)
    );
    '''

    try:
        # Execute all table creation statements
        cursor.execute(create_users_table_sql)
        cursor.execute(create_quiz_table_sql)
        cursor.execute(create_quizfeedbacks_table_sql)

        conn.commit()
        print("Required tables created successfully!")

        # Check if tables were created
        tables = ['Users', 'Quiz', 'QuizFeedbacks']
        for table in tables:
            cursor.execute("SELECT name FROM sqlite_master WHERE type='table' AND name=?;", (table,))
            result = cursor.fetchone()
            if result:
                print(f"Verification: {table} table exists in the database.")
            else:
                print(f"Warning: {table} table was not created properly.")

    except sqlite3.Error as e:
        print(f"Database error: {e}")
    finally:
        conn.close()

if __name__ == "__main__":
    create_required_tables()