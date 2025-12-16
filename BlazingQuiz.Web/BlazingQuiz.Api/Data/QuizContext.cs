using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Data
{
    public class QuizContext : DbContext
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public QuizContext(DbContextOptions<QuizContext> options, IPasswordHasher<User> passwordHasher) : base(options)
        {
            _passwordHasher = passwordHasher;
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<StudentQuiz> StudentQuizzes { get; set; }
        public DbSet<StudentQuizForRoom> StudentQuizzesForRoom { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StudentQuizQuestion> StudentQuizQuestions { get; set; }
        public DbSet<StudentQuizQuestionForRoom> StudentQuizQuestionsForRoom { get; set; }
        public DbSet<QuizBookmark> QuizBookmarks { get; set; }
        public DbSet<QuizFeedback> QuizFeedbacks { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomQuiz> RoomQuizzes { get; set; }
        public DbSet<RoomParticipant> RoomParticipants { get; set; }
        public DbSet<RoomAnswer> RoomAnswers { get; set; }
        public DbSet<TextAnswer> TextAnswers { get; set; }
        public DbSet<QuizCategory> QuizCategories { get; set; }
        public DbSet<RecommendedQuiz> RecommendedQuizzes { get; set; } 

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentQuizQuestion>()
                .HasKey(s => new { s.StudentQuizId, s.QuestionId });

            modelBuilder.Entity<StudentQuizQuestion>()
                .HasOne(s => s.StudentQuiz)
                .WithMany(s => s.StudentQuizQuestions)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentQuizQuestion>()
                .HasOne(s => s.Question)
                .WithMany(s => s.StudentQuizQuestions)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure QuizFeedback entity
            modelBuilder.Entity<QuizFeedback>()
                .HasOne(r => r.Student)
                .WithMany(s => s.QuizFeedbacks)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QuizFeedback>()
                .HasOne(r => r.Quiz)
                .WithMany(q => q.QuizFeedbacks)
                .HasForeignKey(r => r.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Rating entity
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Student)
                .WithMany(s => s.Ratings)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Quiz)
                .WithMany(q => q.Ratings)
                .HasForeignKey(r => r.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Comment entity
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Student)
                .WithMany(s => s.Comments)
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Quiz)
                .WithMany(q => q.Comments)
                .HasForeignKey(c => c.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Room entity - direct relationship with Quiz
            modelBuilder.Entity<Room>()
                .HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Quiz)
                .WithMany()
                .HasForeignKey(r => r.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure RoomAnswer entity
            modelBuilder.Entity<RoomAnswer>()
                .HasOne(ra => ra.Room)
                .WithMany(r => r.RoomAnswers)
                .HasForeignKey(ra => ra.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RoomAnswer>()
                .HasOne(ra => ra.User)
                .WithMany()
                .HasForeignKey(ra => ra.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RoomAnswer>()
                .HasOne(ra => ra.Question)
                .WithMany()
                .HasForeignKey(ra => ra.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure TextAnswer entity
            modelBuilder.Entity<TextAnswer>()
                .HasOne(ta => ta.Question)
                .WithMany(q => q.TextAnswers)
                .HasForeignKey(ta => ta.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure many-to-many relationship between Quiz and Category through QuizCategory
            modelBuilder.Entity<QuizCategory>()
                .HasKey(qc => new { qc.Id });

            modelBuilder.Entity<QuizCategory>()
                .HasOne(qc => qc.Quiz)
                .WithMany(q => q.QuizCategories)
                .HasForeignKey(qc => qc.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizCategory>()
                .HasOne(qc => qc.Category)
                .WithMany()
                .HasForeignKey(qc => qc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure StudentQuizForRoom entity
            modelBuilder.Entity<StudentQuizForRoom>()
                .HasOne(sqfr => sqfr.Student)
                .WithMany()
                .HasForeignKey(sqfr => sqfr.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentQuizForRoom>()
                .HasOne(sqfr => sqfr.Quiz)
                .WithMany()
                .HasForeignKey(sqfr => sqfr.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentQuizForRoom>()
                .HasOne(sqfr => sqfr.Room)
                .WithMany(r => r.StudentQuizzesForRoom)
                .HasForeignKey(sqfr => sqfr.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure StudentQuizQuestion entity (for regular quizzes)
            modelBuilder.Entity<StudentQuizQuestion>()
                .HasKey(s => new { s.StudentQuizId, s.QuestionId });

            modelBuilder.Entity<StudentQuizQuestion>()
                .HasOne(s => s.StudentQuiz)
                .WithMany(s => s.StudentQuizQuestions)
                .HasForeignKey(s => s.StudentQuizId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentQuizQuestion>()
                .HasOne(s => s.Question)
                .WithMany(q => q.StudentQuizQuestions)
                .HasForeignKey(s => s.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure StudentQuizQuestionForRoom entity
            modelBuilder.Entity<StudentQuizQuestionForRoom>()
                .HasKey(s => new { s.StudentQuizForRoomId, s.QuestionId });

            modelBuilder.Entity<StudentQuizQuestionForRoom>()
                .HasOne(s => s.StudentQuizForRoom)
                .WithMany(s => s.StudentQuizQuestionsForRoom)
                .HasForeignKey(s => s.StudentQuizForRoomId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentQuizQuestionForRoom>()
                .HasOne(s => s.Question)
                .WithMany(q => q.StudentQuizQuestionsForRoom)
                .HasForeignKey(s => s.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure RecommendedQuiz entity
            modelBuilder.Entity<RecommendedQuiz>()
                .HasOne(rq => rq.User)
                .WithMany()
                .HasForeignKey(rq => rq.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
            var adminUser = new User
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@gmail.com",
                Phone = "0123456789",
                Role = nameof(UserRole.Admin),
                IsApproved = true,
            };
            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "123456");

            modelBuilder.Entity<User>().HasData(adminUser);
        }
    }
}
