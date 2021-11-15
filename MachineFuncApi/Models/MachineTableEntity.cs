using MachineApiTwo.Models;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace MachineFuncApi.Models
{
    //public class MachineTableEntity: TableEntity //7. HAS TO BE COSMOS.TABLE implmentation has been moved. 73 
    //{
    //    //75 E-tag of TableEntity used in Caching
    //    public string Name { get; set; }
    //    public Status Status { get; set; } = Status.Offline;
    //    public List<string> Log { get; set; } = new List<string>(); //null reference.
    //}

    public class MachineTableEntity : TableEntity //Updated for more in razor page
    {
        public string Id;
        public string Name { get; set; }

        public List<string> Log { get; set; }

        public bool Status { get; set; }

        public DateTime? ServiceDate { get; set; } = new DateTime(1979, 07, 28);
        public MachineTableEntity()
        {
            Id = Guid.NewGuid().ToString("n");
        }

        public Category Category { get; set; }
    }
}
