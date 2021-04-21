using PizzaStore.Library.Interfaces;
using PizzaStore.Library.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace PizzaStore.Tests
{
    public class UserTest : AFakeDBTest
    {
        [Fact]
        public void UsersWithNullFirstNameShouldBeInvalid()
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                new User(firstName: null, lastName: "Belotte", defaultLocation: new Location(1)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void UsersWithWhiteSpaceFirstNameShouldBeInvalid(string first)
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                new User(firstName: first, lastName: "Belotte", defaultLocation: new Location(1)));
        }

        [Fact]
        public void UsersWithTooLongFirstNameShouldBeInvalid()
        {
            var first = new string('A', User.MaxNameChars + 1);
            Assert.ThrowsAny<ArgumentException>(() =>
                new User(firstName: first, lastName: "Belotte", defaultLocation: new Location(1)));
        }

        [Fact]
        public void UsersWithNullLastNameShouldBeInvalid()
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                new User(firstName: "Fred", lastName: null, defaultLocation: new Location(1)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void UsersWithWhiteSpaceLastNameShouldBeInvalid(string last)
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                new User(firstName: "Fred", lastName: last, defaultLocation: new Location(1)));
        }

        [Fact]
        public void UsersWithTooLongLastNameShouldBeInvalid()
        {
            var last = new string('A', User.MaxNameChars + 1);
            Assert.ThrowsAny<ArgumentException>(() =>
                new User(firstName: "Fred", lastName: last, defaultLocation: new Location(1)));
        }

        [Fact]
        public void UsersWithNullDefaultLocationShouldBeInvalid()
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                new User(firstName: "Fred", lastName: "Belotte", defaultLocation: null));
        }

        [Fact]
        public void UserMaxOrdersPerCallShouldBe3()
        {
            User user = CreateUser();
            Assert.Equal(expected: 3, actual: user.MaxOrdersPerCall);
        }

        [Fact]
        public void UserShouldHaveUniqueAccountID()
        {
            var location = new Location(1);
            var user1 = CreateUser(location);
            var user2 = CreateUser(location);
            Assert.NotEqual(user1.AccountID, user2.AccountID);
        }

        [Fact]
        public void UserShouldStoreDefaultLocation()
        {
            var location = new Location(1);
            var user = CreateUser(location);
            Assert.Equal(expected: location, actual: user.DefaultLocation);
        }

        [Fact]
        public void UserShouldBeAbleToOrderFromLocation()
        {
            User user = CreateUser();
            var order = new Order(pizzasByPrice: new Dictionary<decimal, int> { [10m] = 1 },
                                  location: user.DefaultLocation,
                                  user: user);
            var orders = new List<IOrder> { order };
            ICollection<IOrder> rejected = null;
            Exception ex = Record.Exception(() => rejected = user.PlaceOrders(orders));
            Assert.Null(ex);
            Assert.Empty(rejected);
        }

        [Fact]
        public void UserShouldNotBeAbleToOrderFromMultipleLocationsAtOnce()
        {
            User user = CreateUser();
            var order1 = new Order(pizzasByPrice: new Dictionary<decimal, int> { [10m] = 1 },
                                  location: user.DefaultLocation,
                                  user: user);
            var order2 = new Order(pizzasByPrice: new Dictionary<decimal, int> { [15m] = 1 },
                                  location: new Location(1),
                                  user: user);
            var orders = new List<IOrder> { order1, order2 };
            Assert.ThrowsAny<ArgumentException>(() => user.PlaceOrders(orders));
        }

        public static User CreateUser(ILocation location = null)
        {
            var user = new User(firstName: "Fred", lastName: "Belotte", defaultLocation: location ?? new Location(1));
            return user;
        }
    }
}
