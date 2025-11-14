using backend.DTOs;
using backend.Models;
using backend.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services
{
    public interface ITeamService
    {
        public Task<Team> GetTeamAsync(RefDto teamRef, int currentYear);
    }

    public class TeamService : ITeamService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public TeamService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<Team> GetTeamAsync(RefDto teamRef, int currentYear)
        {
            var teamResponse = await _httpClient.GetFromJsonResilientAsync<TeamResponseDto>(teamRef.Ref)
                ?? throw new Exception("Error fetching team data");

            var Team = new Team
            {
                Id = teamResponse.Id,
                Location = teamResponse.Location,
                Name = teamResponse.Name,
                DisplayName = teamResponse.DisplayName,
            };

            return Team;
        }
    }
}