// using backend.Services;
// using Microsoft.AspNetCore.Mvc;

// namespace backend.Models
// {
//     public interface IModelDataService
//     {
//         public Task<SpreadData> GetSpreadDataAsync();
//         public Task<TotalsData> GetTotalsDataAsync();
//         public Task<MoneyLineData> GetMoneyLineDataAsync();
//     }

//     public class ModelDataService : IModelDataService
//     {
//         private readonly ISeasonService _seasonService;
//         private List<Season> _seasonData = [];
//         public ModelDataService(ISeasonService seasonService)
//         {
//             _seasonService = seasonService;
//         }

//         // Make the call to get the seasons only once, this is expensive enough already
//         private async Task GetAllSeasonDataAsync(int startYear, int endYear)
//         {
//             var seasonList = await _seasonService.GetSeasonsRangedAsync(2020, 2025)
//                 ?? throw new Exception($"Error fetching season data for years {startYear} - {endYear}");

//             _seasonData = [.. seasonList];
//         }

//         public Task<SpreadData> GetSpreadDataAsync()
//         {

//         }

//         public Task<TotalsData> GetTotalsDataAsync()
//         {

//         }

//         public Task<MoneyLineData> GetMoneyLineDataAsync();
//         {

//         }
//     }
// }