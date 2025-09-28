using backend.DTOs;

// TODO:
// Again, we need to think about the transferring from weekDto to Week Model
namespace backend.Services
{
    public interface IWeeksService
    {
        public Task<IEnumerable<WeekDto>> GetAllWeeksForYearAsync(int seasonYear);

        public Task<WeekDto> GetWeekByWeekNumberAsync(int seasonYear, int weekNumber);
    }

    public class WeeksService : IWeeksService
    {
        private readonly HttpClient _httpClient;

        public WeeksService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task<WeeksResponseDto> GetWeekResponse(int seasonYear)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks?lang=en&region=us";

            var response = await _httpClient.GetFromJsonAsync<WeeksResponseDto>(url)
                ?? throw new Exception($"Error fetching weeks for {seasonYear}");

            return response;
        }

        public async Task<IEnumerable<WeekDto>> GetAllWeeksForYearAsync(int seasonYear)
        {
            var weekResponse = await GetWeekResponse(seasonYear);

            var weekList = new List<WeekDto>();

            // We will need to extract the Week model from this week Dto,
            // There is a lot of data caught in refs that we need in our model
            foreach (var weekRef in weekResponse.WeekRefs)
            {
                var url = weekRef.Ref;

                var week = await _httpClient.GetFromJsonAsync<WeekDto>(url)
                    ?? throw new Exception("Error fetching week");

                weekList.Add(week);
            }

            return weekList;
        }

        public async Task<WeekDto> GetWeekByWeekNumberAsync(int seasonYear, int weekNumber)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks/{weekNumber}?lang=en&region=us";

            var response = await _httpClient.GetFromJsonAsync<WeekDto>(url)
                ?? throw new Exception($"Week data not found for week {weekNumber} of the {seasonYear} season");

            return response;
        }
    }
}