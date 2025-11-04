using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
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
            Console.WriteLine("=== TestController constructor START ===");
            _endpointTestService = endpointTestService;
            Console.WriteLine("=== TestController constructor END ===");
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
            Console.WriteLine("=== TestEventData method START ===");
            var response = await _endpointTestService.GetEventDataTestAsync();
            Console.WriteLine("=== TestEventData method END ===");
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

        [HttpGet("predictionData")]
        public async Task<IActionResult> TestPredictionDataForEvent()
        {
            var response = await _endpointTestService.GetPredictionDataForEventTestAsync();
            return Ok(response);
        }

        [HttpGet("allPredictionData")]
        public async Task<IActionResult> TestPredictionData()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var response = await _endpointTestService.GetPredictionDataAsync();
            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;
            Console.WriteLine($"Elapsed time: {elapsed.TotalSeconds} seconds");
            Console.WriteLine($"Elapsed Time: {elapsed.TotalMinutes} minutes");
            return Ok(response);
        }
    }
}