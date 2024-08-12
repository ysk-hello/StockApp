using Azure.Storage.Blobs;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockApp.Api.Models;

namespace StockApp.Api.Functions
{
    public class UploadCompanyFileFunction
    {
        private readonly ILogger<UploadCompanyFileFunction> _logger;

        public UploadCompanyFileFunction(ILogger<UploadCompanyFileFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(UploadCompanyFileFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "companies/upload")] HttpRequest req)
        {
            var files = req.Form.Files;
            if (files == null || files.Count != 1)
            {
                // 銘柄一覧ファイルを1つ添付してください
                return new BadRequestResult();
            }

            var file = files.Single();
            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
            {
                // .xlsxのファイルを添付してください
                return new BadRequestResult();
            }

            var storageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            var storageContainerName = Environment.GetEnvironmentVariable($"StorageContainerName");
            var blobContainerClient = new BlobContainerClient(storageConnectionString, storageContainerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            using (var fileStream = file.OpenReadStream())
            {
                var blobClient = blobContainerClient.GetBlobClient(file.FileName);
                // アップロード
                await blobClient.UploadAsync(fileStream, overwrite: true);

                var dbConnectionString = Environment.GetEnvironmentVariable("DbConnectionString");
                var dbName = Environment.GetEnvironmentVariable("DbName");
                var dbContainerName = Environment.GetEnvironmentVariable("DbContainerName");
                var client = new CosmosClient(dbConnectionString);
                var dbContainer = client.GetContainer(dbName, dbContainerName);

                var wb = new XLWorkbook(fileStream);
                var ws = wb.Worksheets.First();
                foreach (var row in ws.Rows().Skip(1))
                {
                    var message = $"{row.Cell("B").Value} {row.Cell("C").Value}";
                    System.Diagnostics.Debug.WriteLine(message);
                    Console.WriteLine(message);

                    var company = new Models.Company { Country = Country.JPN, Code = row.Cell("B").Value.ToString(), Name = row.Cell("C").Value.ToString() };

                    // CosmosDBを更新
                    await dbContainer.UpsertItemAsync(company);
                }
            }

            return new OkResult();
        }
    }
}
