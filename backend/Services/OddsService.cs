using backend.DTOs;
using backend.Models;

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
            var oddsResponse = await _httpclient.GetFromJsonAsync<OddsResponseDto>(oddsRef.Ref)
                ?? throw new Exception("Error fetchings odds response");

            var averageTotal = oddsResponse.Odds.Sum(x => x.OverUnder) / oddsResponse.Odds.Count;
            var averageSpread = oddsResponse.Odds.Sum(x => x.Spread) / oddsResponse.Odds.Count;

            // TODO: This is not how we want to handle this data. Need to think about this more
            return new Odds
            {
                Details = "",
                AverageOverUnder = averageTotal,
                AverageSpread = averageSpread,
                MoneyLineWinner = oddsResponse.Odds.ElementAt(0).MoneyLineWinner,
                SpreadWinner = oddsResponse.Odds.ElementAt(0).SpreadWinner,
            };
        }
    }
}