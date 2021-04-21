using Microsoft.AspNetCore.Mvc;
using PizzaStore.Interface.Models;
using System;
using System.Linq;
using ILib = PizzaStore.Library.Interfaces;
using Lib = PizzaStore.Library.Models;

namespace PizzaStore.Interface.Controllers
{
    public class UserController : Controller
    {
        private static Lib.LibHelper s_libHelper = Lib.LibHelper.Instance;

        // GET: User
        public ActionResult Index()
        {
            return View(s_libHelper.Users.Select(u => new User(u, new Location(u.DefaultLocation))));
        }

        // GET: User/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Orders = s_libHelper.Orders.Where(o => o.User.AccountID == id && o.Time != null).Select(o =>
            {
                Location l = new Location(o.Location);
                return new Order(o, l, new User(o.User, new Location(o.User.DefaultLocation)));
            });
            ILib.IUser lU = s_libHelper.Users.First(u => u.AccountID == id);
            return View(new User(lU, new Location(lU.DefaultLocation)));
        }

        // GET: User/Create
        public ActionResult Create()
        {
            ViewBag.Locations = s_libHelper.Locations.Select(l => l.ID);
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("invalid user.");
                }
                user.DefaultLocation = new Location(s_libHelper.Locations.First(l => l.ID == user.DefaultLocationID));
                user.Commit();
                s_libHelper.Reload();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Create));
            }
        }
    }
}