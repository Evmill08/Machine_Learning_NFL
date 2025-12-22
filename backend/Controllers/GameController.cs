using backend.Services;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

// This is our controller for our front end to call to get basic game data to display to the user
namespace backend.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly IGameDataService _gameDataService;

        public GameController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        [HttpGet("currentWeekGames/{weekNumber}")]
        public async Task<IActionResult> GetCurrentWeekGames(int weekNumber)
        {
            var response = await _gameDataService.GetGameDataForCurrentWeekAsync(weekNumber);
            return Ok(response);
        }

        [HttpGet("currentWeek")]
        public async Task<IActionResult> GetCurrentWeek()
        {
            var response = await _gameDataService.GetCurrentWeekAsync();
            return Ok(response);
        }
    }
}