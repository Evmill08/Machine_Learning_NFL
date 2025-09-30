using backend.DTOs;
using backend.Models;

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
            var teamResponse = await _httpClient.GetFromJsonAsync<TeamResponseDto>(teamRef.Ref)
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
            var recordResponse = await _httpClient.GetFromJsonAsync<RecordResponseDto>(teamResponse.RecordRef.Ref)
                ?? throw new Exception("Error fetching team record");

            var overallRecord = recordResponse.Records.Where(r => r.Name == "overall").FirstOrDefault();
            var homeRecord = recordResponse.Records.Where(r => r.Name == "Home").FirstOrDefault();
            var awayRecord = recordResponse.Records.Where(r => r.Name == "Road").FirstOrDefault();
            var conferenceRecord = recordResponse.Records.Where(r => r.Name == "vs. Conf").FirstOrDefault();

            // Since ESPN just gives us a list of "stat" objects, this is really the only option for getting the data
            return new Record
            {
                AveragePointsAgainst = overallRecord.RecordStats.Where(rs => rs.Name == "avgPointsAgainst").FirstOrDefault().Value,
                AveragePointsFor = overallRecord.RecordStats.Where(rs => rs.Name == "avgPointsFor").FirstOrDefault().Value,
                PointDifferential = overallRecord.RecordStats.Where(rs => rs.Name == "differential").FirstOrDefault().Value,
                DivisionWinPercent = overallRecord.RecordStats.Where(rs => rs.Name == "divisionWinPercent").FirstOrDefault().Value,
                LeagueWinPercent = overallRecord.RecordStats.Where(rs => rs.Name == "leagueWinPercent").FirstOrDefault().Value,
                Losses = Convert.ToInt16(overallRecord.RecordStats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                Streak = Convert.ToInt32(overallRecord.RecordStats.Where(rs => rs.Name == "streak").FirstOrDefault().Value),
                Wins = Convert.ToInt32(overallRecord.RecordStats.Where(rs => rs.Name == "wins").FirstOrDefault().Value),
                Ties = Convert.ToInt32(overallRecord.RecordStats.Where(rs => rs.Name == "ties").FirstOrDefault().Value),
                WinPercent = overallRecord.RecordStats.Where(rs => rs.Name == "winPercent").FirstOrDefault().Value,
                DivisionLosses = Convert.ToInt32(overallRecord.RecordStats.Where(rs => rs.Name == "divisionLosses").FirstOrDefault().Value),
                DivisionWins = Convert.ToInt32(overallRecord.RecordStats.Where(rs => rs.Name == "divisionWins").FirstOrDefault().Value),
                HomeWins = Convert.ToInt32(homeRecord.RecordStats.Where(rs => rs.Name == "wins").FirstOrDefault().Value),
                HomeLosses = Convert.ToInt32(homeRecord.RecordStats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                AwayWins = Convert.ToInt32(awayRecord.RecordStats.Where(rs => rs.Name == "wins").FirstOrDefault().Value),
                AwayLosses = Convert.ToInt32(awayRecord.RecordStats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                ConferenceLosses = Convert.ToInt32(conferenceRecord.RecordStats.Where(rs => rs.Name == "losses").FirstOrDefault().Value),
                ConferenceWins = Convert.ToInt32(conferenceRecord.RecordStats.Where(rs => rs.Name == "wins").FirstOrDefault().Value)
            };
        }

        // TODO: This is stupid, find a way to clean this up
        private async Task<OddsRecord> GetTeamOddsRecordAsync(TeamResponseDto teamResponse)
        {
            var oddsResponse = await _httpClient.GetFromJsonAsync<OddsRecordDto>(teamResponse.OddsRecordRef.Ref)
                ?? throw new Exception("Error fetching team record");

            var ML = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ML").FirstOrDefault();
            var homeML = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ML HOME").FirstOrDefault();
            var awayML = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ML AWAY").FirstOrDefault();
            var underdogML = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ML UND").FirstOrDefault();
            var favoriteML = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ML FAV").FirstOrDefault();
            var Spread = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ATS").FirstOrDefault();
            var homeSpread = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ATS HOME").FirstOrDefault();
            var awaySpread = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ATS AWAY").FirstOrDefault();
            var underdogSpread = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ATS UND").FirstOrDefault();
            var favoriteSpread = oddsResponse.OddsRecords.Where(or => or.Abbreviation == "ATS FAV").FirstOrDefault();

            var OddsList = new List<BookOddsRecord> { ML, homeML, awayML, underdogML, favoriteML, Spread, homeSpread, awaySpread, underdogSpread, favoriteSpread };
            var OddsRecordList = new List<OddsStat>();

            foreach (var odds in OddsList)
            {
                OddsRecordList.Add(GetOddsStatForCategory(odds));
            }

            return new OddsRecord
            {
                OddsStats = OddsRecordList
            };
        }

        private OddsStat GetOddsStatForCategory(BookOddsRecord bookOdds)
        {
            return new OddsStat
            {
                OddsRecord = bookOdds.ShortDisplayName,
                Wins = Convert.ToInt32(bookOdds.OddsRecordStats.Where(os => os.Abbreviation == "W").FirstOrDefault().Value),
                Losses = Convert.ToInt32(bookOdds.OddsRecordStats.Where(os => os.Abbreviation == "L").FirstOrDefault().Value)
            };
        }

        private async Task<Statistics> GetTeamStatisticsAsync(TeamResponseDto teamResponse)
        {
            var statsResponse = await _httpClient.GetFromJsonAsync<StatisticsDto>(teamResponse.StatsRef.Ref)
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
            var injuriesResponse = await _httpClient.GetFromJsonAsync<InjuriesDto>(teamResponse.InjuriesRef.Ref)
                ?? throw new Exception("Error fetching team record");

            return new Injuries
            {
                InjuryCount = injuriesResponse.Count
            };
        }
    }
}