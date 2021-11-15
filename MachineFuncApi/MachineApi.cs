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
//using MachineFuncApi.Shared; Trouble with asp vs .net
using MachineFuncApi.Extensions;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.Storage.Blob;

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

        [FunctionName("Post")]
        public static async Task<IActionResult> Post(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "machines")] HttpRequest req, //localhost/api/route //TRIGGER, INPUT BINDER, OUTPUT BINDER
        // Anonymous Authorization for post to work on web?? TODO
        [Table("Machines", Connection = "AzureWebJobsStorage")] //3. DB nuggets Microsoft.Azure.Webjobs.Extensions.Storage 4.05. + Microsoft.Azure.Cosmos.Table 55
        //4. Connection from APP settings 70 //11 Machines will be the Name of the Table
        IAsyncCollector<MachineTableEntity> MachineTable, //5. IAsyncCollector + TableEntity
        ILogger log)
        {
            log.LogInformation("Create new Machine."); // Logs are the best, use them.

            //1. No dependency Injection by default in functions. Add Startup class if needed. 25
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





        // 23. Put function find all ctrl H + Regex  //.*
        [FunctionName("Put")] //Does not WORK.
        public static async Task<IActionResult> Put(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "machines/{machineid}")] HttpRequest req, 
        [Table("Machines", Connection = "AzureWebJobsStorage")] CloudTable MachineTable, //25. Work with CloudTable COSMOS
        string machineid, // 24. To check ID MachineId for binding to work properly?
        ILogger log)
        {
            log.LogInformation("Update Machine."); 
                        
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var machine = JsonConvert.DeserializeObject<Machine>(requestBody); //26 MUST HAVE ETAG MACHINE OBJECT
            // Todo Make a MachineUpdateModel without the ID.

            if (machine is null || machine.Id != machineid) return new BadRequestResult();

            //27. Create Table Entity
            var tableEntity = machine.ToTableEntity();
            tableEntity.ETag = "*"; //Default etag.
            //29 TABLE ENTITY MUST HAVE ETAG
            var operation = TableOperation.Replace(tableEntity); //28. This is WHy need COSMOS CloudTable. Else it will not work.

            await MachineTable.ExecuteAsync(operation); 

            return new NoContentResult();                     
        }

        // 2.1 ToDoQueue Trigger Blob. ctrl H + Regex  //.*
        [FunctionName("Delete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "machines/{machineid}")] HttpRequest req,
            [Table("Machines","Machine", "{machineid}", Connection = "AzureWebJobsStorage")] MachineTableEntity machineToRemove, //2.3 Input binder to find object. 15
            [Table("Machines", Connection = "AzureWebJobsStorage")] CloudTable MachineTable,

            string machineid,
            ILogger log)
        {
            log.LogInformation($"Deleting item {machineid}");

            if (string.IsNullOrWhiteSpace(machineid)) return new BadRequestResult();

            // 2.2 Route one for delete. Create object to delete then delete. Not working
            //var MachineTableEntityToDelete = new MachineTableEntity
            //{
            //    PartitionKey = "machines",
            //    RowKey = machineid,
            //    ETag = "*" // ok to delete ALL concurrent versions of this object.
            //};
            //var operation = TableOperation.Delete(MachineTableEntityToDelete);

            var operation = TableOperation.Delete(machineToRemove);
            await MachineTable.ExecuteAsync(operation);

            return new NoContentResult();
        }

        //[FunctionName("RemoveComplete")] // 2.4 Start from scratch. Creates a QUEUE
        //public static async Task RemoveComplete(
        //    [TimerTrigger("0 */1 * * * *")]TimerInfo timer, // 2.5 Cron-expression 
        //    [Table("Machines", Connection = "AzureWebJobsStorage")] CloudTable machineTable,
        //    [Queue("MachineQueue", Connection = "AzureWebJobsStorage")] IAsyncCollector<Machine> queuedMachines, //2.6 On the fly Queue creation
        //    ILogger log)
        //{
        //    log.LogInformation("RemoveComplete Initiated...");

        //    var query = machineTable.CreateQuery<MachineTableEntity>().Where(m => m.Status == Status.Offline).AsTableQuery(); // 2.7 Import Cosmos

        //    var result = await machineTable.ExecuteQuerySegmentedAsync(query, null); // 2.8 Execute

        //    foreach(var machineTableEntity in result)
        //    {
        //        await queuedMachines.AddAsync(machineTableEntity.ToMachine());
        //        await machineTable.ExecuteAsync(TableOperation.Delete(machineTableEntity));

        //    }

        //}
        ////2.9 Queue Trigger to Blob storage
        //[FunctionName("GetRemoveFromQueue")]
        //public static async Task GetRemoveFromQueue(
        //    [QueueTrigger("MachineQueue", Connection = "AzureWebJobsStorage")] Machine machine,
        //    [Blob("done", Connection = "AzureWebJobsStorage")] CloudBlobContainer blobContainer, //3.0 Ms azure storage blob
        //     ILogger log)
        //{
        //    log.LogInformation("Queue trigger started...");

        //    await blobContainer.CreateIfNotExistsAsync(); // 3.1 Blob must be created if there isnt one
        //    var blob = blobContainer.GetBlockBlobReference($"{machine.MachineId}.txt"); //create a textfile out the item
        //    await blob.UploadTextAsync($"{machine.MachineId} is completed");
        //}


    }
}
