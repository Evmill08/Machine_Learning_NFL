using backend.DTOs;
using backend.Models;
using backend.Utilities;

namespace backend.Services
{
    public interface IOddsService
    {
        public Task<Odds> GetOddsAsync(RefDto oddsRef);
    }

    public class OddsService : IOddsService
    {
        private readonly HttpClient _httpclient;

        public OddsService(HttpClient httpClient)
        {
            _httpclient = httpClient;
        }

        public async Task<Odds> GetOddsAsync(RefDto oddsRef)
        {
            // This is a fix for missing data, we may just want to wipe this data from the model. 
            if (oddsRef is null)
            {
                return new Odds
                {
                    Details = "",
                    AverageOverUnder = 0,
                    AverageSpread = 0,
                    MoneyLineWinner = false,
                    SpreadWinner = false
                };
            }

            // TODO: Think a little bit more about this
            // It is optimal for predictions to have the minimized spread and total to have the best odds to beat vegas.
            var oddsResponse = await _httpclient.GetFromJsonResilientAsync<OddsResponseDto>(oddsRef.Ref)
                ?? throw new Exception("Error fetchings odds response");

            // Can probably clean this up a bit, but since 2025 games don't have odds data for future weeks, we can get divide by 0 errors 
            var minimizedTotal = (oddsResponse.Odds.Sum(x => x.OverUnder) == 0 || oddsResponse.Odds.Count == 0)
                ? 0
                : oddsResponse.Odds.Min(x => x.OverUnder);

            var minimizedSpread = (oddsResponse.Odds.Sum(x => x.Spread) == 0 || oddsResponse.Odds.Count == 0)
                ? 0
                : oddsResponse.Odds.Min(x => x.Spread);

            var MlWinner = (oddsResponse.Odds.Count > 0) && oddsResponse.Odds.ElementAt(0).MoneyLineWinner;
            var AtsWinner = (oddsResponse.Odds.Count > 0) && oddsResponse.Odds.ElementAt(0).SpreadWinner;

            // TODO: This is not how we want to handle this data. Need to think about this more
            return new Odds
            {
                Details = "",
                AverageOverUnder = minimizedTotal, // if mins are what we go with, change model names
                AverageSpread = minimizedSpread,
                MoneyLineWinner = MlWinner,
                SpreadWinner = AtsWinner,
            };
        }
    }
}