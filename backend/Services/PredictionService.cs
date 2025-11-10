using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using backend.Models;
using ClosedXML.Excel;

namespace backend.Services
{
    public interface IPredictionService
    {
        // TODO: need a way to give the game ID or soemthing to this
        public Task<IEnumerable<PredictionResponse>> GetPredictionsForWeekAsync();
    }

    public class PredictionService : IPredictionService
    {
        private readonly IPredictionDataService _predictionDataService;
        private readonly IEventService _eventService;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _exportDirectory;
        private readonly string _filePath;

        public PredictionService(IPredictionDataService predictionDataService, IEventService eventService, HttpClient httpClient)
        {
            _predictionDataService = predictionDataService;
            _eventService = eventService;
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = false
            };
            _exportDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NFLPredictions");
            Directory.CreateDirectory(_exportDirectory);

            _filePath = Path.Combine(_exportDirectory, "NFL_Predictions.xlsx");
        }

        public async Task<IEnumerable<PredictionResponse>> GetPredictionsForWeekAsync()
        {
            int currentSeasonYear = DateTime.Now.Year;

            var currentGames = await _eventService.GetEventsForCurrentWeek(currentSeasonYear);
            if (!currentGames.Any())
            {
                Console.WriteLine("No games found for the current week.");
                return new List<PredictionResponse>();
            }

            var predictions = currentGames
            .Select(e => (e, _predictionDataService.GetPredictionDataForEvent(e)))
            .ToList();

            var currentWeek = predictions.FirstOrDefault().e.Week;

            var predictionResults = new ConcurrentBag<PredictionResponse>();
            var url = "http://localhost:8000/predict";

            var tasks = predictions.Select(async prediction =>
            {
                try
                {
                    var jsonContent = JsonSerializer.Serialize(prediction.Item2, _jsonOptions);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    Console.WriteLine($"Sending to FastAPI for {prediction.Item2.HomeTeamName} vs {prediction.Item2.AwayTeamName}:");
                    Console.WriteLine(jsonContent.Substring(0, Math.Min(500, jsonContent.Length)) + "...");

                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Response: {responseJson}");

                        var result = JsonSerializer.Deserialize<GamePrediction>(responseJson, _jsonOptions);

                        if (result != null)
                        {
                            var predictionResponse = new PredictionResponse
                            {
                                HomeTeamName = prediction.Item2.HomeTeamName,
                                AwayTeamName = prediction.Item2.AwayTeamName,
                                EventId = prediction.e.Id,
                                Date = prediction.e.Date,
                                GamePrediction = result,
                                VegasLowestSpread = prediction.e.Competitions.FirstOrDefault().CompetitionOdds.AverageSpread,
                                VegasLowestTotal = prediction.e.Competitions.FirstOrDefault().CompetitionOdds.AverageOverUnder,
                                VegasWinner = prediction.e.Competitions.FirstOrDefault().CompetitionOdds.AverageSpread < 0 ? "Home" : "Away",

                            };
                            predictionResults.Add(predictionResponse);
                            
                            Console.WriteLine($"Successfully processed prediction for {prediction.Item2.HomeTeamName} vs {prediction.Item2.AwayTeamName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calling FastAPI: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);

            SaveToExcel(predictionResults, currentWeek);
            return predictionResults;
        }
        
        private void SaveToExcel(IEnumerable<PredictionResponse> predictions, int weekNumber)
        {
            XLWorkbook workbook = File.Exists(_filePath) ? new XLWorkbook(_filePath) : new XLWorkbook();

            string sheetName = $"Predictions_Week_{weekNumber}";
            if (workbook.Worksheets.Any(ws => ws.Name == sheetName))
                workbook.Worksheet(sheetName).Delete();

            var worksheet = workbook.Worksheets.Add(sheetName);

            var headers = new[]
            {
                "HomeTeamName",
                "AwayTeamName",
                "Date",
                "VegasLowestSpread",
                "VegasLowestTotal",
                "VegasWinner",
                "Spread",
                "SpreadConfidenceScore",
                "Total",
                "TotalConfidenceScore",
                "WinnerPrediction",
                "WinnerConfidence",
                "HomeWinProbability",
                "AwayWinProbability",
                "ImpliedHomeScore",
                "ImpliedAwayScore",
                "ModelsAligned"
            };

            for (int i = 0; i < headers.Length; i++)
                worksheet.Cell(1, i + 1).Value = headers[i];

            int row = 2;
            foreach (var prediction in predictions)
            {
                var gp = prediction.GamePrediction;

                worksheet.Cell(row, 1).Value = prediction.HomeTeamName;
                worksheet.Cell(row, 2).Value = prediction.AwayTeamName;
                worksheet.Cell(row, 3).Value = prediction.Date;
                worksheet.Cell(row, 4).Value = prediction.VegasLowestSpread;
                worksheet.Cell(row, 5).Value = prediction.VegasLowestTotal;
                worksheet.Cell(row, 6).Value = prediction.VegasWinner;

                if (gp != null)
                {
                    worksheet.Cell(row, 7).Value = gp.SpreadPrediction;
                    worksheet.Cell(row, 8).Value = gp.SpreadConfidenceScore;
                    worksheet.Cell(row, 9).Value = gp.TotalPrediction;
                    worksheet.Cell(row, 10).Value = gp.TotalConfidenceScore;
                    worksheet.Cell(row, 11).Value = gp.WinnerPrediction;
                    worksheet.Cell(row, 12).Value = gp.WinnerConfidence;
                    worksheet.Cell(row, 13).Value = gp.HomeWinProbability;
                    worksheet.Cell(row, 14).Value = gp.AwayWinProbability;
                    worksheet.Cell(row, 15).Value = gp.ImpliedHomeScore;
                    worksheet.Cell(row, 16).Value = gp.ImpliedAwayScore;
                    worksheet.Cell(row, 17).Value = gp.ModelsAligned;
                }

                row++;
            }

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(_filePath);
            workbook.Dispose();

            Console.WriteLine($"Exported predictions to sheet '{sheetName}' in {_filePath}");
        }

    }
}