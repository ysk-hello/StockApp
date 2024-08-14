using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockApp.Api.Models;
using System.Text;

namespace StockApp.Api.Functions
{
    public class UpdateSelectedCompanyFunction
    {
        private readonly ILogger<UpdateSelectedCompanyFunction> _logger;

        public UpdateSelectedCompanyFunction(ILogger<UpdateSelectedCompanyFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(UpdateSelectedCompanyFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "companies/update")] HttpRequest req)
        {
            var input = await new StreamReader(req.Body, Encoding.UTF8).ReadToEndAsync();
            var inputModel = UpdateSelectedCompanyRequestModel.Parse(input);

            if (inputModel == null)
            {
                return new BadRequestResult();
            }

            var dbConnectionString = Environment.GetEnvironmentVariable("DbConnectionString");
            var dbName = Environment.GetEnvironmentVariable("DbName");
            var dbContainerName = Environment.GetEnvironmentVariable("DbCompaniesContainerName");
            var client = new CosmosClient(dbConnectionString);
            var dbContainer = client.GetContainer(dbName, dbContainerName);

            var company = await dbContainer.ReadItemAsync<Company>(inputModel.Code, new((int)inputModel.Country));
            company.Resource.IsSelected = inputModel.IsSelected;

            await dbContainer.UpsertItemAsync(company.Resource);

            return new OkObjectResult(company.Resource);
        }
    }
}
