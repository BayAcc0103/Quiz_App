import sqlite3
import uuid
from datetime import datetime, timedelta

def populate_sample_data():
    """
    Populates the database with sample data to enable the recommender system
    """
    db_path = 'quiz_app.db'
    
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    try:
        # Clear existing sample data (for testing purposes)
        cursor.execute("DELETE FROM QuizFeedbacks WHERE StudentId >= 1 AND StudentId <= 10;")
        cursor.execute("DELETE FROM Quiz WHERE Id LIKE 'sample-%';")
        cursor.execute("DELETE FROM Users WHERE Id >= 1 AND Id <= 10;")
        
        print("Adding sample users...")
        # Insert sample users
        sample_users = [
            (1, 'Alice Johnson', 'alice@example.com', '1111111111', 'hash1', 'Student'),
            (2, 'Bob Smith', 'bob@example.com', '2222222222', 'hash2', 'Student'),
            (3, 'Charlie Brown', 'charlie@example.com', '3333333333', 'hash3', 'Student'),
            (4, 'Diana Prince', 'diana@example.com', '4444444444', 'hash4', 'Student'),
            (5, 'Evan Peters', 'evan@example.com', '5555555555', 'hash5', 'Student'),
            (6, 'Fiona Green', 'fiona@example.com', '6666666666', 'hash6', 'Student'),
            (7, 'George King', 'george@example.com', '7777777777', 'hash7', 'Student'),
            (8, 'Helen Stone', 'helen@example.com', '8888888888', 'hash8', 'Student'),
            (9, 'Ian Wright', 'ian@example.com', '9999999999', 'hash9', 'Student'),
            (10, 'Julia Roberts', 'julia@example.com', '1010101010', 'hash10', 'Student')
        ]
        
        cursor.executemany('''
            INSERT OR REPLACE INTO Users (Id, Name, Email, Phone, PasswordHash, Role) 
            VALUES (?, ?, ?, ?, ?, ?)
        ''', sample_users)
        
        print("Adding sample quizzes...")
        # Insert sample quizzes with UUIDs
        sample_quizzes = []
        quiz_ids = []
        for i in range(1, 8):
            quiz_id = f"sample-{i}-{uuid.uuid4()}"
            quiz_ids.append(quiz_id)
            quiz_name = f"Sample Quiz {i}"
            quiz_desc = f"Description for Sample Quiz {i}"
            sample_quizzes.append((
                quiz_id, quiz_name, quiz_desc, i % 3 + 1, 10, 20 + (i*5), 
                i % 3 == 0 and 'Hard' or i % 2 == 0 and 'Medium' or 'Easy', 
                i % 5 + 1  # CreatedBy user
            ))
        
        cursor.executemany('''
            INSERT OR REPLACE INTO Quiz (Id, Name, Description, CategoryId, TotalQuestions, TimeInMinutes, Level, CreatedBy) 
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        ''', sample_quizzes)
        
        print("Adding sample quiz feedback...")
        # Insert sample feedback - creating a matrix of different students rating different quizzes
        feedback_data = []
        for student_id in range(1, 11):  # Students 1-10
            for quiz_idx, quiz_id in enumerate(quiz_ids):  # All quizzes
                # Not every student rates every quiz - create a sparse rating matrix
                if (student_id + quiz_idx) % 2 == 0 or (student_id * quiz_idx) % 7 == 0:
                    # Generate a random score between 1-5
                    import random
                    score = random.randint(1, 5)
                    feedback_data.append((student_id, quiz_id, score, f"Feedback from student {student_id} for quiz {quiz_idx+1}"))
        
        cursor.executemany('''
            INSERT OR REPLACE INTO QuizFeedbacks (StudentId, QuizId, Score, Comment) 
            VALUES (?, ?, ?, ?)
        ''', feedback_data)
        
        conn.commit()
        
        # Verify the data
        cursor.execute("SELECT COUNT(*) FROM Users")
        user_count = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM Quiz")
        quiz_count = cursor.fetchone()[0]
        
        cursor.execute("SELECT COUNT(*) FROM QuizFeedbacks")
        feedback_count = cursor.fetchone()[0]
        
        print(f"Sample data added successfully!")
        print(f"- {user_count} users")
        print(f"- {quiz_count} quizzes") 
        print(f"- {feedback_count} feedback entries")
        
        # Show some sample feedback
        cursor.execute("SELECT StudentId, QuizId, Score FROM QuizFeedbacks LIMIT 10")
        sample_feedbacks = cursor.fetchall()
        print("\nSample feedbacks (StudentId, QuizId, Score):")
        for fb in sample_feedbacks:
            print(f"  {fb}")
        
    except sqlite3.Error as e:
        print(f"Database error: {e}")
    finally:
        conn.close()

if __name__ == "__main__":
    populate_sample_data()