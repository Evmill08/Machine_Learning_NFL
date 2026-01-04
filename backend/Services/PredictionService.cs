using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using backend.Models;
using ClosedXML.Excel;
using backend.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services
{
    public interface IPredictionService
    {
        // TODO: need a way to give the game ID or soemthing to this
        public Task<IEnumerable<PredictionResponse>> GetPredictionsForWeekAsync();
        public Task<PredictionResponse> GetPredictionsForEventAsync(string eventId);
        public Task<PredictionData> GetPredictionDataForEventAsync(string eventId);
    }

    public class PredictionService : IPredictionService
    {
        private readonly IPredictionDataService _predictionDataService;
        private readonly IOddsService _oddsService;
        private readonly IEventService _eventService;
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _exportDirectory;
        private readonly string _filePath;

        public PredictionService(IPredictionDataService predictionDataService, IOddsService oddsService, IEventService eventService, HttpClient httpClient, IMemoryCache cache)
        {
            _predictionDataService = predictionDataService;
            _oddsService = oddsService;
            _eventService = eventService;
            _cache = cache;
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

        public async Task<PredictionResponse> GetPredictionsForEventAsync(string eventId)
        {
            var e = await _eventService.GetEventByIdAsync(eventId);
            var predictionData = await _predictionDataService.GetPredictionDataForEvent(e);

            var url = "http://localhost:8000/predict";

            var predictionResult = new PredictionResponse{};

            try
            {
                var jsonContent = JsonSerializer.Serialize(predictionData, _jsonOptions);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending to FastAPI for {predictionData.HomeTeamName} vs {predictionData.AwayTeamName}:");
                Console.WriteLine(jsonContent.Substring(0, Math.Min(500, jsonContent.Length)) + "...");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<GamePrediction>(responseJson, _jsonOptions);

                    var (bestTotal, bestSpread) = await _oddsService.GetBestOdds(e);


                    if (result != null)
                    {
                        var predictionResponse = new PredictionResponse
                            {
                                HomeTeamName = predictionData.HomeTeamName,
                                AwayTeamName = predictionData.AwayTeamName,
                                EventId = e.Id,
                                Date = e.Date,
                                GamePrediction = new GamePredictionResponse
                                {
                                    SpreadPrediction = result.SpreadPrediction,
                                    SpreadRange = result.SpreadRange,
                                    TotalPrediction = result.TotalPrediction,
                                    TotalRange = result.TotalRange,
                                    WinnerPrediction = result.WinnerPrediction,
                                    HomeWinProbability = result.HomeWinProbability,
                                    AwayWinProbability = result.AwayWinProbability
                                },
                                VegasLowestSpread = bestSpread,
                                VegasLowestTotal = bestTotal,
                                VegasWinner = bestSpread.OddsValue < 0 ? "Home" : "Away",
                            };
                        predictionResult = predictionResponse;
                    }
                }   
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling FastAPI: {ex.Message}");
            }
            return predictionResult;
        }

        public async Task<IEnumerable<PredictionResponse>> GetPredictionsForWeekAsync()
        {
            //TODO: Stinky fix because 2026 just happened 
            int currentSeasonYear = DateTime.Now.Year - 1;

            var currentGames = await _eventService.GetEventsForCurrentWeek(currentSeasonYear);
            if (!currentGames.Any())
            {
                Console.WriteLine("No games found for the current week.");
                return new List<PredictionResponse>();
            }

            var predictionTasks = currentGames
                .Select(async e => (e, await GetPredictionDataForEventAsync(e.Id)))
                .ToList();

            var predictions = await Task.WhenAll(predictionTasks);

            var currentWeek = predictions.First().e.Week;

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

                        var (bestTotal, bestSpread) = await _oddsService.GetBestOdds(prediction.e);

                        if (result != null)
                        {
                            var predictionResponse = new PredictionResponse
                            {
                                HomeTeamName = prediction.Item2.HomeTeamName,
                                AwayTeamName = prediction.Item2.AwayTeamName,
                                EventId = prediction.e.Id,
                                Date = prediction.e.Date,
                                GamePrediction = new GamePredictionResponse
                                {
                                    SpreadPrediction = result.SpreadPrediction,
                                    SpreadRange = result.SpreadRange,
                                    TotalPrediction = result.TotalPrediction,
                                    TotalRange = result.TotalRange,
                                    WinnerPrediction = result.WinnerPrediction,
                                    HomeWinProbability = result.HomeWinProbability,
                                    AwayWinProbability = result.AwayWinProbability
                                },
                                VegasLowestSpread = bestSpread,
                                VegasLowestTotal = bestTotal,
                                VegasWinner = bestSpread.OddsValue < 0 ? "Home" : "Away",
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
        
        public async Task<PredictionData> GetPredictionDataForEventAsync(string eventId)
        {
            var cacheKey = $"predictiondata:{eventId}";

            if (_cache.TryGetValue(cacheKey, out PredictionData cachedPredictionData))
            {
                return cachedPredictionData;
            }

            var eventRef = new RefDto
            {
                Ref = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/{eventId}?lang=en&region=us"
            };

            var eventResponse = await _eventService.GetEventByRefAsync(eventRef);
            var predictionData = await _predictionDataService.GetPredictionDataForEvent(eventResponse);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(6)); 

            _cache.Set(cacheKey, predictionData, cacheOptions);

            return predictionData;
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
                worksheet.Cell(row, 4).Value = prediction.VegasLowestSpread.OddsValue;
                worksheet.Cell(row, 5).Value = prediction.VegasLowestTotal.OddsValue;
                worksheet.Cell(row, 6).Value = prediction.VegasWinner;

                if (gp != null)
                {
                    worksheet.Cell(row, 7).Value = gp.SpreadPrediction;
                    worksheet.Cell(row, 8).Value = gp.TotalPrediction;
                    worksheet.Cell(row, 9).Value = gp.WinnerPrediction;
                    worksheet.Cell(row, 10).Value = gp.HomeWinProbability;
                    worksheet.Cell(row, 11).Value = gp.AwayWinProbability;
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