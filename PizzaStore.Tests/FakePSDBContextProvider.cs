using Microsoft.EntityFrameworkCore.ChangeTracking;
using PizzaStore.Data.Interfaces;
using PizzaStore.Data.Models;
using PizzaStore.Library.Models;
using System;

namespace PizzaStore.Tests
{
    public class FakePSDBContextProvider : PSDBContextProvider
    {
        private static PSDBContextProvider s_instance;

        private FakePSDBContextProvider() { }

        private int _nextUnusedUserID = 1;

        public static PSDBContextProvider Instance => s_instance ?? (s_instance = new FakePSDBContextProvider());

        // keep track of assigned account ids to mimic database identity constraint behavior
        protected int NextUnusedUserID => _nextUnusedUserID++;

        public override IPizzaStoreDBContext NewPSDBContext => new FakePSDBContext(() => NextUnusedUserID);

        public class FakePSDBContext : IPizzaStoreDBContext
        {
            public Func<int> GetUserID;

            public FakePSDBContext(Func<int> getUserID) =>
                GetUserID = getUserID ?? throw new ArgumentNullException(nameof(getUserID));

            public void Dispose() { }

            public int SaveChanges() => 0;

            public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
            {
                // provide user dao with an account id
                // (can't just do a overload here, have to check type at run-time)
                if (entity is Psuser psuser)
                {
                    psuser.UserId = GetUserID();
                }
                return null;
            }
        }
    }
}