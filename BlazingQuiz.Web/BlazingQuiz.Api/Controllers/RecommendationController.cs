using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;

namespace BlazingQuiz.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly QuizContext _context;

        public RecommendationController(QuizContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<RecommendedQuizDto>>> GetRecommendedQuizzes(int userId)
        {
            try
            {
                var recommendedQuizzes = await _context.RecommendedQuizzes
                    .Where(rq => rq.UserId == userId)
                    .OrderByDescending(rq => rq.PredictedRating)
                    .ToListAsync();

                var quizIds = recommendedQuizzes.Select(rq => Guid.Parse(rq.QuizId)).ToList();

                var quizzes = await _context.Quizzes
                    .Where(q => quizIds.Contains(q.Id))
                    .ToListAsync();

                var result = recommendedQuizzes
                    .Join(quizzes,
                          recommended => Guid.Parse(recommended.QuizId),
                          quiz => quiz.Id,
                          (recommended, quiz) => new RecommendedQuizDto
                          {
                              QuizId = recommended.QuizId,
                              Name = quiz.Name,
                              Description = quiz.Description,
                              CategoryId = quiz.CategoryId,
                              TotalQuestions = quiz.TotalQuestions,
                              TimeInMinutes = quiz.TimeInMinutes,
                              Level = quiz.Level,
                              PredictedRating = (double)recommended.PredictedRating
                          })
                    .OrderByDescending(q => q.PredictedRating)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("trigger/{userId}")]
        public async Task<IActionResult> TriggerRecommendationUpdate(int userId)
        {
            try
            {
                // Call the Python recommendation system to update recommendations for this user
                await TriggerPythonRecommendationUpdate(userId);

                return Ok(new { message = "Recommendation update triggered", userId = userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private async Task TriggerPythonRecommendationUpdate(int userId)
        {
            string pythonScriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "recommender_system", "trigger_recommendation_update.py");

            // Try different path combinations to find the script
            if (!System.IO.File.Exists(pythonScriptPath))
            {
                pythonScriptPath = Path.Combine(AppContext.BaseDirectory, "recommender_system", "trigger_recommendation_update.py");
            }

            if (!System.IO.File.Exists(pythonScriptPath))
            {
                // Try using the project root path
                string projectRoot = Directory.GetCurrentDirectory();
                // Navigate up to find the recommender_system directory
                for (int i = 0; i < 3; i++)
                {
                    string possiblePath = Path.Combine(projectRoot, "recommender_system", "trigger_recommendation_update.py");
                    if (System.IO.File.Exists(possiblePath))
                    {
                        pythonScriptPath = possiblePath;
                        break;
                    }
                    projectRoot = Path.GetDirectoryName(projectRoot);
                    if (string.IsNullOrEmpty(projectRoot)) break;
                }
            }

            if (System.IO.File.Exists(pythonScriptPath))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScriptPath}\" --user-id {userId} --method local",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();

                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            Console.WriteLine($"Python script error for user {userId}: {error}");
                        }
                        else
                        {
                            Console.WriteLine($"Python script output for user {userId}: {output}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Python script not found at: {pythonScriptPath}");
            }
        }

        public class RecommendedQuizDto
        {
            public string QuizId { get; set; }
            public string Name { get; set; }
            public string? Description { get; set; }
            public int? CategoryId { get; set; }
            public int TotalQuestions { get; set; }
            public int TimeInMinutes { get; set; }
            public string? Level { get; set; }
            public double PredictedRating { get; set; }
        }
    }
}