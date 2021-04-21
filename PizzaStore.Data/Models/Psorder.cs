using System;
using System.Collections.Generic;

namespace PizzaStore.Data.Models
{
    public partial class Psorder
    {
        public Psorder()
        {
            PsorderPart = new HashSet<PsorderPart>();
        }

        public int OrderId { get; set; }
        public int LocationId { get; set; }
        public int UserId { get; set; }
        public DateTime? OrderTime { get; set; }

        public Pslocation Location { get; set; }
        public Psuser User { get; set; }
        public ICollection<PsorderPart> PsorderPart { get; set; }
    }
}
