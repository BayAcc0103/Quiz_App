using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazingQuiz.Shared.Enums;

namespace BlazingQuiz.Api.Data.Entities
{
    public class QuizFeedback
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Guid QuizId { get; set; }
        public int? Score { get; set; } // Rating as integer (1-5 scale: 1=very bad, 2=bad, 3=normal, 4=good, 5=very good)
        public string? Comment { get; set; } // Comment content
        public bool IsCommentDeleted { get; set; } = false;

        [ForeignKey(nameof(StudentId))]
        public virtual User Student { get; set; }

        [ForeignKey(nameof(QuizId))]
        public virtual Quiz Quiz { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Converts the integer score back to the corresponding text representation
        /// </summary>
        public string? ScoreText
        {
            get
            {
                return Score switch
                {
                    1 => RatingText.VeryBad,
                    2 => RatingText.Bad,
                    3 => RatingText.Normal,
                    4 => RatingText.Good,
                    5 => RatingText.VeryGood,
                    _ => null
                };
            }
        }

        /// <summary>
        /// Converts text score to corresponding integer value
        /// </summary>
        public static int? TextScoreToInt(string? textScore)
        {
            if (string.IsNullOrWhiteSpace(textScore))
                return null;

            return textScore.Trim().ToLower() switch
            {
                var s when s == RatingText.VeryBad => 1,
                var s when s == RatingText.Bad => 2,
                var s when s == RatingText.Normal => 3,
                var s when s == RatingText.Good => 4,
                var s when s == RatingText.VeryGood => 5,
                _ => null
            };
        }
    }
}