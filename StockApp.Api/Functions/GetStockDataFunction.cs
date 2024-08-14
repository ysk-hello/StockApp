using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockApp.Api.Models;

namespace StockApp.Api.Functions
{
    public class GetStockDataFunction
    {
        private readonly ILogger<GetStockDataFunction> _logger;

        public GetStockDataFunction(ILogger<GetStockDataFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GetStockDataFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stock-data/jpn/{code}")] HttpRequest req, string code)
        {
            if(code.Length != 4)
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

            return new OkObjectResult(data.OrderBy(d => d.Date));
        }
    }
}
