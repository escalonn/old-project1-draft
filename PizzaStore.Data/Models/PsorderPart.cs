using System;
using System.Collections.Generic;

namespace PizzaStore.Data.Models
{
    public partial class PsorderPart
    {
        public int OrderId { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }

        public Psorder Order { get; set; }
    }
}
