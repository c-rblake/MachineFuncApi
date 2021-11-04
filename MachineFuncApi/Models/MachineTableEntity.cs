using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace MachineFuncApi.Models
{
    public class MachineTableEntity: TableEntity //7. HAS TO BE COSMOS.TABLE implmentation has been moved. 73 
    {
        //75 E-tag of TableEntity used in Caching
        public string Name { get; set; }
        public Status Status { get; set; } = Status.Offline;
        public List<string> Log { get; set; } = new List<string>(); //null reference.
    }
}
