using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PizzaStore.Interface.Models;
using System.Collections.Generic;
using System.Linq;
using Lib = PizzaStore.Library.Models;
using LibI = PizzaStore.Library.Interfaces;

namespace PizzaStore.Interface.Controllers
{
    public class OrderController : Controller
    {
        private static Lib.LibHelper s_libHelper = Lib.LibHelper.Instance;

        // GET: Order
        public ActionResult Index(int id)
        {
            object userId = TempData["UserID"];
            if (userId is null || (int) userId != id)
            {
                TempData["OrderIDs"] = null;
            }
            TempData["UserID"] = id;
            var orderIDs = new List<int>(TempData["OrderIDs"] as int[] ?? new int[] { });
            TempData["OrderIDs"] = orderIDs.ToArray<int>();
            return View(s_libHelper.Orders.Where(o => orderIDs.Contains(o.ID)).Select(o =>
            {
                Location l = new Location(o.Location);
                return new Order(o, l, new User(o.User, new Location(o.User.DefaultLocation)));
            }));
        }

        // GET: Order/Details/5
        public ActionResult Details(int id)
        {
            LibI.IOrder lO = s_libHelper.Orders.First(o => o.ID == id);
            Location l = new Location(lO.Location);
            Order order = new Order(lO, l, new User(lO.User, new Location(lO.User.DefaultLocation)));
            ViewBag.OrderParts = lO.PizzasByPrice;
            return View(order);
        }

        // GET: Order/Create
        public ActionResult Create()
        {
            ViewBag.Locations = s_libHelper.Locations.Select(l => l.ID);
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            int userID = (int)TempData["UserID"];
            int locationID = int.Parse(collection["LocationID"]);
            Location location = new Location(s_libHelper.Locations.First(l => l.ID == locationID));
            LibI.IUser lU = s_libHelper.Users.First(u => u.AccountID == userID);
            User user = new User(lU, new Location(lU.DefaultLocation));
            Order order = new Order(location.Lib.SuggestOrder(lU), location, user);
            s_libHelper.Reload();
            var orderIDs = new List<int>(TempData["OrderIDs"] as int[] ?? new int[] { }) { order.ID };
            TempData["OrderIDs"] = orderIDs.ToArray<int>();

            return RedirectToAction(nameof(Index), new { id = userID });

        }

        // GET: Order/Delete/5
        public ActionResult Delete(int id)
        {
            var orderIDs = new List<int>(TempData["OrderIDs"] as int[] ?? new int[] { });
            orderIDs.Remove(id);
            TempData["OrderIDs"] = orderIDs.ToArray<int>();
            int userID = (int)TempData["UserID"];
            return RedirectToAction(nameof(Index), new { id = userID });
        }

        // GET: Order/Call
        public ActionResult Call()
        {
            var orderIDs = new List<int>(TempData["OrderIDs"] as int[] ?? new int[] { });
            try
            {
                ICollection<LibI.IOrder> lOrders = orderIDs.Select(i => s_libHelper.Orders.First(o => o.ID == i)).ToList();
                LibI.IOrder first = lOrders.First();
                ICollection<LibI.IOrder> rejected = first.Location.Order(first.User, lOrders);
                orderIDs = lOrders.Select(o => o.ID).Where(i => rejected.Any(ro => ro.ID == i)).ToList();
            }
            catch
            {
                // some malformed order calls just throw exception
                return RedirectToAction("Failure");
            }
            finally
            {
                TempData["OrderIDs"] = orderIDs.ToArray<int>();
            }
            s_libHelper.Reload();
            int userID = (int)TempData["UserID"];
            if (orderIDs.Count == 0)
            {
                return RedirectToAction("Success");
            }
            return RedirectToAction(nameof(Index), new { id = userID });
        }

        public ActionResult Success()
        {
            return View();
        }

        public ActionResult Failure()
        {
            return View();
        }
    }
}