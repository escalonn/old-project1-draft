using System;
using System.Collections.Generic;

namespace PizzaStore.Data.Models
{
    public partial class Psuser
    {
        public Psuser()
        {
            Psorder = new HashSet<Psorder>();
        }

        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int DefaultLocationId { get; set; }

        public Pslocation DefaultLocation { get; set; }
        public ICollection<Psorder> Psorder { get; set; }
    }
}
