using System.Net.WebSockets;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

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

        // Gets predictions for the entire week, good for personal use
        [HttpGet("weekPrediction")]
        public async Task<IActionResult> GetWeekPrediction()
        {
            var response = await _predictionService.GetPredictionsForWeekAsync();
            return Ok(response);
        }

        // Gets the data for a specific event, much quicker.
        [HttpGet("gamePrediction/{eventId}")]
        public async Task<IActionResult> GetGamePrediction(string eventId)
        {
            var response = await _predictionService.GetPredictionsForEventAsync(eventId);
            return Ok(response);
        }
    }
}
