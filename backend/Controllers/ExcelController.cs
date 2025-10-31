using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("data")]
    public class ExcelController : ControllerBase
    {
        private readonly IExcelService _excelService;

        public ExcelController(IExcelService excelService)
        {
            _excelService = excelService;
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetPredictionDataForRange()
        {
            await _excelService.ExportRangeDataToExcelAsync( 2020, 2025);
            return Ok("NFL_Predictions created");
        }

        [HttpGet("week")]
        public async Task<IActionResult> GetPredictionDataForWeek()
        {
            await _excelService.ExportWeekDataToExcelAsync();
            return Ok("NFL_Predictions updated with current games");
        }

        [HttpGet("year")]
        public async Task<IActionResult> GetPredictionDataForYear()
        {
            await _excelService.ExportYearDataToExcelAsync();
            return Ok("NFL_Predictions updated with this years games");
        }
    }
}