using backend.Models;
using ClosedXML.Excel;

// TODO: Look for any optimizations here
namespace backend.Services
{
    public interface IExcelService
    {
        Task ExportRangeDataToExcelAsync(int startYear, int endYear);
        Task ExportWeekDataToExcelAsync();
        Task ExportYearDataToExcelAsync();
    }

    public class ExcelService : IExcelService
    {
        private readonly IPredictionDataService _predictionDataService;
        private readonly IEventService _eventService;
        private readonly string _exportDirectory;
        private readonly string _filePath;

        public ExcelService(IPredictionDataService predictionDataService, IEventService eventService)
        {
            _predictionDataService = predictionDataService;
            _eventService = eventService;

            _exportDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NFLPredictions");
            Directory.CreateDirectory(_exportDirectory);

            _filePath = Path.Combine(_exportDirectory, "NFL_Predictions.xlsx");
        }

        public async Task ExportRangeDataToExcelAsync(int startYear, int endYear)
        {
            XLWorkbook workbook = File.Exists(_filePath) ? new XLWorkbook(_filePath) : new XLWorkbook();

            string sheetName = $"Range_{startYear}_{endYear}";
            if (workbook.Worksheets.Any(ws => ws.Name == sheetName))
                workbook.Worksheet(sheetName).Delete();

            var worksheet = workbook.Worksheets.Add(sheetName);

            var props = typeof(PredictionData).GetProperties();
            for (int i = 0; i < props.Length; i++)
                worksheet.Cell(1, i + 1).Value = props[i].Name;

            var predictionData = await _predictionDataService.GetPredictionDataForTimeframe(startYear, endYear);

            int row = 2;
            foreach (var prediction in predictionData)
            {
                for (int col = 0; col < props.Length; col++)
                {
                    var value = props[col].GetValue(prediction);
                    var cell = worksheet.Cell(row, col + 1);

                    // Set value based on type
                    if (value == null)
                    {
                        cell.Value = "";
                    }
                    else
                    {
                        cell.Value = value switch
                        {
                            string s => s,
                            int i => i,
                            long l => l,
                            double d => d,
                            float f => f,
                            decimal dec => dec,
                            bool b => b,
                            DateTime dt => dt,
                            TimeSpan ts => ts,
                            _ => value.ToString() ?? ""
                        };
                    }
                }
                row++;
            }

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(_filePath);
            workbook.Dispose();

            Console.WriteLine($"✅ Exported range data to sheet '{sheetName}' in {_filePath}");
        }

        public async Task ExportWeekDataToExcelAsync()
        {
            int currentSeasonYear = DateTime.Now.Year;

            var currentGames = await _eventService.GetEventsForCurrentWeek(currentSeasonYear);
            if (!currentGames.Any())
            {
                Console.WriteLine("⚠️ No games found for the current week.");
                return;
            }

            var currentWeek = currentGames.First().Week;
            var predictions = new List<PredictionData>();

            foreach (var e in currentGames)
            {
                var prediction = await _predictionDataService.GetPredictionDataForEvent(e);
                predictions.Add(prediction);
            }

            XLWorkbook workbook = File.Exists(_filePath) ? new XLWorkbook(_filePath) : new XLWorkbook();

            string sheetName = $"{currentSeasonYear}_W{currentWeek}";
            if (workbook.Worksheets.Any(ws => ws.Name == sheetName))
                workbook.Worksheet(sheetName).Delete();

            var worksheet = workbook.Worksheets.Add(sheetName);

            var props = typeof(PredictionData).GetProperties();
            for (int i = 0; i < props.Length; i++)
                worksheet.Cell(1, i + 1).Value = props[i].Name;

            int row = 2;
            foreach (var prediction in predictions)
            {
                for (int col = 0; col < props.Length; col++)
                {
                    var value = props[col].GetValue(prediction);
                    var cell = worksheet.Cell(row, col + 1);

                    // Set value based on type
                    if (value == null)
                    {
                        cell.Value = "";
                    }
                    else
                    {
                        cell.Value = value switch
                        {
                            string s => s,
                            int i => i,
                            long l => l,
                            double d => d,
                            float f => f,
                            decimal dec => dec,
                            bool b => b,
                            DateTime dt => dt,
                            TimeSpan ts => ts,
                            _ => value.ToString() ?? ""
                        };
                    }
                }
                row++;
            }

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(_filePath);
            workbook.Dispose();

            Console.WriteLine($"✅ Exported week {currentWeek} predictions to sheet '{sheetName}' in {_filePath}");
        }

        public async Task ExportYearDataToExcelAsync()
        {
            int currentSeasonYear = DateTime.Now.Year - 1;
            var predictionData = await _predictionDataService.GetPredictionDataForYear(currentSeasonYear);

            // Load existing workbook if available, otherwise create new
            using var workbook = File.Exists(_filePath) ? new XLWorkbook(_filePath) : new XLWorkbook();

            string sheetName = $"Year_{currentSeasonYear}";

            // If worksheet exists, delete it to overwrite
            var existingSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);
            existingSheet?.Delete();

            var worksheet = workbook.Worksheets.Add(sheetName);

            // Write header row
            var props = typeof(PredictionData).GetProperties();
            for (int i = 0; i < props.Length; i++)
                worksheet.Cell(1, i + 1).Value = props[i].Name;

            // Write data rows
            int row = 2;
            foreach (var prediction in predictionData)
            {
                for (int col = 0; col < props.Length; col++)
                {
                    var value = props[col].GetValue(prediction);
                    var cell = worksheet.Cell(row, col + 1);

                    cell.Value = value switch
                    {
                        null => "",
                        string s => s,
                        int i => i,
                        long l => l,
                        double d => d,
                        float f => f,
                        decimal dec => dec,
                        bool b => b,
                        DateTime dt => dt,
                        TimeSpan ts => ts,
                        _ => value.ToString() ?? ""
                    };
                }
                row++;
            }

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(_filePath);
            Console.WriteLine($"✅ Exported year data to sheet '{sheetName}' in {_filePath}");
        }

    }
}
