using MachineFuncApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MachineFuncApi
{
    public static class Mapper //8. Map machine to and From Table Entity 77
    {
        public static MachineTableEntity ToTableEntity(this Machine machine)
        {
            return new MachineTableEntity
            {
                Name = machine.Name,
                Status = machine.Status,
                Log = machine.Log,
                PartitionKey = "Machine",
                RowKey = machine.MachineId
            };
        }

        public static Machine ToMachine(this MachineTableEntity tableEntity)
        {
            return new Machine
            {
                MachineId = tableEntity.RowKey,
                Name = tableEntity.Name,
                Log = tableEntity.Log,
                Status = tableEntity.Status
            };
        }


    }
}
