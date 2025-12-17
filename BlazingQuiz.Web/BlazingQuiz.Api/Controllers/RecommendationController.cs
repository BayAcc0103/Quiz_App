using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<RecommendedQuiz>>> GetRecommendations(int userId)
        {
            var recommendations = await _context.RecommendedQuizzes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.PredictedRating)
                .ToListAsync();

            return Ok(recommendations);
        }
        
        [HttpPost("trigger/{userId}")]
        public async Task<IActionResult> TriggerRecommendationUpdate(int userId)
        {
            try
            {
                // This would typically call the Python service to update recommendations
                // For now, we'll just return success to acknowledge the call
                // In a real implementation, this would trigger the Python recommendation system
                
                // Log that recommendation update was requested
                Console.WriteLine($"Recommendation update triggered for user {userId}");
                
                return Ok(new { message = "Recommendation update triggered successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error triggering recommendation update: {ex.Message}");
                return BadRequest(new { error = "Failed to trigger recommendation update" });
            }
        }
    }
}