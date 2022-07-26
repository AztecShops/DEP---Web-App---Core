using AztecAppleDepApp.Models;
using Dep;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using AztecAppleDepApp.Helpers;

namespace AztecAppleDepApp.Controllers
{
    public class OverrideController : Controller
    {
        // GET: Override
        public ActionResult Index()
        {
            return View();
        }

        //=============================
        //POST: Search
        // - This allows you to verify if an order exist or not.
        // - The order has to exist via enrolled beforehand before the user can update the serial numbers.
        // - If the order doesn't exist, it will throw an error.
        //=============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Microsoft.AspNetCore.Mvc.ActionResult Search([Bind(Prefix = "TransactionNumber")]string OrderNumber)
        {
            OrderEf Order;
            string TransNo = string.Empty;
            var ReturnTranactionLookUp = TransactionLookUp(OrderNumber.Trim());
            TransNo = ReturnTranactionLookUp.TransactionNo;

            using (_dbContext _Db = new _dbContext())
            {
                Order = _Db.OrdersEf.FirstOrDefault(x => x.OrderNumber == "OR" + TransNo);
            }

            // If order is not null, return information from POS and create a BED Transaction with the previous information.
            if (Order != null)
            {
                var Response = BedTransaction(ReturnTranactionLookUp);
                return PartialView("Result", Response);
            }
            // If null, throw an error.
            else
            {
                ViewBag.Error = true;
                return PartialView("Result");
            }
        }

        //=============================
        //POST: Process
        //=============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Microsoft.AspNetCore.Mvc.ActionResult Process(TransactionLookupViewModel Transaction)
        {
            string OrderNumber = Transaction.TransactionNo.Trim();
            int ChangeCounter = 0;
            var Response = TransactionLookUp(OrderNumber);
            List<string> OldSerials = new List<string>();
            List<string> NewSerials = new List<string>();

            //Process Update to POS Transaction.
            for (int i = 0; i < Transaction.SerialNos.Count; i++)
            {
                if (Transaction.SerialNos[i].Substring(0, 1) == "S")
                {
                    Transaction.SerialNos[i] = Transaction.SerialNos[i].Substring(1);
                }

                // Compare Current vs Original
                // - If it doesn't match, it means that one of the serial number is getting updated.
                if (Transaction.SerialNos[i] != Response.SerialNos[i])
                {
                    NewSerials.Add(Transaction.SerialNos[i].Trim());
                    OldSerials.Add(Response.SerialNos[i].Trim());

                    //Return the Update Success Status
                    bool UpdateSuccess = UpdateSerialPOS(OrderNumber, Response.SerialNos[i], Transaction.SerialNos[i]);

                    // If it's not updated successfully, throw error.
                    if (UpdateSuccess != true)
                    {
                        return View("Error");
                    }

                    ChangeCounter++;
                }
            }

            //if (ChangeCounter == 0)
            //{
            //    ViewBag.Error = true;
            //    return View("Error");
            //}

            for (int i = 0; i < Transaction.SerialNos.Count; i++)
            {
                Transaction.devices.Add(new DeviceEf()
                {
                    DeviceId = Transaction.SerialNos[i].Trim(),
                    AssetTag = Transaction.assetTags[i].Trim(),
                    IsSelected = true
                });
            }

            BedResponse result = null;
            DateTime Stamp = DateTime.Now;
            Transaction.transactionId = Transaction.transactionId.Trim() + "-OR";

            //Process Order and Return Result
            result = ProcessOR(Transaction);

           // If enrollment devices response is not null, check to make sure the 'Status Code' is 'SUCCESS' and save the transaction into DB.
            if (result.enrollDevicesResponse != null)
            {
                if (result.enrollDevicesResponse.statusCode.ToUpper() == "SUCCESS")
                {
                    using (_dbContext _Db = new _dbContext())
                    {
                        // If there are new serial numbers, update and save to DB
                        if (NewSerials.Count != 0)
                        {
                            var ReturnDelivery = _Db.DeliveriesEf.Where(x => x.DeviceEnrollmentTransactionId == result.deviceEnrollmentTransactionId).FirstOrDefault();

                            for (int i = 0; i < NewSerials.Count; i++)
                            {
                                string OldSerialNo = OldSerials[i].Trim();
                                string NewSerialNo = NewSerials[i].Trim();
                                string AssetTagNo = Transaction.assetTags[i].Trim();

                                var ReturnExistingDevice = _Db.DevicesEf.Where(x => x.DeviceId == OldSerialNo);

                                foreach (var item in ReturnExistingDevice)
                                {
                                    item.DeviceId = NewSerialNo;
                                    item.AssetTag = AssetTagNo;
                                    item.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;
                                    item.Stamp = Stamp;
                                    _Db.Entry(item).State = EntityState.Modified;
                                }
                                _Db.SaveChanges();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < Transaction.assetTags.Count; i++)
                            {
                                string DeviceNo = Transaction.SerialNos[i].Trim();
                                string AssetTagNo = Transaction.assetTags[i].Trim();

                                var ReturnExistingDevice = _Db.DevicesEf.Where(x => x.DeviceId == DeviceNo);

                                foreach (var item in ReturnExistingDevice)
                                {
                                    item.AssetTag = AssetTagNo;
                                    item.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;
                                    item.Stamp = Stamp;
                                    _Db.Entry(item).State = EntityState.Modified;
                                }
                                _Db.SaveChanges();
                            }
                        }
                    }
                    //Record Transaction History
                    RecordTransactionHistory(result, Transaction);
                    //Save Transaction
                    SaveTransaction(result, Transaction);
                }
            }

            return View("Process", result);
        }


        //==================================
        // Record Transaction History
        //==================================
        public void RecordTransactionHistory(BedResponse result, TransactionLookupViewModel TransactionLookUpVM)
        {
            DateTime Stamp = DateTime.Now;

            string POSTransaction = TransactionLookUpVM.orderNumber.Substring(2);
            using (_dbContext _db = new _dbContext())
            {
                foreach (var item in TransactionLookUpVM.devices)
                {
                    TransactionHistory Transaction = new TransactionHistory
                    {
                        UserName = User.Identity.Name.ToString(),
                        TimeStamp = Stamp,
                        Device = item.DeviceId,
                        Details = "ORDER",
                        DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId,
                        TransactionId = TransactionLookUpVM.transactionId,
                        POSTransaction = POSTransaction
                    };
                    _db.TransactionHistorys.Add(Transaction);
                }
                _db.SaveChanges();
            }
        }

        //==================================
        // Save Transaction
        //==================================
        private bool SaveTransaction(BedResponse result, TransactionLookupViewModel TransactionLookUpVM)
        {
            try
            {
                var bodyEf = new RequestBodyEf();
                bodyEf.Stamp = DateTime.Now;

                var contextEf = new RequestContextEf();
                contextEf.Stamp = DateTime.Now;

                var orderEf = new OrderEf();
                orderEf.Stamp = DateTime.Now;

                var deliveryEf = new DeliveryEf();
                deliveryEf.Stamp = DateTime.Now;

                var devicesEf = new List<DeviceEf>();

                bodyEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;
                bodyEf.DepResellerId = TransactionLookUpVM.depResellerId;
                bodyEf.TransactionId = TransactionLookUpVM.transactionId.Trim();

                contextEf.LangCode = TransactionLookUpVM.langCode;
                contextEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;
                contextEf.ShipTo = TransactionLookUpVM.shipTo;
                contextEf.TimeZone = TransactionLookUpVM.timeZone;

                bodyEf.RequestContext = contextEf;

                deliveryEf.DeliveryNumber = TransactionLookUpVM.deliveryNumber;
                deliveryEf.ShipDate = TransactionLookUpVM.shipDate;
                deliveryEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;

                orderEf.TransactionId = TransactionLookUpVM.transactionId.Trim();
                orderEf.CustomerDepId = TransactionLookUpVM.customerDepId;
                orderEf.Deliveries.Add(deliveryEf);
                orderEf.OrderDate = TransactionLookUpVM.orderDate;
                orderEf.OrderNumber = TransactionLookUpVM.orderNumber;
                orderEf.OrderType = "OV";
                orderEf.PoNumber = TransactionLookUpVM.poNumber;
                orderEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;

                bodyEf.Orders.Add(orderEf);
                bodyEf.RequestContext = contextEf;

                using (var _db = new _dbContext())
                {
                    _db.RequestBodiesEf.Add(bodyEf);
                    _db.SaveChanges();
                }

                // add service record to be processed by Windows Services for "Check Transaction Status"
                AddServiceRecord(result.deviceEnrollmentTransactionId, TransactionLookUpVM.timeZone, TransactionLookUpVM.depResellerId, TransactionLookUpVM.langCode, TransactionLookUpVM.shipTo, "OV", TransactionLookUpVM.orderNumber, TransactionLookUpVM.transactionId.Trim());
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        //=============================
        // UpdateSerialPOS
        // - Update a single serial number.
        // - Takes in the original Transaction Number, Old Serial and New Serial.
        //=============================
        public bool UpdateSerialPOS(string TransactionID, string OldSerialNo, string NewSerialNo)
        {
            try
            {
                string result = DepTransaction.CashFileUpdate(TransactionID, OldSerialNo, NewSerialNo);
                TransactionLookup transaction = JsonConvert.DeserializeObject<TransactionLookup>(result);

                if (transaction.ErrorCode == "-1")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //=============================
        // Add Service Record
        //=============================
        private bool AddServiceRecord(string deviceEnrollmentTransactionId, string timeZone, string depResellerId, string langCode, string shipTo, string orderType, string orderNumber, string transactionId)
        {
            try
            {
                using (var _db = new _dbContext())
                {
                    var depService = new DepService();
                    depService.Stamp = DateTime.Now;
                    depService.IsProcessed = false;
                    depService.IsInProcess = false;
                    depService.ProcessCounter = 0;
                    depService.ProcessStatus = "Ready to be processed";
                    depService.ReadyToProcess = true;
                    depService.Status = true;
                    depService.LangCode = langCode;
                    depService.TimeZone = timeZone;
                    depService.DepResellerId = depResellerId;
                    depService.ShipTo = shipTo;
                    depService.DeviceEnrollmentTransactionId = deviceEnrollmentTransactionId;
                    depService.ObjNum = deviceEnrollmentTransactionId;
                    depService.OrderNumber = orderNumber;
                    depService.TransactionId = transactionId;
                    depService.OrderType = orderType;
                    _db.DepServices.Add(depService);
                    _db.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        //==================================
        // ProcessOV
        // - Process Override Transaction Devices
        //==================================
        private BedResponse ProcessOR(TransactionLookupViewModel Transaction)
        {
            // Create a New BED Request.
            var request = new BedRequest();
            request.depResellerId = Transaction.depResellerId;
            request.transactionId = Transaction.transactionId.Trim();

            // Create a New BED Request Context.
            request.requestContext = new BedRequestcontext();
            request.requestContext.langCode = Transaction.langCode;
            request.requestContext.shipTo = Transaction.shipTo;
            request.requestContext.timeZone = Transaction.timeZone;

            // Create a new BED order.
            // - You will need this to send off to Apple.
            var order = new BedOrder();
            order.customerId = Transaction.customerId;
            order.orderDate = Transaction.orderDate;
            order.orderNumber = Transaction.orderNumber;
            order.orderType = "OV";
            order.poNumber = Transaction.poNumber;

            // Create a new BED delivery.
            // - You will need this to send off to Apple.
            var deliver = new BedDelivery();
            deliver.deliveryNumber = Transaction.deliveryNumber;
            deliver.shipDate = Transaction.shipDate;

            // - Add each selected device to the BED device object and BED delivery object.
            foreach (var device in Transaction.devices)
            {
                var dev = new BedDevice
                {
                    assetTag = device.AssetTag.Trim(),
                    deviceId = device.DeviceId.Trim()
                };
                deliver.devices.Add(dev);
            }

            order.deliveries.Add(deliver);
            request.orders.Add(order);

            //  You need to Serialize the result into JSON format to send to Apple
            var json = JsonConvert.SerializeObject(request, Formatting.Indented);
            Debug.WriteLine(json);

            //Post Request to Apple and Return DEP Result
            var depResult = PostDEP(json);

            // De-serialize the response from Apple and return result
            var result = JsonConvert.DeserializeObject<BedResponse>(depResult);

            return result;
        }

        //=============================
        // TransactionLookUp
        // - Call the subroutine and de-serialize the JSON result.
        //=============================
        public TransactionLookup TransactionLookUp(string TransactionNumber)
        {
            string result = DepTransaction.CashFileRead(TransactionNumber);
            TransactionLookup transaction = JsonConvert.DeserializeObject<TransactionLookup>(result);
            return transaction;
        }

        //==================================
        // Bed Transaction
        //==================================
        public TransactionLookupViewModel BedTransaction(TransactionLookup TransactionLookUpPOS)
        {
            var request = new TransactionLookupViewModel();
            var ErpResult = new ErpApiResponse();
            string TransNo = TransactionLookUpPOS.TransactionNo.Trim();
            List<string> AssetTagList = new List<string>();

            ErpResult.DepCustomerName = "SDSU";
            DepCustomer customer;

            //Search for customer
            using (var _db = new _dbContext())
            {
                customer = _db.DepCustomers.FirstOrDefault(x => x.Name.ToUpper() == ErpResult.DepCustomerName.ToUpper());
            }

            // If customer exist, add to request.
            if (customer != null)
            {
                request.customerDepId = customer.DepId;
                request.shipTo = customer.ShipTo; // "0000022176";
                request.AccountManager = customer.AccountManager;
                request.ClientContact = customer.Name;
                request.ClientEmail = customer.Email;
                request.langCode = customer.LangCode;   // "en"
                request.timeZone = customer.TimeZone; // "420";
                request.customerId = customer.CustomerId;
            }

            // Search for Return Order
            //List<OrderEf> ReturnExistOrder;
            using (var _db = new _dbContext())
            {
                var ReturnExistOrder = _db.OrdersEf.Where(x => x.OrderNumber == "OR" + TransNo).OrderByDescending(x => x.Stamp).FirstOrDefault();

                //====================================
                // Test Data - Start
                //====================================
                request.orderDate = ReturnExistOrder.OrderDate;
                request.shipDate = ReturnExistOrder.OrderDate;

                foreach (var deliveries in ReturnExistOrder.Deliveries)
                {
                    foreach (var devices in deliveries.Devices)
                    {
                        AssetTagList.Add(devices.AssetTag);
                    }
                    break;
                }

                //====================================
                // Test Data - End
                //====================================

                //foreach (var item in ReturnExistOrder)
                //{
                //    request.orderDate = item.OrderDate;
                //    request.shipDate = item.OrderDate;
                //}

                //foreach (var item in ReturnExistOrder)
                //{
                //    foreach (var deliveries in item.Deliveries)
                //    {
                //        foreach (var devices in deliveries.Devices)
                //        {
                //            AssetTagList.Add(devices.AssetTag);
                //        }
                //        break;
                //    }
                //    break;
                //}
            }

            // DEP Reseller ID
            request.depResellerId = "8B30C50";
            request.poNumber = "PO" + TransNo;
            request.transactionId = "TX" + TransNo;
            request.orderNumber = "OR" + TransNo;
            request.deliveryNumber = "DL" + TransNo;
            request.orderType = "OV";
            request.SerialNos = TransactionLookUpPOS.SerialNos;
            request.assetTags = AssetTagList;
            request.TransactionNo = TransactionLookUpPOS.TransactionNo;

            return request;
        }

        //==================================
        // POST DEP
        // - This method is used when you're sending data to APPLE.
        //==================================
        private string PostDEP(string jsonData)
        {
            var sslPath = MyServer.MapPath("~/") + Properties.Settings.Default.CertPath + Properties.Settings.Default.CertName;
            try
            {
                X509Certificate2Collection certificates = new X509Certificate2Collection();
                var myCert = new X509Certificate2(sslPath, @"Hadaf00x!@");
                certificates.Add(myCert);
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, error) => { return true; };
                var uri = new Uri(Properties.Settings.Default.ProdBulkURL);
                //var uri = new Uri("https://api-applecareconnect.apple.com/enroll-service/1.0/bulk-enroll-devices");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ClientCertificates = certificates;
                request.ContentType = "application/json";
                request.Method = "POST";
                request.ContentLength = jsonData.Length;

                var classLog = JsonConvert.DeserializeObject<BedRequest>(jsonData);

                // Save JSON Request structure to JSON Logs
                using (var _db = new _dbContext())
                {
                    var log = new JsonLog();
                    log.DataDirection = "request";
                    log.TransactionType = "BED";
                    log.OrderType = classLog.orders[0].orderType;
                    log.JsonData = jsonData;
                    log.TransRef = classLog.transactionId;
                    log.Stamp = DateTime.Now;
                    log.Status = true;
                    _db.JsonLogs.Add(log);
                    _db.SaveChanges();
                }

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                    streamWriter.Flush();
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();

                    // Save JSON response structure to JSON Logs
                    using (var _db = new _dbContext())
                    {
                        var log = new JsonLog();
                        log.DataDirection = "response";
                        log.TransactionType = "BED";
                        log.OrderType = classLog.orders[0].orderType;
                        log.JsonData = responseText;
                        log.Stamp = DateTime.Now;
                        log.TransRef = classLog.transactionId;
                        log.Status = true;
                        _db.JsonLogs.Add(log);
                        _db.SaveChanges();
                    }

                    return responseText;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return ex.Message;
            }
        }
    }
}