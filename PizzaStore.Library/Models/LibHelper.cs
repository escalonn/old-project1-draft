using Microsoft.EntityFrameworkCore;
using PizzaStore.Data.Models;
using PizzaStore.Library.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PizzaStore.Library.Models
{
    public class LibHelper
    {
        private static LibHelper s_instance;

        private LibHelper() => Reload();

        public void Reload()
        {
            using (PizzaStoreDBContext context = (PizzaStoreDBContext)PSDBContextProvider.Current.NewPSDBContext)
            {
                // probably a lot of wasted time here
                Locations = context.Pslocation.Select(l => new Location(l)).ToList();
                Users = context.Psuser.Include(u => u.DefaultLocation).ToList()
                    .Select(u => new User(u, Locations.First(l => l.Dao.LocationId == u.DefaultLocationId))).ToList();
                Orders = context.Psorder.Include(o => o.Location).Include(o => o.User).Include(o => o.PsorderPart).ToList()
                    .Select(o => new Order(o, Locations.First(l => l.Dao.LocationId == o.Location.LocationId),
                                           Users.First(u => u.Dao.UserId == o.User.UserId))).ToList();
            }
        }

        public static LibHelper Instance => s_instance ?? (s_instance = new LibHelper());

        public IReadOnlyCollection<ILocation> Locations { get; private set; }

        public IReadOnlyCollection<IUser> Users { get; private set; }

        public IReadOnlyCollection<IOrder> Orders { get; private set; }
    }
}
