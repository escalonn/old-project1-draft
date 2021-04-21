using PizzaStore.Library.Interfaces;
using PizzaStore.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PizzaStore.Tests
{
    public class LocationTest : AFakeDBTest
    {
        [Fact]
        public void LocationWithNegativeInventoryShouldBeInvalid()
        {
            Assert.ThrowsAny<ArgumentException>(() => new Location(pieCount: -2));
        }

        [Fact]
        public void LocationShouldStoreInventory()
        {
            var pieCount = 27;
            var location = new Location(pieCount);
            Assert.Equal(pieCount, location.PieCount);
        }

        [Fact]
        public void LocationMinOrderIntervalShouldBe2Hours()
        {
            var location = new Location(1);
            var twoHours = new TimeSpan(hours: 2, minutes: 0, seconds: 0);
            Assert.Equal(twoHours, location.MinOrderInterval);
        }

        [Fact]
        public void LocationShouldSuggestSomeOrder()
        {
            var location = new Location(1);
            var user = CreateUser(location);
            IOrder order = location.SuggestOrder(user);
            Assert.NotNull(order);
        }

        [Fact]
        public void LocationShouldNotAllowUserToPlaceTooManyOrdersAtOnce()
        {
            var location = new Location(1);
            var user = CreateUser(location);
            IOrder order = location.SuggestOrder(user);
            int tooMany = user.MaxOrdersPerCall + 1;
            List<IOrder> orders = Enumerable.Repeat(order, tooMany).ToList();
            Assert.ThrowsAny<ArgumentException>(() => location.Order(user, orders));
        }

        public static IEnumerable<object[]> GetOrderValuesAndAdequateInventories()
        {
            yield return new object[] { new[] { new Dictionary<decimal, int> { [10m] = 6 } }, 6 };
            yield return new object[] { new[] { new Dictionary<decimal, int> { [8m] = 4, [10m] = 4 },
                                                new Dictionary<decimal, int> { [100m] = 2, [10m] = 10 } }, 22 };
            yield return new object[] { new[] { new Dictionary<decimal, int> { [8m] = 4, [10m] = 4 },
                                                new Dictionary<decimal, int> { [100m] = 2, [10m] = 10 },
                                                new Dictionary<decimal, int> { [0.01m] = 12 } }, 35 };
}

        [Theory]
        [MemberData(nameof(GetOrderValuesAndAdequateInventories))]
        public void LocationShouldAcceptOrdersWithinInventory(IDictionary<decimal, int>[] orderPizzas, int inventory)
        {
            var location = new Location(inventory);
            var user = CreateUser(location);
            List<IOrder> orders = orderPizzas.Select(x => (IOrder)new Order(x, location, user)).ToList();
            ICollection<IOrder> rejected = location.Order(user, orders);
            Assert.Empty(rejected);
        }

        public static IEnumerable<object[]> GetOrderValuesAndInadequateInventories()
        {
            yield return new object[] { new[] { new Dictionary<decimal, int> { [10m] = 6 } }, 0 };
            yield return new object[] { new[] { new Dictionary<decimal, int> { [8m] = 4, [10m] = 4 },
                                                new Dictionary<decimal, int> { [100m] = 2, [10m] = 10 } }, 10 };
        }

        [Theory]
        [MemberData(nameof(GetOrderValuesAndInadequateInventories))]
        public void LocationShouldRejectOrdersBeyondInventory(IDictionary<decimal, int>[] orderPizzas, int inventory)
        {
            var location = new Location(inventory);
            var user = CreateUser(location);
            List<IOrder> orders = orderPizzas.Select(x => (IOrder)new Order(x, location, user)).ToList();
            ICollection<IOrder> rejected = location.Order(user, orders);
            Assert.NotEmpty(rejected);
        }

        [Fact]
        public void LocationShouldNotAllowUserToPlaceOrdersTwiceInTwoHours()
        {
            var location = new Location(100);
            var user = CreateUser(location);
            var orders = new List<IOrder>
            {
                location.SuggestOrder(user),
                location.SuggestOrder(user),
                location.SuggestOrder(user)
            };
            var almostTwoHoursAgo = new PastTimeProvider(new TimeSpan(hours: 1, minutes: 50, seconds: 0));
            ICollection<IOrder> rejected = null;
            try
            {
                // order 110 minutes in the past
                TimeProvider.Current = almostTwoHoursAgo;
                rejected = location.Order(user, orders);
            }
            finally
            {
                // reset time
                TimeProvider.ResetToDefault();
            }
            // first order accepted
            Assert.Empty(rejected);
            // place more orders again in the present
            var orders2 = new List<IOrder>
            {
                location.SuggestOrder(user),
                location.SuggestOrder(user),
                location.SuggestOrder(user)
            };
            rejected = location.Order(user, orders2);
            // all orders should be rejected
            Assert.Equal(orders2, rejected);
        }

        [Fact]
        public void LocationShouldAllowUserToPlaceMoreOrdersAfterTwoHours()
        {
            var location = new Location(100);
            var user = CreateUser(location);
            var orders = new List<IOrder>
            {
                location.SuggestOrder(user),
                location.SuggestOrder(user),
                location.SuggestOrder(user)
            };
            var overTwoHoursAgo = new PastTimeProvider(new TimeSpan(hours: 2, minutes: 10, seconds: 0));
            ICollection<IOrder> rejected = null;
            try
            {
                // order 130 minutes in the past
                TimeProvider.Current = overTwoHoursAgo;
                rejected = location.Order(user, orders);
            }
            finally
            {
                // reset time
                TimeProvider.ResetToDefault();
            }
            // first order accepted
            Assert.Empty(rejected);
            // place more orders again in the present
            var orders2 = new List<IOrder>
            {
                location.SuggestOrder(user),
                location.SuggestOrder(user),
                location.SuggestOrder(user)
            };
            rejected = location.Order(user, orders2);
            // all orders should be accepted
            Assert.Empty(rejected);
        }

        [Fact]
        public void LocationShouldAllowDifferentUsersToOrderInSuccession()
        {
            var location = new Location(100);
            var user1 = CreateUser(location);
            var user2 = CreateUser(location);
            var orders = new List<IOrder>
            {
                location.SuggestOrder(user1),
                location.SuggestOrder(user1),
                location.SuggestOrder(user1)
            };
            ICollection<IOrder> rejected = location.Order(user1, orders);
            Assert.Empty(rejected);
            var orders2 = new List<IOrder>
            {
                location.SuggestOrder(user2),
                location.SuggestOrder(user2),
                location.SuggestOrder(user2)
            };
            rejected = location.Order(user2, orders2);
            Assert.Empty(rejected);
        }

        public IUser CreateUser(Location defaultLocation)
        {
            return new User("Fred", "Belotte", defaultLocation);
        }

        public class PastTimeProvider : TimeProvider
        {
            public TimeSpan Interval { get; }

            public override DateTime UtcNow => DateTime.UtcNow - Interval;

            public PastTimeProvider(TimeSpan interval) => Interval = interval;
        }
    }
}
