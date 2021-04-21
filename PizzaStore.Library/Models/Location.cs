using PizzaStore.Data.Interfaces;
using PizzaStore.Data.Models;
using PizzaStore.Library.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaStore.Library.Models
{
    public class Location : ILocation
    {
        // user with no order history will get suggested order of 1 pizza at $8 each
        protected static IDictionary<decimal, int> DefaultSuggestedOrderPizzas = new Dictionary<decimal, int> { [8m] = 1 };

        protected IDictionary<IUser, Stack<IOrder>> OrderHistory { get; } = new Dictionary<IUser, Stack<IOrder>>();

        protected IDictionary<IUser, DateTime> LastOrderTime { get; } = new Dictionary<IUser, DateTime>();

        public Pslocation Dao { get; }

        public int ID { get; }

        public Location(int pieCount)
        {
            if (pieCount < 0)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(pieCount),
                                                      message: "inventory cannot be negative.");
            }
            PieCount = pieCount;
            Dao = new Pslocation()
            {
                Inventory = pieCount
            };
            PSDBContextProvider.Current.UpdateAndSave(Dao);
            if (Dao.LocationId == default)
            {
                throw new InvalidOperationException("could not register location.");
            }
            ID = Dao.LocationId;
        }

        public Location(Pslocation dao)
        {
            Dao = dao ?? throw new ArgumentNullException(paramName: nameof(dao));
            if (dao.Inventory < 0)
            {
                throw new ArgumentException(message: "inventory cannot be negative.",
                                            paramName: nameof(dao));
            }
            PieCount = dao.Inventory;
            ID = dao.LocationId;
        }

        // inventory
        public int PieCount { get; set; }

        // minimum time interval between orders
        public TimeSpan MinOrderInterval = new TimeSpan(hours: 2, minutes: 0, seconds: 0);

        // returns any rejected orders
        public ICollection<IOrder> Order(IUser user, ICollection<IOrder> orders)
        {
            if (user is null)
            {
                throw new ArgumentNullException(paramName: nameof(user));
            }
            if (orders is null)
            {
                throw new ArgumentNullException(paramName: nameof(orders));
            }
            var attemptedOrderTime = TimeProvider.Current.UtcNow;
            if (orders.Count > user.MaxOrdersPerCall)
            {
                throw new ArgumentException(message: $"user should not exceed {user.MaxOrdersPerCall} orders per call.",
                                            paramName: nameof(orders));
            }
            if (LastOrderTime.TryGetValue(user, out DateTime userLastOrderTime) &&
                attemptedOrderTime - userLastOrderTime < MinOrderInterval)
            {
                // user cannot order from here within 2 hours of last order
                return new List<IOrder>(orders);
            }
            ICollection<IOrder> rejectedOrders = new List<IOrder>();

            IPizzaStoreDBContext context = PSDBContextProvider.Current.NewPSDBContext;

            foreach (IOrder order in orders)
            {
                // reject if this order is for another location
                if (order.Location != this)
                {
                    rejectedOrders.Add(order);
                    continue;
                }
                // reject if this exact order has already been ordered
                if (order.Time != null)
                {
                    rejectedOrders.Add(order);
                    continue;
                }
                int piesRequired = order.PizzasByPrice.Sum(x => x.Value);
                // reject if there is not enough inventory for this order
                if (piesRequired > PieCount)
                {
                    rejectedOrders.Add(order);
                    continue;
                }
                if (!OrderHistory.TryGetValue(user, out Stack<IOrder> userOrderHistory))
                {
                    userOrderHistory = new Stack<IOrder>();
                    OrderHistory[user] = userOrderHistory;
                }
                userOrderHistory.Push(order);
                LastOrderTime[user] = attemptedOrderTime;
                order.Time = attemptedOrderTime;
                PieCount -= piesRequired;

                Dao.Inventory = PieCount;
                context.Update(Dao);
                context.Update(order.Dao);
            }

            context.SaveChanges();
            context.Dispose();

            return rejectedOrders;
        }

        public IOrder SuggestOrder(IUser user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(paramName: nameof(user));
            }
            if (OrderHistory.TryGetValue(user, out Stack<IOrder> userHistory) && userHistory.Count > 0)
            {
                // suggest user's last order
                return new Order(userHistory.Peek());
            }
            return new Order(pizzasByPrice: DefaultSuggestedOrderPizzas, location: this, user: user);
        }
    }
}
