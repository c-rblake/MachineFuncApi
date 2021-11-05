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
using MachineFuncApi.Extensions;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;

namespace MachineFuncApi
{
    public static class MachineApi
    {
        //[FunctionName("Function1")]
        //public static async Task<IActionResult> Run(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        //    ILogger log)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");

        //    string name = req.Query["name"];

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.name;

        //    string responseMessage = string.IsNullOrEmpty(name)
        //        ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
        //        : $"Hello, {name}. This HTTP triggered function executed successfully.";

        //    return new OkObjectResult(responseMessage);
        //}

        [FunctionName("Create")]
        public static async Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "machines")] HttpRequest req, //localhost/api/route //TRIGGER, INPUT BINDER, OUTPUT BINDER
        [Table("Machines", Connection = "AzureWebJobsStorage")] //3. DB nuggets Microsoft.Azure.Webjobs.Extensions.Storage 4.05. + Microsoft.Azure.Cosmos.Table 55
        //4. Connection from APP settings 70 //11 Machines will be the Name of the Table
        IAsyncCollector<MachineTableEntity> MachineTable, //5. IAsyncCollector + TableEntity
        ILogger log)
        {
            log.LogInformation("Create new Machine."); // Logs are the best, use them.

            //1. No dependency Injection. Add Startup class if needed. 25
            //string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            var machineDto = JsonConvert.DeserializeObject<MachineDto>(requestBody); //Machine only needs a String Name

            if (machineDto is null || string.IsNullOrWhiteSpace(machineDto?.Name)) return new BadRequestResult();

            var machine = new Machine { Name = machineDto.Name };

            //2. Table Storage 50

            await MachineTable.AddAsync(machine.ToTableEntity()); //Extension


            return new OkObjectResult(machine); //10 Go to Postman and check. Needs Azure function core tools installed. Ok 200
            //11 Check in Storage Emulator - Microsoft Azure Storage Emulator Local Attached. ToDO MISSING STATUS AND LOG.
        }

        //19 Get Function
        //Clear out all Code Comments https://stackoverflow.com/questions/3885723/how-to-delete-all-comments-in-a-selected-code-section
        [FunctionName("Get")]
        public static async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "machines")] HttpRequest req,         
        [Table("Machines", Connection = "AzureWebJobsStorage")] CloudTable MachineTable, //20. Cloud Table. COSMOS TABLE       
        ILogger log)
        {
            log.LogInformation("Get all Machines.");

            var query = new TableQuery<MachineTableEntity>(); //21 Queries go here. Left for null returns all.
            var result = await MachineTable.ExecuteQuerySegmentedAsync(query, null); //Returns Array of results

            //Nested List in a Class
            var response = new MachineList { Machines = result.Select(Mapper.ToMachine).ToList() }; //Send func over Lambda. 22
            //var response new Mahincelist { Machines = result.Select(m=> m.ToItem().ToList()); 

            return new OkObjectResult(response);             
        }
    }
}
