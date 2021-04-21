using PizzaStore.Data.Models;
using System.Collections.Generic;

namespace PizzaStore.Library.Interfaces
{
    public interface ILocation
    {
        Pslocation Dao { get; }

        int ID { get; }

        int PieCount { get; set; }

        ICollection<IOrder> Order(IUser user, ICollection<IOrder> orders);

        IOrder SuggestOrder(IUser user);
    }
}
