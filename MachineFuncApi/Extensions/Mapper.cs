using MachineFuncApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineFuncApi.Extensions
{
    public static class Mapper //8. Map machine to and From Table Entity 77
    {
        // Table storage is a very cheap storage. XBOX live is on table storage.
        //9. Create 1:20 https://www.youtube.com/watch?v=WWimijQqteY Create File(s) inside folder at once.
        // Add new file Extension => extension manager shift+f2
        // file1.cs, file2.cs or IMapper => interface.
        public static MachineTableEntity ToTableEntity(this Machine machine)
        {
            return new MachineTableEntity
            {
                RowKey = machine.Id,
                Name = machine.Name,
                Category = machine.Category,
                ServiceDate = machine.ServiceDate,
                Log = machine.Log,
                Status = machine.Status,
                PartitionKey = "Machine"
            };
        }

        public static Machine ToMachine(this MachineTableEntity tableEntity)
        {
            return new Machine
            {
                Id = tableEntity.RowKey,
                Name = tableEntity.Name,
                Category = tableEntity.Category,
                ServiceDate = tableEntity.ServiceDate,
                Log = tableEntity.Log,
                Status = tableEntity.Status
            };
        }


    }
}
