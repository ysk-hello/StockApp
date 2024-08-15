using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockApp.Api.Models;

namespace StockApp.Api.Functions
{
    public class GetRsiFunction
    {
        private readonly ILogger<GetRsiFunction> _logger;

        public GetRsiFunction(ILogger<GetRsiFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GetRsiFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rsi/jpn/{code}")] HttpRequest req, string code)
        {
            if (code.Length != 4)
            {
                return new BadRequestResult();
            }

            var dbConnectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            var dbName = Environment.GetEnvironmentVariable("DbName");
            var dbContainerName = Environment.GetEnvironmentVariable("DbStockDataContainerName");
            var client = new CosmosClient(dbConnectionString);
            var dbContainer = client.GetContainer(dbName, dbContainerName);

            var query = dbContainer.GetItemLinqQueryable<StockData>().Where(d => d.CountryCompanyCode == $"{Country.JPN}/{code}".ToLower());
            var feeder = query.ToFeedIterator();
            var data = new List<StockData>();
            while (feeder.HasMoreResults)
            {
                var response = await feeder.ReadNextAsync();
                data.AddRange(response.ToList());
            }

            var rsi = CalcRsi(data);

            return new OkObjectResult(rsi);
        }

        private static Data CalcRsi(List<StockData> data)
        {
            // Š”‰¿ƒf[ƒ^
            data = data.OrderBy(d => d.Date).ToList();

            var diffDataList = new List<Data>();
            for (var i = 1; i < data.Count; i++)
            {
                var diff = new Data { Date = data[i].Date, Value = data[i].Close - data[i - 1].Close };
                diffDataList.Add(diff);
            }

            diffDataList = diffDataList.TakeLast(14).ToList();
            var minus = diffDataList.Where(d => d.Value < 0).Sum(d => d.Value);
            var plus = diffDataList.Where(d => d.Value >= 0).Sum(d => d.Value);

            var rsi = new Data { Date = diffDataList.Last().Date, Value = Math.Round(plus / (plus - minus) * 100, 2) };
           
            return rsi;
        }
    }
}
