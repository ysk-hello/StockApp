using Irony.Parsing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockApp.Api.Models;
using StockApp.Api.Web;
using System;

namespace StockApp.Api.Functions
{
    public class SaveStockDataTaskFunction
    {
        private readonly ILogger _logger;

        public SaveStockDataTaskFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SaveStockDataTaskFunction>();
        }

        // azure functions timezone変更
        // https://qiita.com/fukasawah/items/c2484b28b17c5fa20328
        // Tokyo Standard Timeを指定

        [Function("SaveStockDataTaskFunction")]
        public async Task<IActionResult> Run([TimerTrigger("%TimerScheduleSaveStockDataTask%")] TimerInfo myTimer)
        {
            _logger.LogInformation($"{nameof(SaveStockDataTaskFunction)}: start");

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

            _logger.LogInformation($"{nameof(SaveStockDataTaskFunction)}: end");

            return new OkResult();
        }
    }
}
