using MachineApiTwo.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MachineFuncApi.Models
{
    public class Machine
    {
        public string Id;
        public string Name { get; set; }

        public List<string> Log { get; set; }

        public bool Status { get; set; }

        public DateTime? ServiceDate { get; set; } = new DateTime(1979, 07, 28);
        public Machine()
        {
            Id = Guid.NewGuid().ToString("n");
        }

        public Category Category { get; set; }
    }
}
