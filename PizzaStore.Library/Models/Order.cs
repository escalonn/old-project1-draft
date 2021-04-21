using PizzaStore.Data.Models;
using PizzaStore.Library.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PizzaStore.Library.Models
{
    public class Order : IOrder
    {
        private DateTime? _time;

        private Order(IDictionary<decimal, int> pizzasByPrice, DateTime? time = null)
        {
            ValidatePizzas(pizzasByPrice);
            Time = time;
            
            TotalValueUsd = pizzasByPrice.Sum(x => x.Key * x.Value);
            if (TotalValueUsd > MaxTotalValueUsd)
            {
                throw new ArgumentException(message: $"order should not exceed ${MaxTotalValueUsd} in total value.",
                                            paramName: nameof(pizzasByPrice));
            }
            // make immutable dictionary without 0-pizza entries
            PizzasByPrice = pizzasByPrice.Where(p => p.Value != 0).ToImmutableDictionary(p => p.Key, p => p.Value);
        }

        public Order(IDictionary<decimal, int> pizzasByPrice, ILocation location, IUser user) : this(pizzasByPrice)
        {
            Location = location ?? throw new ArgumentNullException(paramName: nameof(location));
            User = user ?? throw new ArgumentNullException(paramName: nameof(user));

            Dao = new Psorder
            {
                Location = Location.Dao,
                User = User.Dao,
                OrderTime = Time
            };
            foreach (KeyValuePair<decimal, int> entry in PizzasByPrice)
            {
                Dao.PsorderPart.Add(new PsorderPart() { Price = entry.Key, Qty = entry.Value });
            }
            Dao.Location = Location.Dao;
            Dao.User = User.Dao;
            Dao.OrderTime = Time;
            PSDBContextProvider.Current.UpdateAndSave(Dao);
            if (Dao.OrderId == default)
            {
                throw new InvalidOperationException("could not register order.");
            }
            ID = Dao.OrderId;
        }

        public Order(IOrder other) : this(other.PizzasByPrice, other.Location, other.User) { }

        public Order(Psorder dao, ILocation location, IUser user)
        {
            Dao = dao ?? throw new ArgumentNullException(paramName: nameof(dao));

            if (location.Dao.LocationId != dao.Location.LocationId)
            {
                throw new InvalidOperationException("location inconsistent between dto and dao.");
            }
            Location = location;
            if (user.Dao.UserId != dao.User.UserId)
            {
                throw new InvalidOperationException("user inconsistent between dto and dao.");
            }
            User = user;
            Time = Dao.OrderTime;
            ID = Dao.OrderId;

            IDictionary<decimal, int> pizzasByPrice = Dao.PsorderPart.ToDictionary(x => x.Price, x => x.Qty);
            ValidatePizzas(pizzasByPrice);
            PizzasByPrice = pizzasByPrice.Where(p => p.Value != 0).ToImmutableDictionary(p => p.Key, p => p.Value);

            TotalValueUsd = pizzasByPrice.Sum(x => x.Key * x.Value);
            if (TotalValueUsd > MaxTotalValueUsd)
            {
                throw new ArgumentException(message: $"order should not exceed ${MaxTotalValueUsd} in total value.",
                                            paramName: nameof(pizzasByPrice));
            }
        }

        public Psorder Dao { get; }

        // price in USD maps to number of pizzas at price
        public IDictionary<decimal, int> PizzasByPrice { get; }

        public ILocation Location { get; }

        public IUser User { get; }

        public int ID { get; }
        
        // can change from null only once
        public DateTime? Time
        {
            get => _time;
            set
            {
                if (value != null)
                {
                    if (_time != null)
                    {
                        throw new InvalidOperationException("must not change recorded order time.");
                    }
                    _time = value;
                    Dao.OrderTime = value;
                    // caller is responsible for updating database!
                }
            }
        }

        public decimal TotalValueUsd { get; }

        public int MaxPizzaCount { get; } = 12;

        public decimal MaxTotalValueUsd { get; } = 500m;

        private void ValidatePizzas(IDictionary<decimal, int> pizzasByPrice)
        {
            if (pizzasByPrice is null)
            {
                throw new ArgumentNullException(paramName: nameof(pizzasByPrice));
            }
            if (pizzasByPrice.Any(p => p.Key < 0))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(pizzasByPrice),
                                                      message: "order cannot contain pizzas with negative price.");
            }
            if (pizzasByPrice.Any(p => p.Value < 0))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(pizzasByPrice),
                                                      message: "order cannot contain negative pizza counts.");
            }
            int pizzaCount = pizzasByPrice.Sum(x => x.Value);
            if (pizzaCount < 1)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(pizzasByPrice),
                                                      message: "order should contain at least 1 pizza.");
            }
            if (pizzaCount > MaxPizzaCount)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(pizzasByPrice),
                                                      message: $"order should not exceed {MaxPizzaCount} pizzas per order.");
            }
            decimal totalValueUsd = pizzasByPrice.Sum(x => x.Key * x.Value);
            if (totalValueUsd > MaxTotalValueUsd)
            {
                throw new ArgumentException(message: $"order should not exceed ${MaxTotalValueUsd} in total value.",
                                            paramName: nameof(pizzasByPrice));
            }
        }
    }
}
