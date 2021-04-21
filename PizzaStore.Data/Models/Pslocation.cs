using System;
using System.Collections.Generic;

namespace PizzaStore.Data.Models
{
    public partial class Pslocation
    {
        public Pslocation()
        {
            Psorder = new HashSet<Psorder>();
            Psuser = new HashSet<Psuser>();
        }

        public int LocationId { get; set; }
        public int Inventory { get; set; }

        public ICollection<Psorder> Psorder { get; set; }
        public ICollection<Psuser> Psuser { get; set; }
    }
}
