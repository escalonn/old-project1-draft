using PizzaStore.Data.Interfaces;
using System;

namespace PizzaStore.Library.Models
{
    public abstract class PSDBContextProvider
    {
        private static PSDBContextProvider s_current = DefaultPSDBContextProvider.Instance;

        public static PSDBContextProvider Current
        {
            get => s_current;
            set => s_current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public abstract IPizzaStoreDBContext NewPSDBContext { get; }

        public void UpdateAndSave<TEntity>(TEntity dao) where TEntity : class
        {
            using (IPizzaStoreDBContext context = NewPSDBContext)
            {
                context.Update(dao);
                context.SaveChanges();
            }
        }

        public static void ResetToDefault() => s_current = DefaultPSDBContextProvider.Instance;
    }
}
