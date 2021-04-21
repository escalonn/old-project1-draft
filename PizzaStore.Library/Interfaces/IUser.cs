using PizzaStore.Data.Models;
using System.Collections.Generic;

namespace PizzaStore.Library.Interfaces
{
    public interface IUser
    {
        Psuser Dao { get; }

        int AccountID { get; }

        ILocation DefaultLocation { get; }

        string FirstName { get; }

        string LastName { get; }

        int MaxOrdersPerCall { get; }

        ICollection<IOrder> PlaceOrders(ICollection<IOrder> orders);

        string DisplayName { get; }
    }
}
