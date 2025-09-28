

using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OddsController : ControllerBase
    {
        // Injected services go here
        private readonly IOddsService _oddsService;

        public OddsController(IOddsService oddsService)
        {
            // Initialize injected services here
            _oddsService = oddsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGamesForWeek(DateTime weekStart, DateTime weekEnd)
        {
            var games = await _oddsService.GetGamesForWeekAsync(weekStart, weekEnd);

            if (games == null)
            {
                return NotFound("No games found for the specified week.");
            }

            return Ok(games);
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetGameDetails(string eventId)
        {
            var gameDetails = await _oddsService.GetGameDetailsAsync(eventId);

            if (gameDetails == null)
            {
                return NotFound($"No details found for event ID: {eventId}");
            }

            return Ok(gameDetails);
        }
    }
}