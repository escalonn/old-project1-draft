using System;
using System.Collections.Generic;
using Lib = PizzaStore.Library.Models;
using LibI = PizzaStore.Library.Interfaces;

namespace PizzaStore.Interface.Models
{
    public class Order
    {
        private DateTime? _time;

        public Order() { }

        public Order(LibI.IOrder lib, Location location, User user)
        {
            Lib = lib;
            if (location.Lib.ID != Lib.Location.ID)
            {
                throw new InvalidOperationException("location inconsistent between model and library.");
            }
            Location = location;
            if (user.Lib.AccountID != Lib.User.AccountID)
            {
                throw new InvalidOperationException("user inconsistent between model and library.");
            }
            User = user;
            _time = lib.Time;
            PizzasByPrice = lib.PizzasByPrice;
            ID = lib.ID;
        }

        public int ID { get; protected set; }

        public Location Location { get; set; }

        public User User { get; set; }

        public DateTime? Time
        {
            get => _time;
            set
            {
                _time = value;
                Lib.Time = value;
            }
        }

        public IDictionary<decimal, int> PizzasByPrice { get; set; }

        public LibI.IOrder Lib { get; protected set; }

        public void Commit()
        {
            if (Lib != null)
            {
                throw new InvalidOperationException("must not commit a model already connected to the library.");
            }
            Lib = new Lib.Order(PizzasByPrice, Location.Lib, User.Lib);
            Time = Lib.Time;
        }
    }
}
