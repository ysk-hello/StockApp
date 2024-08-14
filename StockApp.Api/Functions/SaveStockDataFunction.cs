using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockApp.Api.Models;
using StockApp.Api.Web;

namespace StockApp.Api.Functions
{
    public class SaveStockDataFunction
    {
        private readonly ILogger<SaveStockDataFunction> _logger;

        public SaveStockDataFunction(ILogger<SaveStockDataFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SaveStockDataFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stock-data")] HttpRequest req)
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            var dbName = Environment.GetEnvironmentVariable("DbName");
            var dbContainerName = Environment.GetEnvironmentVariable("DbCompaniesContainerName");
            var client = new CosmosClient(dbConnectionString);
            var dbContainer = client.GetContainer(dbName, dbContainerName);

            #region 選択銘柄の取得

            var query = dbContainer.GetItemLinqQueryable<Company>().Where(c => c.IsSelected);
            var feeder = query.ToFeedIterator();
            var companies = new List<Company>();
            while (feeder.HasMoreResults)
            {
                var response = await feeder.ReadNextAsync();
                companies.AddRange(response.ToList());
            }

            #endregion

            dbContainerName = Environment.GetEnvironmentVariable("DbStockDataContainerName");
            dbContainer = client.GetContainer(dbName, dbContainerName);
            foreach (var company in companies)
            {
                System.Diagnostics.Debug.WriteLine(company.Name);

                // 株価データ取得
                var data = await KabutanAccess.GetStockDataAsync(company.Code, DateTime.Today.AddMonths(-1));

                foreach (var d in data)
                {
                    System.Diagnostics.Debug.WriteLine($"{d.Date} {d.Close}");
                    await dbContainer.UpsertItemAsync(d);
                }
            }

            return new OkResult();
        }
    }
}
