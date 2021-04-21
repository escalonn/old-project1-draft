using System;
using System.ComponentModel.DataAnnotations;
using Lib = PizzaStore.Library.Models;
using LibI = PizzaStore.Library.Interfaces;

namespace PizzaStore.Interface.Models
{
    public class User
    {
        public User() { }

        public User(LibI.IUser lib, Location location)
        {
            Lib = lib;
            if (location.Lib.ID != Lib.DefaultLocation.ID)
            {
                throw new InvalidOperationException("location inconsistent between model and library.");
            }
            DefaultLocation = location;
            DefaultLocationID = location.ID;
            AccountID = lib.AccountID;
            FirstName = lib.FirstName;
            LastName = lib.LastName;
            DisplayName = lib.DisplayName;
        }

        [Display(Name = "Account ID")]
        public int AccountID { get; protected set; }
        
        public Location DefaultLocation { get; set; }

        [Required]
        [Display(Name = "Default Location ID")]
        public int DefaultLocationID { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Name")]
        public string DisplayName { get; protected set; }

        public LibI.IUser Lib { get; protected set; }

        public void Commit()
        {
            if (Lib != null)
            {
                throw new InvalidOperationException("must not commit a model already connected to the library.");
            }
            Lib = new Lib.User(FirstName, LastName, DefaultLocation.Lib);
            DisplayName = Lib.DisplayName;
            AccountID = Lib.AccountID;
        }
    }
}
