using PizzaStore.Data.Interfaces;
using PizzaStore.Data.Models;

namespace PizzaStore.Library.Models
{
    public class DefaultPSDBContextProvider : PSDBContextProvider
    {
        private static PSDBContextProvider s_instance;

        private DefaultPSDBContextProvider() { }

        public static PSDBContextProvider Instance => s_instance ?? (s_instance = new DefaultPSDBContextProvider());

        public override IPizzaStoreDBContext NewPSDBContext => new PizzaStoreDBContext();
    }
}