using Microsoft.AspNetCore.Mvc;
using PizzaStore.Interface.Models;
using System;
using System.Linq;
using Lib = PizzaStore.Library.Models;

namespace PizzaStore.Interface.Controllers
{
    public class LocationController : Controller
    {
        private static Lib.LibHelper s_libHelper = Lib.LibHelper.Instance;

        // GET: Location
        public ActionResult Index()
        {
            return View(s_libHelper.Locations.Select(l => new Location(l)));
        }

        // GET: Location/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Orders = s_libHelper.Orders.Where(o => o.Location.ID == id && o.Time != null).Select(o =>
            {
                Location l = new Location(o.Location);
                return new Order(o, l, new User(o.User, new Location(o.User.DefaultLocation)));
            });
            return View(new Location(s_libHelper.Locations.First(l => l.ID == id)));
        }

        // GET: Location/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Location/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Location location)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("invalid location.");
                }
                location.Commit();
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