using backend.DTOs;
using backend.Models;
using backend.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services
{
    public interface ITeamService
    {
        public Task<Team> GetTeamAsync(RefDto teamRef, int currentYear);
        public Dictionary<string, List<CategoryStat>> GetCategorizedStats(Team team, int seasonYear);
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

            var Team = await FillTeamDataAsync(teamResponse, currentYear);

            return Team;
        }

        public Dictionary<string, List<CategoryStat>> GetCategorizedStats(Team team, int seasonYear)
        {
            var cacheKey = $"categorized_stats_{team.Id}_{seasonYear}";
            var currentYear = DateTime.Now.Year;

            if (seasonYear < currentYear && _cache.TryGetValue(cacheKey, out Dictionary<string, List<CategoryStat>> cachedCategories))
            {
                return cachedCategories;
            }

            var categorizedStats = team.Statistics.StatCategories
                .ToDictionary(sc => sc.Name, sc => sc.CategoryStats);

            if (seasonYear < currentYear)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                    Priority = CacheItemPriority.Normal
                };
                _cache.Set(cacheKey, categorizedStats, cacheOptions);
            }
            return categorizedStats;
        }

        // May want to abstract this to make it easier to get Ref Data for any ref since there are so many 
        private async Task<Team> FillTeamDataAsync(TeamResponseDto teamResponse, int currentYear)
        {
            var recordTask = GetTeamRecordAsync(teamResponse);
            var oddsRecordTask = GetTeamOddsRecordAsync(teamResponse);
            var statisticTask = GetTeamStatisticsAsync(teamResponse, currentYear);
            var injuriesTask = GetTeamInjuriesAsync(teamResponse);

            return new Team
            {
                Id = teamResponse.Id,
                Location = teamResponse.Location,
                Name = teamResponse.Name,
                DisplayName = teamResponse.DisplayName,
                Record = await recordTask,
                OddsRecord = await oddsRecordTask,
                Statistics = await statisticTask,
                Injuries = await injuriesTask,
            };
        }

        private async Task<Record> GetTeamRecordAsync(TeamResponseDto teamResponse)
        {
            var recordResponse = await _httpClient.GetFromJsonResilientAsync<RecordResponseDto>(teamResponse.RecordRef.Ref)
                ?? throw new Exception("Error fetching team record");

            var recordsByName = recordResponse.Records.ToDictionary(r => r.Name, r => r);

            var overallRecord = recordsByName["overall"];
            var homeRecord = recordsByName["Home"];
            var awayRecord = recordsByName["Road"];
            var conferenceRecord = recordsByName["vs. Conf."];

            var overallStats = overallRecord.Stats.ToDictionary(s => s.Name, s => s.Value);
            var homeStats = homeRecord.Stats.ToDictionary(s => s.Name, s => s.Value);
            var awayStats = awayRecord.Stats.ToDictionary(s => s.Name, s => s.Value);
            var confStats = conferenceRecord.Stats.ToDictionary(s => s.Name, s => s.Value);

            return new Record
            {
                AveragePointsAgainst = overallStats["avgPointsAgainst"],
                AveragePointsFor = overallStats["avgPointsFor"],
                PointDifferential = overallStats["differential"],
                DivisionWinPercent = overallStats["divisionWinPercent"],
                LeagueWinPercent = overallStats["leagueWinPercent"],
                Losses = Convert.ToInt16(overallStats["losses"]),
                Streak = Convert.ToInt32(overallStats["streak"]),
                Wins = Convert.ToInt32(overallStats["wins"]),
                Ties = Convert.ToInt32(overallStats["ties"]),
                WinPercent = overallStats["winPercent"],
                DivisionLosses = Convert.ToInt32(overallStats["divisionLosses"]),
                DivisionWins = Convert.ToInt32(overallStats["divisionWins"]),
                HomeWins = Convert.ToInt32(homeStats["wins"]),
                HomeLosses = Convert.ToInt32(homeStats["losses"]),
                AwayWins = Convert.ToInt32(awayStats["wins"]),
                AwayLosses = Convert.ToInt32(awayStats["losses"]),
                ConferenceLosses = Convert.ToInt32(confStats["losses"]),
                ConferenceWins = Convert.ToInt32(confStats["wins"])
            };
        }

        private async Task<OddsRecord> GetTeamOddsRecordAsync(TeamResponseDto teamResponse)
        {

            if (teamResponse.OddsRecordRef == null || string.IsNullOrEmpty(teamResponse.OddsRecordRef.Ref))
                return new OddsRecord { OddsStats = new List<OddsStat>() };

            var oddsResponse = await _httpClient.GetFromJsonResilientAsync<OddsRecordDto>(teamResponse.OddsRecordRef.Ref)
                ?? throw new Exception("Error fetching team record");

            var abbreviations = new[] { "ML", "ML HOME", "ML AWAY", "ML UND", "ML FAV",
                            "ATS", "ATS HOME", "ATS AWAY", "ATS UND", "ATS FAV" };

            var oddsDict = (oddsResponse?.BookOddsRecords ?? new List<BookOddsRecord>())
                 .ToDictionary(or => or.Abbreviation, or => or);

            var oddsRecordList = abbreviations
                .Where(abbr => oddsDict.ContainsKey(abbr))
                .Select(abbr => GetOddsStatForCategory(oddsDict[abbr]))
                .ToList();

            return new OddsRecord { OddsStats = oddsRecordList };
        }

        private OddsStat GetOddsStatForCategory(BookOddsRecord bookOdds)
        {
            var statsDict = bookOdds.OddsStats.ToDictionary(os => os.Abbreviation, os => os.Value);
            
            return new OddsStat
            {
                OddsRecord = bookOdds.ShortDisplayName,
                Wins = Convert.ToInt32(statsDict["W"]),
                Losses = Convert.ToInt32(statsDict["L"])
            };
        }

    // We can cache this to speed things up, the stats are the same for every week for any not current season team 
        private async Task<Statistics> GetTeamStatisticsAsync(TeamResponseDto teamResponse, int seasonYear)
        {
            var cacheKey = $"team_stats_{teamResponse.Id}";
            var currentYear = DateTime.Now.Year;

            // We also need to pass the year here
            if (seasonYear < currentYear && _cache.TryGetValue(cacheKey, out Statistics cachedStats))
            {
                return cachedStats;
            }

            var statsResponse = await _httpClient.GetFromJsonResilientAsync<StatisticsDto>(teamResponse.StatisticsRef.Ref)
                ?? throw new Exception("Error fetching team record");

            var statistics = new Statistics
            {
                StatCategories = statsResponse.Splits.StatCategories.Select(sc => new StatCategory
                {
                    Name = sc.Name,
                    DisplayName = sc.DisplayName,
                    CategoryStats = sc.CategoryStats.Select(cs => new CategoryStat
                    {
                        Name = cs.Name,
                        DisplayName = cs.DisplayName,
                        Value = cs.Value,
                        Rank = cs.Rank
                    }).ToList()
                }).ToList()
            };

            if (seasonYear < currentYear)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                    SlidingExpiration = TimeSpan.FromHours(1),
                    Priority = CacheItemPriority.Normal
                };
            
                _cache.Set(cacheKey, statistics, cacheOptions);
            }
            return statistics;
        }

        private async Task<Injuries> GetTeamInjuriesAsync(TeamResponseDto teamResponse)
        {
            var injuriesResponse = await _httpClient.GetFromJsonResilientAsync<InjuriesDto>(teamResponse.InjuriesRef.Ref)
                ?? throw new Exception("Error fetching team record");

            return new Injuries
            {
                InjuryCount = injuriesResponse.Count
            };
        }
    }
}