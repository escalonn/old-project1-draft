using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using Lib = PizzaStore.Library.Models;
using LibI = PizzaStore.Library.Interfaces;

namespace PizzaStore.Interface.Models
{
    public class Location
    {
        private int _inventory;

        public Location() { }

        public Location(LibI.ILocation lib)
        {
            Lib = lib;
            Inventory = lib.PieCount;
            ID = lib.ID;
        }

        [Display(Name = "ID")]
        public int ID { get; protected set; }
        
        [Required]
        [Display(Name = "Inventory")]
        public int Inventory
        {
            get => _inventory;
            set
            {
                _inventory = value;
                if (Lib != null)
                {
                    Lib.PieCount = value;
                }
            }
        }

        public LibI.ILocation Lib { get; protected set; }

        public Order SuggestOrder(User user)
        {
            return new Order(Lib.SuggestOrder(user.Lib), this, user);
        }

        public void Commit()
        {
            if (Lib != null)
            {
                throw new InvalidOperationException("must not commit a model already connected to the library.");
            }
            Lib = new Lib.Location(Inventory);
            ID = Lib.ID;
        }
    }
}