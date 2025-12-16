DATABASE_CONNECTION_STRING = (
    "mssql+pyodbc://@./BlazingQuiz"
    "?driver=ODBC+Driver+17+for+SQL+Server"
    "&trusted_connection=yes"
)
PORT = 5000
K_NEIGHBORS = 5                 # số quiz tương tự dùng cho KNN
UPDATE_INTERVAL_MINUTES = 5     # chạy phân tích lại mỗi 5 phút