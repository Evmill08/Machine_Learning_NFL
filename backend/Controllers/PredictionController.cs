using System.Net.WebSockets;
using backend.Models;
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

        // Gets predictions for the entire week, good for personal use
        [HttpGet("weekPrediction")]
        public async Task<IActionResult> GetWeekPrediction()
        {
            var response = await _predictionService.GetPredictionsForWeekAsync();
            return Ok(response);
        }

        // Gets the data for a specific event, much quicker.
        // EventId is a string here. We pass strings back and forth
        [HttpGet("gamePrediction/{eventId}")]
        public async Task<IActionResult> GetGamePrediction(string eventId)
        {
            //var response = await _predictionService.GetPredictionDataForEventAsync(eventId);
            // TODO: Replace this with the call above once the python predictions are good

            var response = new PredictionResponse
            {
                HomeTeamName = "Ravens",
                AwayTeamName = "Bengals",
                EventId = eventId,
                Date = DateTime.Now,
                GamePrediction = new GamePrediction
                {
                    SpreadPrediction = 1,
                    SpreadRange = [1,2],
                    SpreadConfidenceScore = 1,
                    TotalPrediction = 1,
                    TotalRange = [1,2],
                    TotalConfidenceScore = 1,
                    WinnerPrediction = "Ravens",
                    WinnerConfidence = 1,
                    HomeWinProbability = 1,
                    AwayWinProbability = 0,
                },
                VegasLowestSpread = new VegasPrediction
                {
                    SportsBook = "ESPNBet",
                    OddsValue = -12.5
                },
                VegasLowestTotal = new VegasPrediction
                {
                    SportsBook = "ESPNBet",
                    OddsValue = -12.5
                },
                VegasWinner = "Ravens"
            };

            return Ok(response);
        }
    }
}