using System.Drawing;
using System.Text.Json;
using backend.Models;
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
            var response = await _endpointTestService.TestSeasonEndpointAysnc();
            return Ok(response);
        }

        [HttpGet("weeks")]
        public async Task<IActionResult> TestWeek()
        {
            var response = await _endpointTestService.TestWeekEndpointAsync();
            return Ok(response);
        }

        [HttpGet("events")]
        public async Task<IActionResult> TestEvents()
        {
            var response = await _endpointTestService.TestEventsEndpointAsync();
            return Ok(response);
        }

        [HttpGet("odds")]
        public async Task<IActionResult> TestOdds()
        {
            var response = await _endpointTestService.TestOddsEndpointAsync();
            return Ok(response);
        }

        [HttpGet("event")]
        public async Task<IActionResult> TestEventData()
        {
            var response = await _endpointTestService.GetEventDataTestAsync();
            return Ok(response);
        }


        [HttpGet("team")]
        public async Task<IActionResult> TestTeamData()
        {
            var response = await _endpointTestService.GetTeamDataTestAsync();
            return Ok(response);
        }
    }
}