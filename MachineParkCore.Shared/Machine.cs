using System;
using System.Collections.Generic;
using System.Text;

namespace MachineParkCore.Shared
{
    public class Machine
    {
        public string MachineId { get; set; }
        public string Name { get; set; }

        public Status Status { get; set; } = Status.Offline;

        public List<string> Log { get; set; } = new List<string>(); //null reference.

        public Machine()
        {
            MachineId = Guid.NewGuid().ToString("n");
        }
    }
    public enum Status
    {
        Online,
        Offline
    }
}
