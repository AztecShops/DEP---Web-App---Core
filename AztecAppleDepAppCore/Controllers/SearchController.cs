using AztecAppleDepApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace AztecAppleDepApp.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchSerial(string Id)
        {
            var result = new List<SearchResultViewModel>();

            using (var _db = new _dbContext())
            {
                var recs = _db.DevicesEf.Where(x => x.DeviceId == Id.Trim()).OrderByDescending(x => x.Stamp).ToList();
                foreach (var rec in recs)
                {
                    result.Add(new SearchResultViewModel()
                    {
                        DepTransactionDate = rec.Stamp.ToString(),
                        DepTransactionId = rec.DeviceEnrollmentTransactionId,
                        DepTransactionType = rec.DeliveryEf.OrderEf.OrderType,
                        RecId = rec.DeliveryEf.OrderEf.ID.ToString(),
                        DepTransactoinNote = rec.DeliveryEf.OrderEf.Note
                    });
                }
            }
            return View("Search",result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchOrder(string Id)
        {
            var result = new List<SearchResultViewModel>();

            using (var _db = new _dbContext())
            {
                var recs = _db.OrdersEf.Where(x => x.OrderNumber == "OR" + Id).OrderByDescending(x => x.Stamp).ToList();
                foreach (var rec in recs)
                {
                    result.Add(new SearchResultViewModel()
                    {
                        DepTransactionDate = rec.Stamp.ToString(),
                        DepTransactionId = rec.DeviceEnrollmentTransactionId,
                        DepTransactionType = rec.OrderType,
                        RecId = rec.ID.ToString(),
                        DepTransactoinNote = rec.Note
                    });
                }
            }
            return View("Search",result);
        }

        [HttpGet]
        public ActionResult Details(string Id)
        {
            var result = new DepTransactionDetailViewModel();
            var recId = Convert.ToInt32(Id);
            using (var _db = new _dbContext())
            {
                var order = _db.OrdersEf.Find(recId);
                result.OrderEf = order;
                if (order != null)
                {
                    var body = _db.RequestBodiesEf.FirstOrDefault(x => x.ID == order.RequestBodyEf.ID);
                    result.BodyEf = body;
                    var delivery = _db.DeliveriesEf.FirstOrDefault(x => x.OrderEf.ID == order.ID);
                    result.DeliveryEf = delivery;
                    var devices = _db.DevicesEf.Where(x => x.DeliveryEf.ID == delivery.ID).ToList();
                    foreach (var device in devices)
                    {
                        result.DeviceEfs.Add(device);
                    }
                }
            }
            return View("Details", result);
        }
    }
}