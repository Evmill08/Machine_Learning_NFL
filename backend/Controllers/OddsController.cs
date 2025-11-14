using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("odds")]
    public class OddsController : ControllerBase
    {
        private readonly IOddsService _oddsService;

        public OddsController(IOddsService oddsService)
        {
            _oddsService = oddsService;
        }

        [HttpGet("allOdds/{eventId}")]
        public async Task<IActionResult> GetBestOddsForGameAysnc(int eventId)
        {
            var response = _oddsService.GetOddsByEventId(eventId);
            return Ok(response);
        }
    }
}