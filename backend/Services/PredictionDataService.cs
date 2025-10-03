using backend.Models;

namespace backend.Services
{
    public interface IPredictionDataService
    {
        public Task<IEnumerable<PredictionData>> GetPredictionDataForTimeframe(int startYear, int EndYear);

        // TODO: Need prediction data per event, as this is what we will be giving our model to predict the outcome
        public Task<PredictionData> GetPredictionDataForEventAsync(Event game);
    }

    public class PredictionDataService : IPredictionDataService
    {
        private readonly ISeasonService _seasonService;

        public PredictionDataService(ISeasonService seasonService)
        {
            _seasonService = seasonService;
        }

        public async Task<IEnumerable<PredictionData>> GetPredictionDataForTimeframe(int startYear, int EndYear)
        {
            var seasons = await _seasonService.GetSeasonsRangedAsync(2020, 2025);

            var predictionData = new List<PredictionData>();

            using var semaphore = new SemaphoreSlim(5);

            var tasks = seasons.Select(async season =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var seasonPredictionData = await GetPredictionDataBySeason(season);
                    lock (predictionData)
                    {
                        predictionData.AddRange(seasonPredictionData);
                    }
                }
                finally { semaphore.Release(); }
            });

            await Task.WhenAll(tasks);
            return predictionData.OrderBy(pd => pd.SeasonYear);
        }


        // TODO: Need to get season data and turn each event into a predictionData model
        private async Task<IEnumerable<PredictionData>> GetPredictionDataBySeason(int seasonNumber)
        {

        }

        public async Task<PredictionData> GetPredictionDataForEventAsync(Event game)
        {
            
        }
    }
}