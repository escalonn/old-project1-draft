using PizzaStore.Library.Models;
using System;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace PizzaStore.Tests
{
    public abstract class AFakeDBTest : IDisposable
    {
        public AFakeDBTest() => PSDBContextProvider.Current = FakePSDBContextProvider.Instance;
        //public AFakeDBTest() => Data.Models.PizzaStoreDBContext.ConfigureConnection = options =>
        //        options.UseSqlServer({connection_string});

        public void Dispose() => PSDBContextProvider.ResetToDefault();
    }
}
