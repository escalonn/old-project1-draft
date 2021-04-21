using PizzaStore.Data.Models;
using PizzaStore.Library.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaStore.Library.Models
{
    public class User : IUser
    {
        public static int MaxNameChars { get; } = 128;

        private User(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                if (firstName is null)
                {
                    throw new ArgumentNullException(paramName: nameof(firstName));
                }
                throw new ArgumentException(message: "first name should not be empty or whitespace.",
                                            paramName: nameof(firstName));
            }
            if (firstName.Length > MaxNameChars)
            {
                throw new ArgumentException(message: $"first name should be at most {MaxNameChars} characters.",
                                            paramName: nameof(firstName));
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                if (lastName is null)
                {
                    throw new ArgumentNullException(paramName: nameof(lastName));
                }
                throw new ArgumentException(message: "last name should not be empty or whitespace.",
                                            paramName: nameof(lastName));
            }
            if (lastName.Length > MaxNameChars)
            {
                throw new ArgumentException(message: $"last name should be at most {MaxNameChars} characters.",
                                            paramName: nameof(lastName));
            }
            FirstName = firstName;
            LastName = lastName;
            DisplayName = $"{firstName} {lastName}";
        }

        public User(string firstName, string lastName, ILocation defaultLocation) : this(firstName, lastName)
        {
            DefaultLocation = defaultLocation ?? throw new ArgumentNullException(paramName: nameof(defaultLocation));
            Dao = new Psuser
            {
                FirstName = FirstName,
                LastName = LastName,
                DefaultLocation = DefaultLocation.Dao
            };
            PSDBContextProvider.Current.UpdateAndSave(Dao);
            if (Dao.UserId == default)
            {
                throw new InvalidOperationException("could not register user.");
            }
            AccountID = Dao.UserId;
        }

        public User(Psuser dao, ILocation defaultLocation) : this(dao.FirstName, dao.LastName)
        {
            Dao = dao ?? throw new ArgumentNullException(paramName: nameof(dao));

            if (defaultLocation.Dao.LocationId != dao.DefaultLocation.LocationId)
            {
                throw new InvalidOperationException("location inconsistent between dto and dao.");
            }

            FirstName = dao.FirstName;
            LastName = dao.LastName;
            AccountID = dao.UserId;
            DefaultLocation = defaultLocation ?? throw new ArgumentNullException(paramName: nameof(defaultLocation));
        }

        public Psuser Dao { get; }

        public int AccountID { get; }

        public string FirstName { get; }

        public string LastName { get; }

        public ILocation DefaultLocation { get; }

        public int MaxOrdersPerCall { get; } = 3;

        public ICollection<IOrder> PlaceOrders(ICollection<IOrder> orders)
        {
            if (orders.Count == 0)
            {
                return new List<IOrder>();
            }
            ILocation locationOfFirst = orders.First().Location;
            if (orders.Any(x => x.Location != locationOfFirst))
            {
                throw new ArgumentException(message: "orders should be associated to the same location",
                                            paramName: nameof(orders));
            }
            return locationOfFirst.Order(user: this, orders: orders);
        }

        public string DisplayName { get; }
    }
}
