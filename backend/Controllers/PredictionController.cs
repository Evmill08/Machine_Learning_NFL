using System.Net.WebSockets;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

// TODO: If we make a frontend, we're going to want to cache these predictions
// and likey have them running in the background from the app start
// Takes about 1-2 mins, for an entire week
// Can run concurrent calls for each game and it should reduce it to like
// Minute total, 10ish seconds per game
namespace backend.Controllers
{
    [ApiController]
    [Route("prediction")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictionController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpGet("gamePrediction")]
        public async Task<IActionResult> GetGamePrediction()
        {
            var response = await _predictionService.GetPredictionsForWeekAsync();
            return Ok(response);
        }
    }
}