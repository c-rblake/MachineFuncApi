using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MachineFuncApi.Models;

namespace MachineFuncApi
{
    public static class MachineApi
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("Create")]
        public static async Task<IActionResult> Create(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "machines")] HttpRequest req, //localhost/api/route
    ILogger log)
        {
            log.LogInformation("Create new Machine."); // Logs are the best, use them.


            //No dependency Injection. Add Startup class if needed.
            //string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            var machineDto = JsonConvert.DeserializeObject<MachineDto>(requestBody); //Machine only needs a String Name

            if (machineDto is null || string.IsNullOrWhiteSpace(machineDto?.Name)) return new BadRequestResult();

            var machine = new Machine { Name = machineDto.Name };


           



            return new OkObjectResult();
        }
    }
}
