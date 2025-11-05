using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazingQuiz.Api.Data.Migrations
{
    [DbContext(typeof(QuizContext))]
    [Migration("20251105140300_AddHasSubmittedToRoomParticipant")]
    partial class AddHasSubmittedToRoomParticipant
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ImagePath")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Option", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("Options");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AnswerExplanation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AudioPath")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("ImagePath")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("IsTextAnswer")
                        .HasColumnType("bit");

                    b.Property<int>("QuizId")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Quiz", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AudioPath")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImagePath")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("MaxQuestions")
                        .HasColumnType("int");

                    b.Property<int?>("MinQuestions")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<int>("TimeInMinutes")
                        .HasColumnType("int");

                    b.Property<int>("TotalQuestions")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("CreatedBy");

                    b.ToTable("Quizzes");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.QuizBookmark", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("QuizId")
                        .HasColumnType("int");

                    b.Property<int>("StudentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.HasIndex("StudentId");

                    b.ToTable("QuizBookmarks");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.QuizFeedback", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsCommentDeleted")
                        .HasColumnType("bit");

                    b.Property<int?>("QuizId")
                        .HasColumnType("int");

                    b.Property<int?>("StudentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.HasIndex("StudentId");

                    b.ToTable("QuizFeedback");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("nvarchar(6)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime?>("EndedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<int>("MaxParticipants")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid?>("QuizId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("StartedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.RoomAnswer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AnsweredAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<int?>("OptionId")
                        .HasColumnType("int");

                    b.Property<string>("TextAnswer")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.HasIndex("RoomId");

                    b.HasIndex("UserId");

                    b.ToTable("RoomAnswers");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.RoomParticipant", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsReady")
                        .HasColumnType("bit");

                    b.Property<bool>("HasSubmitted")
                        .HasColumnType("bit");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.HasIndex("UserId");

                    b.ToTable("RoomParticipants");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.StudentQuiz", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("QuizId")
                        .HasColumnType("int");

                    b.Property<int>("StudentId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<double?>("TotalScore")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentQuizzes");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.StudentQuizQuestionResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AnsweredAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("OptionId")
                        .HasColumnType("int");

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<int>("StudentQuizId")
                        .HasColumnType("int");

                    b.Property<string>("TextAnswer")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.HasIndex("StudentQuizId");

                    b.ToTable("StudentQuizQuestionResponses");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AvatarPath")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Option", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Question", "Question")
                        .WithMany("Options")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Question", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Quiz", "Quiz")
                        .WithMany("Questions")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Quiz");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Quiz", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Category", "Category")
                        .WithMany("Quizzes")
                        .HasForeignKey("CategoryId");

                    b.HasOne("BlazingQuiz.Api.Data.Entities.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedBy");

                    b.Navigation("Category");

                    b.Navigation("CreatedByUser");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.QuizBookmark", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Quiz", "Quiz")
                        .WithMany("Bookmarks")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazingQuiz.Api.Data.Entities.User", "Student")
                        .WithMany("Bookmarks")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Quiz");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.QuizFeedback", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Quiz", "Quiz")
                        .WithMany("Feedbacks")
                        .HasForeignKey("QuizId");

                    b.HasOne("BlazingQuiz.Api.Data.Entities.User", "Student")
                        .WithMany("Feedbacks")
                        .HasForeignKey("StudentId");

                    b.Navigation("Quiz");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Room", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Quiz", "Quiz")
                        .WithMany()
                        .HasForeignKey("QuizId");

                    b.Navigation("Quiz");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.RoomAnswer", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Question", "Question")
                        .WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazingQuiz.Api.Data.Entities.Room", "Room")
                        .WithMany("RoomAnswers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazingQuiz.Api.Data.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.RoomParticipant", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Room", "Room")
                        .WithMany("Participants")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazingQuiz.Api.Data.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.StudentQuiz", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Quiz", "Quiz")
                        .WithMany("StudentQuizzes")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazingQuiz.Api.Data.Entities.User", "Student")
                        .WithMany("StudentQuizzes")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Quiz");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.StudentQuizQuestionResponse", b =>
                {
                    b.HasOne("BlazingQuiz.Api.Data.Entities.Question", "Question")
                        .WithMany()
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazingQuiz.Api.Data.Entities.StudentQuiz", "StudentQuiz")
                        .WithMany("QuestionResponses")
                        .HasForeignKey("StudentQuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");

                    b.Navigation("StudentQuiz");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Quiz", b =>
                {
                    b.Navigation("Bookmarks");

                    b.Navigation("Feedbacks");

                    b.Navigation("Questions");

                    b.Navigation("StudentQuizzes");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.Room", b =>
                {
                    b.Navigation("Participants");

                    b.Navigation("RoomAnswers");
                });

            modelBuilder.Entity("BlazingQuiz.Api.Data.Entities.User", b =>
                {
                    b.Navigation("Bookmarks");

                    b.Navigation("Feedbacks");

                    b.Navigation("StudentQuizzes");
                });
#pragma warning restore 612, 618
        }
    }
}