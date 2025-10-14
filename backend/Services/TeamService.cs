using backend.DTOs;
using backend.Models;
using backend.Utilities;

namespace backend.Services
{
    public interface ITeamService
    {
        public Task<Team> GetTeamAsync(RefDto teamRef);
    }

    public class TeamService : ITeamService
    {
        private readonly HttpClient _httpClient;

        public TeamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Team> GetTeamAsync(RefDto teamRef)
        {
            var teamResponse = await _httpClient.GetFromJsonResilientAsync<TeamResponseDto>(teamRef.Ref)
                ?? throw new Exception("Error fetching team data");

            var Team = await FillTeamDataAsync(teamResponse);

            return Team;
        }

        // May want to abstract this to make it easier to get Ref Data for any ref since there are so many 
        private async Task<Team> FillTeamDataAsync(TeamResponseDto teamResponse)
        {
            var teamRecord = await GetTeamRecordAsync(teamResponse);
            var teamOddsRecord = await GetTeamOddsRecordAsync(teamResponse);
            var teamStatistics = await GetTeamStatisticsAsync(teamResponse);
            var teamInjuries = await GetTeamInjuriesAsync(teamResponse);

            return new Team
            {
                Id = teamResponse.Id,
                Location = teamResponse.Location,
                Name = teamResponse.Name,
                DisplayName = teamResponse.DisplayName,
                Record = teamRecord,
                OddsRecord = teamOddsRecord,
                Statistics = teamStatistics,
                Injuries = teamInjuries,
            };
        }

        private async Task<Record> GetTeamRecordAsync(TeamResponseDto teamResponse)
        {
            var recordResponse = await _httpClient.GetFromJsonResilientAsync<RecordResponseDto>(teamResponse.RecordRef.Ref)
                ?? throw new Exception("Error fetching team record");

            var overallRecord = recordResponse.Records.Where(r => r.Name == "overall").FirstOrDefault();
            var homeRecord = recordResponse.Records.Where(r => r.Name == "Home").FirstOrDefault();
            var awayRecord = recordResponse.Records.Where(r => r.Name == "Road").FirstOrDefault();
            var conferenceRecord = recordResponse.Records.Where(r => r.Name == "vs. Conf.").FirstOrDefault();

            // Since ESPN just gives us a list of "stat" objects, this is really the only option for getting the data
            // This is just not true, I just haven't thought about it enough...
            return new Record
            {
                AveragePointsAgainst = overallRecord.Stats.Where(rs => rs.Name == "avgPointsAgainst").FirstOrDefault().Value,
                AveragePointsFor = overallRecord.Stats.Where(rs => rs.Name == "avgPointsFor").FirstOrDefault().Value,
                PointDifferential = overallRecord.Stats.Where(rs => rs.Name == "differential").FirstOrDefault().Value,
                DivisionWinPercent = overallRecord.Stats.Where(rs => rs.Name == "divisionWinPercent").FirstOrDefault().Value,
                LeagueWinPercent = overallRecord.Stats.Where(rs => rs.Name == "leagueWinPercent").FirstOrDefault().Value,
                Losses = Convert.ToInt16(overallRecord.Stats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                Streak = Convert.ToInt32(overallRecord.Stats.Where(rs => rs.Name == "streak").FirstOrDefault().Value),
                Wins = Convert.ToInt32(overallRecord.Stats.Where(rs => rs.Name == "wins").FirstOrDefault().Value),
                Ties = Convert.ToInt32(overallRecord.Stats.Where(rs => rs.Name == "ties").FirstOrDefault().Value),
                WinPercent = overallRecord.Stats.Where(rs => rs.Name == "winPercent").FirstOrDefault().Value,
                DivisionLosses = Convert.ToInt32(overallRecord.Stats.Where(rs => rs.Name == "divisionLosses").FirstOrDefault().Value),
                DivisionWins = Convert.ToInt32(overallRecord.Stats.Where(rs => rs.Name == "divisionWins").FirstOrDefault().Value),
                HomeWins = Convert.ToInt32(homeRecord.Stats.Where(rs => rs.Name == "wins").FirstOrDefault().Value),
                HomeLosses = Convert.ToInt32(homeRecord.Stats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                AwayWins = Convert.ToInt32(awayRecord.Stats.Where(rs => rs.Name == "wins").FirstOrDefault().Value),
                AwayLosses = Convert.ToInt32(awayRecord.Stats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                ConferenceLosses = Convert.ToInt32(conferenceRecord.Stats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                ConferenceWins = Convert.ToInt32(conferenceRecord.Stats.Where(rs => rs.Name == "wins").FirstOrDefault().Value)
            };
        }

        // TODO: This is stupid, find a way to clean this up
        private async Task<OddsRecord> GetTeamOddsRecordAsync(TeamResponseDto teamResponse)
        {

            if (teamResponse.OddsRecordRef == null || string.IsNullOrEmpty(teamResponse.OddsRecordRef.Ref))
                return new OddsRecord { OddsStats = new List<OddsStat>() };

            var oddsResponse = await _httpClient.GetFromJsonResilientAsync<OddsRecordDto>(teamResponse.OddsRecordRef.Ref)
                ?? throw new Exception("Error fetching team record");

            var abbreviations = new[] { "ML", "ML HOME", "ML AWAY", "ML UND", "ML FAV",
                            "ATS", "ATS HOME", "ATS AWAY", "ATS UND", "ATS FAV" };

            var oddsRecords = oddsResponse?.BookOddsRecords ?? new List<BookOddsRecord>();
            var oddsList = abbreviations
                .Select(abbr => oddsRecords.FirstOrDefault(or => or.Abbreviation == abbr))
                .ToList();

            var oddsRecordList = oddsList
                .Select(o => GetOddsStatForCategory(o))
                .ToList();

            return new OddsRecord { OddsStats = oddsRecordList };
        }

        private OddsStat GetOddsStatForCategory(BookOddsRecord bookOdds)
        {
            return new OddsStat
            {
                OddsRecord = bookOdds.ShortDisplayName,
                Wins = Convert.ToInt32(bookOdds.OddsStats.Where(os => os.Abbreviation == "W").FirstOrDefault().Value),
                Losses = Convert.ToInt32(bookOdds.OddsStats.Where(os => os.Abbreviation == "L").FirstOrDefault().Value)
            };
        }

        private async Task<Statistics> GetTeamStatisticsAsync(TeamResponseDto teamResponse)
        {
            var statsResponse = await _httpClient.GetFromJsonResilientAsync<StatisticsDto>(teamResponse.StatisticsRef.Ref)
                ?? throw new Exception("Error fetching team record");

            return new Statistics
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