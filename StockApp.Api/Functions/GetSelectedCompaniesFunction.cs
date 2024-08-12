using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockApp.Api.Models;

namespace StockApp.Api.Functions
{
    public class GetSelectedCompaniesFunction
    {
        private readonly ILogger<GetSelectedCompaniesFunction> _logger;

        public GetSelectedCompaniesFunction(ILogger<GetSelectedCompaniesFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GetSelectedCompaniesFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "companies/selected")] HttpRequest req)
        {
            var dbConnectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            var dbName = Environment.GetEnvironmentVariable("DbName");
            var dbContainerName = Environment.GetEnvironmentVariable("DbContainerName");
            var client = new CosmosClient(dbConnectionString);
            var dbContainer = client.GetContainer(dbName, dbContainerName);

            var query = dbContainer.GetItemLinqQueryable<Company>().Where(c => c.IsSelected);
            var feeder = query.ToFeedIterator();
            var companies = new List<Company>();
            while (feeder.HasMoreResults)
            {
                var response = await feeder.ReadNextAsync();
                companies.AddRange(response.ToList());
            }

            return new OkObjectResult(companies);
        }
    }
}
