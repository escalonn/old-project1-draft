using PizzaStore.Data.Models;
using System;
using System.Collections.Generic;

namespace PizzaStore.Library.Interfaces
{
    public interface IOrder
    {
        Psorder Dao { get; }

        ILocation Location { get; }

        IUser User { get; }

        int ID { get; }

        DateTime? Time { get; set; }

        IDictionary<decimal, int> PizzasByPrice { get; }

        decimal TotalValueUsd { get; }

        int MaxPizzaCount { get; }

        decimal MaxTotalValueUsd { get; }
    }
}
