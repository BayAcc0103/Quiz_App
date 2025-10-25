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
        public DbSet<User> Users { get; set; }
        public DbSet<StudentQuizQuestion> StudentQuizQuestions { get; set; }
        public DbSet<QuizBookmark> QuizBookmarks { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Comment> Comments { get; set; }

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
