using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;

// TODO: For this and I think team service, abstract event service out
namespace backend.Services
{
    public interface IOddsService
    {
        public Task<IEnumerable<Odds>> GetOddsAsync(RefDto oddsRef);
        public Task<(VegasPrediction, VegasPrediction)> GetBestOdds(Event e);
        public Task<IEnumerable<Odds>> GetOddsByEventId(string eventId);
    }

    public class OddsService : IOddsService
    {
        private readonly HttpClient _httpclient;
        private readonly Lazy<IEventService> _lazyEventService;

        public OddsService(HttpClient httpClient, Lazy<IEventService> eventServiceLazy)
        {
            _httpclient = httpClient;
            _lazyEventService = eventServiceLazy;
        }

        public async Task<IEnumerable<Odds>> GetOddsAsync(RefDto oddsRef)
        {
            var allOdds = new List<Odds>();

            if (oddsRef is null)
            {
                return allOdds;
            }

            // TODO: Think a little bit more about this
            // It is optimal for predictions to have the minimized spread and total to have the best odds to beat vegas.
            var oddsResponse = await _httpclient.GetFromJsonResilientAsync<OddsResponseDto>(oddsRef.Ref)
                ?? throw new Exception("Error fetchings odds response");

            allOdds = oddsResponse.Odds.Select(o => new Odds
            {
                Details = o.Details,
                OverUnder = o.OverUnder,
                Spread = o.Spread,
            }).ToList();

            return allOdds;
        }

        public async Task<(VegasPrediction, VegasPrediction)> GetBestOdds(Event e)
        {
            var allOdds = e.Competitions.FirstOrDefault().CompetitionOdds;

            var bestTotal = allOdds.Where(x => x.Details != null)
            .Select(o => new VegasPrediction
            {
                SportsBook = o.Provider,
                OddsValue = (double)o.OverUnder,
            }).OrderBy(o => o.OddsValue)
            
            .First();

            var bestSpread = allOdds.Where(x => x.Details != null)
            .Select(o => new VegasPrediction
            {
                SportsBook = o.Provider,
                OddsValue = (double)o.Spread,
            }).OrderBy(o => Math.Abs(o.OddsValue))
            .First();

            return (bestTotal, bestSpread);
        }

        public async Task<IEnumerable<Odds>> GetOddsByEventId(string eventId)
        {
            var e = await _lazyEventService.Value.GetEventByIdAsync(eventId);

            return e.Competitions.FirstOrDefault().CompetitionOdds;
        }
    }
}