using PizzaStore.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PizzaStore.Tests
{
    public class OrderTest : AFakeDBTest
    {
        public static IEnumerable<object[]> GetLessThanOnePizzaOrderValues()
        {
            yield return new[] { new Dictionary<decimal, int> { [1m] = 0, [2m] = 0 } };
            yield return new[] { new Dictionary<decimal, int> { [10m] = -1 } };
            yield return new[] { new Dictionary<decimal, int> { [1m] = 2, [3m] = -2 } };
        }

        [Theory]
        [MemberData(nameof(GetLessThanOnePizzaOrderValues))]
        public void OrdersWithLessThanOnePizzaShouldBeInvalid(IDictionary<decimal, int> pizzasByPrice)
        {
            Assert.ThrowsAny<ArgumentException>(() => CreateOrder(pizzasByPrice));
        }

        public static IEnumerable<object[]> GetNegativePricePizzaOrderValues()
        {
            yield return new[] { new Dictionary<decimal, int> { [-1m] = 1, [1m] = 2 } };
            yield return new[] { new Dictionary<decimal, int> { [-1m] = 1, [-10m] = 2 } };
        }

        [Theory]
        [MemberData(nameof(GetNegativePricePizzaOrderValues))]
        public void OrdersWithNegativePricePizzaShouldBeInvalid(IDictionary<decimal, int> pizzasByPrice)
        {
            Assert.ThrowsAny<ArgumentException>(() => CreateOrder(pizzasByPrice));
        }

        public static IEnumerable<object[]> GetNegativeCountPizzaOrderValues()
        {
            yield return new[] { new Dictionary<decimal, int> { [1m] = -1, [5m] = 3 } };
            yield return new[] { new Dictionary<decimal, int> { [10m] = -2 } };
        }

        [Theory]
        [MemberData(nameof(GetNegativeCountPizzaOrderValues))]
        public void OrdersWithNegativeCountPizzaShouldBeInvalid(IDictionary<decimal, int> pizzasByPrice)
        {
            Assert.ThrowsAny<ArgumentException>(() => CreateOrder(pizzasByPrice));
        }

        [Fact]
        public void OrderMaxTotalValueShouldBe500()
        {
            Order order = CreateOrder((IDictionary<decimal, int>)GetValidOrderValues().First()[0]);
            Assert.Equal(expected: 500, actual: order.MaxTotalValueUsd);
        }

        public static IEnumerable<object[]> GetExpensiveOrderValues()
        {
            yield return new[] { new Dictionary<decimal, int> { [500.01m] = 1 } };
            yield return new[] { new Dictionary<decimal, int> { [150m] = 1, [100m] = 4 } };
        }

        [Theory]
        [MemberData(nameof(GetExpensiveOrderValues))]
        public void OrdersOver500DollarsShouldBeInvalid(IDictionary<decimal, int> pizzasByPrice)
        {
            Assert.ThrowsAny<ArgumentException>(() => CreateOrder(pizzasByPrice));
        }

        [Fact]
        public void MaxPizzasPerOrderShouldBe12()
        {
            Order order = CreateOrder((IDictionary<decimal, int>)GetValidOrderValues().First()[0]);
            Assert.Equal(expected: 12, actual: order.MaxPizzaCount);
        }

        public static IEnumerable<object[]> GetLargeOrderValues()
        {
            yield return new[] { new Dictionary<decimal, int> { [10m] = 13 } };
            yield return new[] { new Dictionary<decimal, int> { [10m] = 4, [5m] = 10 } };
        }

        [Theory]
        [MemberData(nameof(GetLargeOrderValues))]
        public void OrdersOver12PizzasShouldBeInvalid(IDictionary<decimal, int> pizzasByPrice)
        {
            Assert.ThrowsAny<ArgumentException>(() => CreateOrder(pizzasByPrice));
        }

        public static IEnumerable<object[]> GetSomeZeroPizzaOrderValues()
        {
            yield return new[] { new Dictionary<decimal, int> { [10m] = 0, [2m] = 5 } };
            yield return new[] { new Dictionary<decimal, int> { [1m] = 4, [5m] = 0, [6m] = 0 } };
        }

        [Theory]
        [MemberData(nameof(GetSomeZeroPizzaOrderValues))]
        public void OrdersWithSomeZeroPizzaCountsShouldBeValidButStripZeroes(IDictionary<decimal, int> pizzasByPrice)
        {
            Order order = null;
            Exception ex = Record.Exception(() => order = CreateOrder(pizzasByPrice));
            Assert.Null(ex);
            foreach (int count in order.PizzasByPrice.Values)
            {
                Assert.NotEqual(expected: 0, actual: count);
            }
        }

        [Fact]
        public void OrdersWithNullLocationShouldBeInvalid()
        {
            Assert.ThrowsAny<ArgumentException>(() => new Order(pizzasByPrice: new Dictionary<decimal, int> { [10m] = 1 },
                                                                location: null,
                                                                user: new User("Fred", "Belotte", new Location(1))));
        }

        [Fact]
        public void OrdersWithNullPizzasShouldBeInvalid()
        {
            Assert.ThrowsAny<ArgumentException>(() => CreateOrder(null));
        }

        public static IEnumerable<object[]> GetValidOrderValues()
        {
            yield return new[] { new Dictionary<decimal, int> { [10m] = 6 } };
            yield return new[] { new Dictionary<decimal, int> { [8m] = 4, [10m] = 4 } };
            yield return new[] { new Dictionary<decimal, int> { [200m] = 2, [10m] = 10 } };
            yield return new[] { new Dictionary<decimal, int> { [500m] = 1 } };
        }

        [Theory]
        [MemberData(nameof(GetValidOrderValues))]
        public void ValidOrdersShouldBeValid(IDictionary<decimal, int> pizzasByPrice)
        {
            Exception ex = Record.Exception(() => CreateOrder(pizzasByPrice));
            Assert.Null(ex);
        }

        public static IEnumerable<object[]> GetOrderValuesAndTotalPrices()
        {
            yield return new object[] { new Dictionary<decimal, int> { [10m] = 6 }, 10m * 6 };
            yield return new object[] { new Dictionary<decimal, int> { [8m] = 4, [10m] = 4 }, 8m * 4 + 10m * 4 };
            yield return new object[] { new Dictionary<decimal, int> { [200m] = 2, [10m] = 10 }, 200m * 2 + 10m * 10 };
            yield return new object[] { new Dictionary<decimal, int> { [500m] = 1 }, 500m * 1 };
        }

        [Theory]
        [MemberData(nameof(GetOrderValuesAndTotalPrices))]
        public void OrdersShouldHaveCorrectTotalPrice(IDictionary<decimal, int> pizzasByPrice, decimal totalValueUsd)
        {
            Assert.Equal(expected: totalValueUsd, actual: CreateOrder(pizzasByPrice).TotalValueUsd);
        }

        [Fact]
        public void OrderShouldStoreLocation()
        {
            var location = new Location(50);
            var user = new User("Fred", "Belotte", location);
            var order = new Order(new Dictionary<decimal, int> { [10m] = 6 }, location, user);
            Assert.Equal(expected: location, actual: order.Location);
        }

        [Fact]
        public void OrderShouldStorePizzaDict()
        {
            var pizzasByPrice = new Dictionary<decimal, int> { [10m] = 6 };
            Order order = CreateOrder(pizzasByPrice);
            Assert.Equal(expected: pizzasByPrice, actual: order.PizzasByPrice);
        }

        [Fact]
        public void OrdersWithDifferentLocationShouldNotBeEqual()
        {
            var pizzas1 = new Dictionary<decimal, int> { [10m] = 6 };
            Order order1 = CreateOrder(pizzas1);
            var pizzas2 = new Dictionary<decimal, int>(pizzas1);
            var order2 = new Order(pizzas2, new Location(1), order1.User);
            Assert.NotEqual(order1, order2);
        }

        [Fact]
        public void OrdersWithDifferentPizzasShouldNotBeEqual()
        {
            var pizzas1 = new Dictionary<decimal, int> { [10m] = 6 };
            Order order1 = CreateOrder(pizzas1);
            var pizzas2 = new Dictionary<decimal, int> { [11m] = 6 };
            var order2 = new Order(pizzas2, order1.Location, order1.User);
            Assert.NotEqual(order1, order2);
        }

        public Order CreateOrder(IDictionary<decimal, int> pizzasByPrice)
        {
            var location = new Location(1);
            var user = new User("Fred", "Belotte", location);
            var order = new Order(pizzasByPrice, location, user);
            return order;
        }
    }
}
