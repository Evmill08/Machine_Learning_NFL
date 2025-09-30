using backend.Services;
using Microsoft.AspNetCore.Mvc;


namespace backend.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly IEndpointTestService _endpointTestService;

        public TestController(IEndpointTestService endpointTestService)
        {
            _endpointTestService = endpointTestService;
        }

        [HttpGet("seasons")]
        public async Task<IActionResult> TestSeason()
        {
            await _endpointTestService.TestSeasonEndpointAysnc();
            return Ok("Season test passed");
        }

        [HttpGet("weeks")]
        public async Task<IActionResult> TestWeek()
        {
            await _endpointTestService.TestWeekEndpointAsync();
            return Ok("week test passed");
        }

        [HttpGet("events")]
        public async Task<IActionResult> TestEvents()
        {
            await _endpointTestService.TestEventsEndpointAsync();
            return Ok("Event tests passed");
        }
    }

}