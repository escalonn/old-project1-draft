using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace PizzaStore.Data.Interfaces
{
    public interface IPizzaStoreDBContext : IDisposable
    {
        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;
        int SaveChanges();
    }
}