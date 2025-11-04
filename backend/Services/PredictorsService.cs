using backend.DTOs;
using backend.Models;
using backend.Utilities;

// TODO: Optimize this service
namespace backend.Services
{
    public interface IPredictorsService
    {
        public Task<Predictors> GetPredictionsAsync(RefDto predictorRef);
    }

    public class PredictorsService : IPredictorsService
    {
        private readonly HttpClient _httpClient;

        public PredictorsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Predictors> GetPredictionsAsync(RefDto predictorRef)
        {
            var predictorResponse = await _httpClient.GetFromJsonResilientAsync<PredictorDto>(predictorRef.Ref)
                ?? throw new Exception("Error fetching predictors");

            return new Predictors
            {
                Name = predictorResponse.Name,
                ShortName = predictorResponse.ShortName,
                LastModified = DateTime.Parse(predictorResponse.LastModified, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                HomeTeamPredictors = new TeamPredictor
                {
                    Predictors = predictorResponse.HomeTeamPredictions.Predictors.
                        Select(p => new Statistic
                        {
                            Name = p.Name,
                            DisplayName = p.DisplayName,
                            Description = p.Description,
                            Value = p.Value,
                            DisplayValue = p.DisplayValue
                        }).ToList()
                },
                AwayTeamPredictors = new TeamPredictor
                {
                    Predictors = predictorResponse.AwayTeamPredictions.Predictors.
                        Select(p => new Statistic
                        {
                            Name = p.Name,
                            DisplayName = p.DisplayName,
                            Description = p.Description,
                            Value = p.Value,
                            DisplayValue = p.DisplayValue
                        }).ToList()
                }
            };
        }
    }
}