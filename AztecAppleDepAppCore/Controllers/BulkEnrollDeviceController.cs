using AztecAppleDepApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Dep;
using AztecAppleDepApp.Helpers;

namespace AztecAppleDepApp.Controllers
{
    public class BulkEnrollDeviceController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        //==========================
        // Return Subroutine
        // - Returns the data from Karen's subroutine.
        //==========================
        public Transaction ReturnSubroutine(string TransactionNumber)
        {
            string result = DepTransaction.Update(TransactionNumber);
            Transaction transaction = JsonConvert.DeserializeObject<Transaction>(result);
            return transaction;
        }

        //==========================
        // Search - POST
        //==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(string TransactionNumber)
        {
            // - Example Format: 2018-12-16T05:07:28Z
            string OrderStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
           //string OrderStamp = "2017-05-19T12:57:57" + "Z";

            // - Remove leading spaces
            TransactionNumber = TransactionNumber.Trim();

            // - Return Transaction Data
            var ReturnPOSData = ReturnSubroutine(TransactionNumber);
            string TransNo = ReturnPOSData.TransactionNo;
            string ReturnTransNo = ReturnPOSData.ReturnTransactionId;
            string OrderType = ReturnPOSData.TypeOfSearch;

            // - If there's an error from the POS Subroutine, return Transaction Not Found.
            if (ReturnPOSData.ErrorCode != "-1")
            {
                ViewBag.TransactionNotFound = true;
                return PartialView("Result");
            }

            // - A transaction is found and you have serial numbers that is registered to the transactions.
            // - You will need to loop through the serial numbers and verify if they belong to other existing transaction(s).
            int OrderCount = 0;
            foreach (var device in ReturnPOSData.SerialNos)
            {
                List<DeviceEf> DeviceExist;
                List<OrderEf> OrderExist;
                string DeviceId = string.Empty;
                if(device.Substring(0,1) == "S")
                {
                    DeviceId = device.Substring(1);
                }
                else
                {
                    DeviceId = device;
                }
              
                using (var _db = new _dbContext())
                {
                    // Query for exist device
                    DeviceExist = _db.DevicesEf.Where(x => x.DeviceId == DeviceId && x.IsSelected == true).ToList();

                    // If device exist, check to see if a transaction has been enrolled with this device.
                    if (DeviceExist.Count > 0)
                    {
                        // Query for exist order with this device by passing in the TransNo
                        OrderExist = _db.OrdersEf.Where(x => x.OrderNumber == "OR" + TransNo).ToList();

                        // Check to see if this transaction is registered to these device(s).
                        // If there is no record indicating that these devices belongs to this transaction/order, then these device(s) cannot be enrolled. 
                        // It means, they are enrolled with another transaction.
                        // You will need to keep track of the count.
                        if (OrderExist.Count == 0)
                        {
                            OrderCount++;
                        }
                    }
                }
            }

            // If the 'OrderCount' is greater than 1, tell the user that these devices are already enrolled with another transaction.
            if (OrderCount > 0)
            {
                ViewBag.DeviceEnrolled = true;
                return PartialView("Result");
            }

            string OriginalTransaction = TransNo;
            string ReturnTransaction = ReturnTransNo;
            string VoidTransaction = TransNo;
            string SearchOrderType = OrderType;
            string LastFourReturnTransaction = string.Empty;
            string LastFourVoidTransaction = string.Empty;
            List<string> ListOfDevices = ReturnPOSData.SerialNos;
            List<DeviceEf> ExistingEnrollDevices;

            var request = new BedTransactionDisplayViewModel();
            var ErpResult = new ErpApiResponse();

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

            // DEP Reseller ID
            request.depResellerId = "8B30C50";

            // Verify what type of Order.
            // - 'VOID' Order Type
            if (SearchOrderType == "VD")
            {
                //You need to check if you're voiding a 'RETURN' transaction or 'ORDER'(sale) transaction.
                // - If RETURN TRANSACTION is empty then it's a SALE TRANSACTION 
                if (ReturnTransaction != "")
                {
                    TransactionHistory ReturnTransactionData;
                    string TransactionID = string.Empty;

                    //Search for Return Transaction
                    using (var _db = new _dbContext())
                    {
                        ReturnTransactionData = _db.TransactionHistorys.Where(x => x.POSTransaction == ReturnTransaction).FirstOrDefault();
                    }

                    if (ReturnTransactionData != null)
                    {
                        TransactionID = ReturnTransactionData.TransactionId.Substring(2);
                        string[] TransactionIDSplit = TransactionID.Split('-');

                        //The first index will always be the original transaction number.
                        OriginalTransaction = TransactionIDSplit[0];
                    }
                    LastFourVoidTransaction = VoidTransaction.Substring(VoidTransaction.Length - 4);
                    LastFourReturnTransaction = ReturnTransaction.Substring(ReturnTransaction.Length - 4);
                }
                else
                {
                    LastFourVoidTransaction = VoidTransaction.Substring(VoidTransaction.Length - 4);
                }

                // Void - Return
                if (!string.IsNullOrEmpty(ReturnTransaction))
                {
                    request.orderType = SearchOrderType;
                    request.poNumber = "PO" + VoidTransaction;
                    request.transactionId = "TX" + VoidTransaction;
                    request.orderNumber = "OR" + OriginalTransaction + "-" + LastFourReturnTransaction;
                    request.deliveryNumber = "DL" + VoidTransaction;
                    request.returnTrasnactionNumber = ReturnTransaction;
                    request.returnVoidNumber = VoidTransaction;
                    request.originalTransactionNumber = OriginalTransaction;
                }
                // Void - Sale
                else
                {
                    request.orderType = SearchOrderType;
                    request.poNumber = "PO" + VoidTransaction.Trim();
                    request.transactionId = "TX" + VoidTransaction.Trim();
                    request.orderNumber = "OR" + OriginalTransaction;
                    request.deliveryNumber = "DL" + VoidTransaction.Trim();
                    request.returnTrasnactionNumber = ReturnTransaction;
                    request.returnVoidNumber = VoidTransaction;
                    request.originalTransactionNumber = OriginalTransaction;
                }

                ExistingEnrollDevices = CheckForEnrollDevices("OR" + OriginalTransaction);
            }

            // - 'RETURN' Order Type
            else if (SearchOrderType == "RE")
            {
                if (ReturnTransaction != null)
                {
                    if (ReturnTransaction.Length > 4)
                    {
                        LastFourReturnTransaction = ReturnTransaction.Substring(ReturnTransaction.Length - 4);
                    }
                }
                request.orderType = SearchOrderType;
                request.poNumber = "PO" + OriginalTransaction;
                request.transactionId = "TX" + OriginalTransaction + "-" + LastFourReturnTransaction;
                request.orderNumber = "OR" + OriginalTransaction + "-" + LastFourReturnTransaction;
                request.deliveryNumber = "DL" + OriginalTransaction;
                request.originalTransactionNumber = OriginalTransaction;
                request.returnTrasnactionNumber = ReturnTransaction;
                ExistingEnrollDevices = CheckForEnrollDevices("OR" + OriginalTransaction);

            }

            // - 'ORDER' Order Type
            else
            {
                request.orderType = SearchOrderType;
                request.poNumber = "PO" + OriginalTransaction;
                request.transactionId = "TX" + OriginalTransaction;
                request.orderNumber = "OR" + OriginalTransaction;
                request.deliveryNumber = "DL" + OriginalTransaction;
                request.returnTrasnactionNumber = ReturnTransaction;
                request.originalTransactionNumber = OriginalTransaction;
                ExistingEnrollDevices = CheckForEnrollDevices("OR" + OriginalTransaction);
            }

            // Search for Return Order
            List<OrderEf> ReturnExistOrder;
            using (var _db = new _dbContext())
            {
                ReturnExistOrder = _db.OrdersEf.Where(x => x.OrderNumber == "OR" + TransNo).ToList();
            }

            if (ReturnExistOrder.Count != 0)
            {
                foreach (var item in ReturnExistOrder)
                {
                    request.orderDate = item.OrderDate;
                    request.shipDate = item.OrderDate;
                }
            }
            else
            {
                request.orderDate = OrderStamp;
                request.shipDate = OrderStamp;
            }

            // Check for existing enrolled devices
            if (ExistingEnrollDevices.Count() > 0)
            {
                foreach (var item in ExistingEnrollDevices)
                {
                    // 'ORDER'
                    if (SearchOrderType == "OR")
                    {
                        // Check to see if there is a 'S' in front of the serial number. Apple puts an serial number in front of their serial number when scanned.
                        // However, sometime user will type in the serial number, so you will need to check and remove the 'S' from the serial number.
                        if (item.DeviceId[0] == 'S' || item.DeviceId[0] == 's')
                        {
                            request.devices.Add(new DeviceEf()
                            {
                                DeviceId = item.DeviceId.Substring(1),
                                AssetTag = item.AssetTag,
                                IsSelected = item.IsSelected,
                                IsVoid = item.IsVoid,
                                IsReturn = item.IsReturn,
                            });
                        }
                        else
                        {
                            request.devices.Add(new DeviceEf()
                            {
                                DeviceId = item.DeviceId,
                                AssetTag = item.AssetTag,
                                IsSelected = item.IsSelected,
                                IsVoid = item.IsVoid,
                                IsReturn = item.IsReturn,
                            });
                        }

                    }

                    // 'RETURN'
                    else if (SearchOrderType == "RE")
                    {
                        for (int i = 0; i < ListOfDevices.Count; i++)
                        {
                            string device = string.Empty;
                            if(ListOfDevices[i].Substring(0,1) == "S")
                            {
                                device = ListOfDevices[i].Substring(1);
                            }
                            else
                            {
                                device = ListOfDevices[i];
                            }
                            //Match the device(s) that is being return from the POS to DB.
                            if (item.DeviceId == device)
                            {

                                TransactionHistory TransactionData;
                                List<OrderEf> OrderData;

                                //Search for all void transactions for this device.
                                using (var _db = new _dbContext())
                                {
                                    TransactionData = _db.TransactionHistorys.Where(x => x.Details == "RETURN" && x.Device == item.DeviceId && x.POSTransaction == OriginalTransaction).FirstOrDefault();
                                }

                                if (TransactionData != null)
                                {
                                    string TransactionIDSubString = TransactionData.TransactionId.Substring(2);
                                    string[] TransactionIDSplit = TransactionIDSubString.Split('-');
                                    string OrderNumber = "OR" + TransactionIDSplit[0] + "-" + TransactionIDSplit[1];
                                    using (var _db = new _dbContext())
                                    {
                                        OrderData = _db.OrdersEf.Where(x => x.OrderNumber == OrderNumber).ToList();
                                    }

                                    if (OrderData.Count == 2)
                                    {
                                        request.devices.Add(new DeviceEf()
                                        {
                                            DeviceId = item.DeviceId,
                                            AssetTag = item.AssetTag,
                                            IsSelected = item.IsSelected,
                                            IsVoid = false,
                                            IsReturn = false,
                                        });
                                        ViewBag.TransactionWasVoided = true;
                                    }

                                    // Already processed return
                                    else
                                    {
                                        request.devices.Add(new DeviceEf()
                                        {
                                            DeviceId = item.DeviceId,
                                            AssetTag = item.AssetTag,
                                            IsSelected = item.IsSelected,
                                            IsVoid = item.IsVoid,
                                            IsReturn = item.IsReturn, // This can be true or false depending if the transaction was voided or not.
                                        });
                                    }
                                }
                                else
                                {
                                    // Select to Return
                                    if (item.IsSelected == true && item.IsReturn == false)
                                    {
                                        request.devices.Add(new DeviceEf()
                                        {
                                            DeviceId = item.DeviceId,
                                            AssetTag = item.AssetTag,
                                            IsSelected = item.IsSelected,
                                            IsVoid = item.IsVoid,
                                            IsReturn = true,
                                        });
                                    }

                                    // Already processed return
                                    else
                                    {
                                        request.devices.Add(new DeviceEf()
                                        {
                                            DeviceId = item.DeviceId,
                                            AssetTag = item.AssetTag,
                                            IsSelected = item.IsSelected,
                                            IsVoid = item.IsVoid,
                                            IsReturn = item.IsReturn, // This can be true or false depending if the transaction was voided or not.
                                        });
                                    }
                                }
                            }
                        }
                    }

                    // 'VOID'
                    else if (SearchOrderType == "VD")
                    {
                        for (int i = 0; i < ListOfDevices.Count; i++)
                        {
                            string DeviceID = string.Empty;

                            if(ListOfDevices[i][0] == 'S' || ListOfDevices[i][0] == 's')
                            {
                                DeviceID = ListOfDevices[i].Substring(1);
                            }
                            else
                            {
                                DeviceID = ListOfDevices[i];
                            }

                            //Match the device(s) that is being voided from the POS to DB.
                            if (item.DeviceId == DeviceID)
                            {
                                // Select to Void Return Transaction
                                if (item.IsSelected == false && item.IsReturn == true)
                                {
                                    request.devices.Add(new DeviceEf()
                                    {
                                        DeviceId = item.DeviceId,
                                        AssetTag = item.AssetTag,
                                        IsSelected = item.IsSelected,
                                        IsVoid = true,
                                        IsReturn = item.IsReturn,
                                    });
                                    ViewBag.Void = false;
                                }
                                // Select to Void Sales Transaction
                                else if (item.IsSelected == true && item.IsReturn == false && item.IsVoid == false)
                                {
                                    request.devices.Add(new DeviceEf()
                                    {
                                        DeviceId = item.DeviceId,
                                        AssetTag = item.AssetTag,
                                        IsSelected = item.IsSelected,
                                        IsVoid = true,
                                        IsReturn = item.IsReturn,
                                    });
                                    ViewBag.Void = false;
                                }
                                // Already Voided
                                else
                                {
                                    request.devices.Add(new DeviceEf()
                                    {
                                        DeviceId = item.DeviceId,
                                        AssetTag = item.AssetTag,
                                        IsSelected = item.IsSelected,
                                        IsVoid = item.IsVoid,
                                        IsReturn = item.IsReturn,
                                    });
                                    ViewBag.Void = true;
                                }
                            }
                        }
                    }
                }

                if (SearchOrderType == "OR")
                {
                    ViewBag.OrderRescan = true;
                }

                ViewBag.ExistingDevices = true;
            }

            // If devices does not exist, then add new devices
            else
            {
                var ErrorCode = ReturnPOSData.ErrorCode;
                if (ErrorCode == "-1")
                {
                    for (int i = 0; i < ReturnPOSData.SerialNos.Count; i++)
                    {
                        if(ReturnPOSData.SerialNos[i][0] == 'S' || ReturnPOSData.SerialNos[i][0] == 's')
                        {
                            request.devices.Add(new DeviceEf()
                            {
                                DeviceId = ReturnPOSData.SerialNos[i].Substring(1),
                                AssetTag = "",
                                IsSelected = true
                            });
                        }
                        else
                        {
                            request.devices.Add(new DeviceEf()
                            {
                                DeviceId = ReturnPOSData.SerialNos[i],
                                AssetTag = "",
                                IsSelected = true
                            });
                        }
                    }
                }
            }
            return PartialView("Result", request);
        }

        //==========================
        // Process - POST
        //==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Process(BedTransactionDisplayViewModel bedDisplayViewModel, string SubmitType)
        {
            BedResponse result = null;

            //---------------------------
            // SUBMIT - ORDER
            //---------------------------
            if (SubmitType.ToUpper() == "ORDER")
            {
                bedDisplayViewModel.transactionId = bedDisplayViewModel.transactionId.Trim() + "-OR";

                //Process Order and Return Result
                result = ProcessOR(bedDisplayViewModel);

                // If enrollment devices response is not null, check to make sure the 'Status Code' is 'SUCCESS' and save the transaction into DB.
                if (result.enrollDevicesResponse != null)
                {
                    if (result.enrollDevicesResponse.statusCode.ToUpper() == "SUCCESS")
                    {
                        // Record Transaction History
                        RecordTransactionHistory(result, bedDisplayViewModel);
                        // Save Transaction 
                        SaveTransaction(result, bedDisplayViewModel, "OR");
                    }
                }
            }

            //---------------------------
            // SUBMIT - VOID
            //---------------------------
            else if (SubmitType.ToUpper() == "VOID")
            {
                string VoidTranscationNumber = bedDisplayViewModel.returnVoidNumber.Trim();
                var ReturnPOSData = ReturnSubroutine(VoidTranscationNumber);

                //Transaction ID
                bedDisplayViewModel.transactionId = bedDisplayViewModel.transactionId.Trim() + "-VD";

                var ErrorCode = ReturnPOSData.ErrorCode;
                if (ErrorCode == "-1")
                {
                    //- Process Void Transaction and return result.
                    result = ProcessVD(bedDisplayViewModel, ReturnPOSData);

                    if (result != null)
                    {
                        if (result.enrollDevicesResponse != null)
                        {
                            // If Status Code from Apple is a Success, Record and Save Transaction.
                            if (result.enrollDevicesResponse.statusCode.ToUpper() == "SUCCESS")
                            {
                                RecordTransactionHistory(result, bedDisplayViewModel);
                                SaveTransaction(result, bedDisplayViewModel, "VD");
                            }
                        }
                    }
                    else
                    {
                        ViewBag.Error = true;
                        return View("Process");
                    }
                }
                else
                {
                    ViewBag.Error = true;
                    return View("Process");
                }
            }

            //---------------------------
            // SUBMIT - RETURN
            //---------------------------
            else if (SubmitType.ToUpper() == "RETURN")
            {
                string ReturnTranscationNumber = bedDisplayViewModel.returnTrasnactionNumber.Trim();
                var ReturnPOSData = ReturnSubroutine(ReturnTranscationNumber);

                //Transaction ID
                bedDisplayViewModel.transactionId = bedDisplayViewModel.transactionId.Trim() + "-RE";

                var ErrorCode = ReturnPOSData.ErrorCode;
                if (ErrorCode == "-1")
                {
                    // - Process Return Transaction and return result.
                    result = ProcessRE(bedDisplayViewModel, ReturnPOSData);

                    // - Make sure the response is not null. If it is, return error.
                    if (result != null)
                    {
                        // - Make sure the enroll devices response from Apple is not null. If it is, return error.
                        if (result.enrollDevicesResponse != null)
                        {
                            // If Status Code from Apple is a Success, Record and Save Transaction.
                            if (result.enrollDevicesResponse.statusCode.ToUpper() == "SUCCESS")
                            {
                                RecordTransactionHistory(result, bedDisplayViewModel);
                                SaveTransaction(result, bedDisplayViewModel, "RE");
                            }
                        }
                    }
                    else
                    {
                        ViewBag.Error = true;
                        return View("Process");
                    }
                }
                else
                {
                    ViewBag.Error = true;
                    return View("Process");
                }
            }
            return View("Process", result);
        }

        //==================================
        // ProcessOR
        // - Process Order Transaction Devices
        //==================================
        private BedResponse ProcessOR(BedTransactionDisplayViewModel bedDisplayViewModel)
        {
            // Create a New BED Request.
            var request = new BedRequest();
            request.depResellerId = bedDisplayViewModel.depResellerId;
            request.transactionId = bedDisplayViewModel.transactionId.Trim();

            // Create a New BED Request Context.
            request.requestContext = new BedRequestcontext();
            request.requestContext.langCode = bedDisplayViewModel.langCode;
            request.requestContext.shipTo = bedDisplayViewModel.shipTo;
            request.requestContext.timeZone = bedDisplayViewModel.timeZone;

            // Create a new BED order.
            // - You will need this to send off to Apple.
            var order = new BedOrder();
            order.customerId = bedDisplayViewModel.customerId;
            order.orderDate = bedDisplayViewModel.orderDate;
            order.orderNumber = bedDisplayViewModel.orderNumber;
            order.orderType = "OR";
            order.poNumber = bedDisplayViewModel.poNumber;

            // Create a new BED delivery.
            // - You will need this to send off to Apple.
            var deliver = new BedDelivery();
            deliver.deliveryNumber = bedDisplayViewModel.deliveryNumber;
            deliver.shipDate = bedDisplayViewModel.shipDate;

            // - Add each selected device to the BED device object and BED delivery object.
            foreach (var device in bedDisplayViewModel.devices)
            {
                if (device.IsSelected)
                {
                    var dev = new BedDevice();
                    dev.assetTag = device.AssetTag.Trim() == null ? "" : device.AssetTag.Trim();
                    dev.deviceId = device.DeviceId.Trim();
                    deliver.devices.Add(dev);
                }
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

        //==================================
        // ProcessRE
        // - Process Return Transaction Devices
        //==================================
        private BedResponse ProcessRE(BedTransactionDisplayViewModel bedDisplayViewModel, Transaction POSData)
        {
            int SelectDevices = 0;
            int NumberOfDevicesPOS = int.Parse(POSData.SerialNoCount);
            foreach (var item in bedDisplayViewModel.devices)
            {
                if (item.IsReturn)
                {
                    SelectDevices++;
                }
            }

            // If there are no selected devices to be returned, return a NULL BedResponse
            if (SelectDevices == 0)
            {
                BedResponse NoResult = new BedResponse();
                NoResult = null;
                return NoResult;
            }
            // If the selected devices does not match up with the total count of devices returned by POS, return a Null BedResponse
            else if (SelectDevices != NumberOfDevicesPOS)
            {
                BedResponse NoResult = new BedResponse();
                NoResult = null;
                return NoResult;
            }

            string OriginalTransaction = POSData.TransactionNo;

            // Create a new BED Request.
            var request = new BedRequest();
            request.depResellerId = bedDisplayViewModel.depResellerId;
            request.transactionId = bedDisplayViewModel.transactionId.Trim();

            // Create a new BED Request Context.
            request.requestContext = new BedRequestcontext();
            request.requestContext.langCode = bedDisplayViewModel.langCode;
            request.requestContext.shipTo = bedDisplayViewModel.shipTo;
            request.requestContext.timeZone = bedDisplayViewModel.timeZone;

            // Create a new BED order.
            // - You will need this to send off to Apple.
            var order = new BedOrder();
            order.customerId = bedDisplayViewModel.customerId;
            order.orderDate = bedDisplayViewModel.orderDate;
            order.orderNumber = bedDisplayViewModel.orderNumber;
            order.orderType = "RE";
            order.poNumber = bedDisplayViewModel.poNumber;

            // Create a new BED delivery.
            var deliver = new BedDelivery();
            deliver.deliveryNumber = bedDisplayViewModel.deliveryNumber;
            deliver.shipDate = bedDisplayViewModel.shipDate;

            // - Add each return device to the BED device object and BED delivery object.
            foreach (var device in bedDisplayViewModel.devices)
            {
                if (device.IsReturn)
                {
                    var dev = new BedDevice();
                    dev.assetTag = device.AssetTag == null ? "" : device.AssetTag;
                    dev.deviceId = device.DeviceId;
                    deliver.devices.Add(dev);
                }
            }

            order.deliveries.Add(deliver);
            request.orders.Add(order);

            // You need to Serialize the result into JSON format to send to Apple
            var json = JsonConvert.SerializeObject(request, Formatting.Indented);
            Debug.WriteLine(json);

            // Post Request to Apple and Return DEP Result
            // De-serialize the JSON response from Apple and return the result
            var result = JsonConvert.DeserializeObject<BedResponse>(PostDEP(json));

            // If the result is not null, go update the Enroll Devices.
            // This will flag the devices as RETURN and unflag them in Enroll.
            if (result != null)
            {
                UpdateEnrollDevices(bedDisplayViewModel, OriginalTransaction);
            }

            return result;
        }

        //==================================
        // ProcessVD
        // - Process Void Transaction Devices
        //==================================
        private BedResponse ProcessVD(BedTransactionDisplayViewModel bedDisplayViewModel, Transaction POSData)
        {
            int SelectDevices = 0;
            int NumberOfDevicesPOS = int.Parse(POSData.SerialNoCount);
            foreach (var item in bedDisplayViewModel.devices)
            {
                if (item.IsVoid)
                {
                    SelectDevices++;
                }
            }

            // If there are no selected devices to be voided, return a NULL BedResponse
            if (SelectDevices == 0)
            {
                BedResponse NoResult = new BedResponse();
                NoResult = null;
                return NoResult;
            }
            // If the selected devices does not match up with the total count of devices returned by POS, return a Null BedResponse
            else if (SelectDevices != NumberOfDevicesPOS)
            {
                BedResponse NoResult = new BedResponse();
                NoResult = null;
                return NoResult;
            }

            string OriginalTransaction = bedDisplayViewModel.originalTransactionNumber;

            var request = new BedRequest();
            // DEP Reseller ID
            request.depResellerId = bedDisplayViewModel.depResellerId;
            // Transaction ID
            request.transactionId = bedDisplayViewModel.transactionId.Trim();

            request.requestContext = new BedRequestcontext();
            // Lang Code
            request.requestContext.langCode = bedDisplayViewModel.langCode;
            // Ship To
            request.requestContext.shipTo = bedDisplayViewModel.shipTo;
            // Time Zone
            request.requestContext.timeZone = bedDisplayViewModel.timeZone;

            // Create a new BED order.
            // - You will need this to send off to Apple.
            var order = new BedOrder();
            order.customerId = bedDisplayViewModel.customerId;
            order.orderDate = bedDisplayViewModel.orderDate;
            order.orderNumber = bedDisplayViewModel.orderNumber;
            order.orderType = "VD";
            order.poNumber = bedDisplayViewModel.poNumber;
            request.orders.Add(order);


            //Serialize the data in JSON format for Apple.
            var json = JsonConvert.SerializeObject(request, Formatting.Indented);
            //Debug.WriteLine(json);

            // POST DEP
            // - Send it off to Apple.
            var DepResult = PostDEP(json);

            // De-serialize the result back from Apple.
            var result = JsonConvert.DeserializeObject<BedResponse>(DepResult);

            // If the result is not null, go update the Enroll Devices.
            // This will flag the devices as VOID and unflag them as Enroll.
            if (result != null)
            {
                UpdateEnrollDevices(bedDisplayViewModel, OriginalTransaction);
            }

            // Return result from Apple.
            return result;
        }

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
        // Save Transaction
        //==================================
        private bool SaveTransaction(BedResponse result, BedTransactionDisplayViewModel bedDisplayViewModel, string orderType)
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
                bodyEf.DepResellerId = bedDisplayViewModel.depResellerId;
                bodyEf.TransactionId = bedDisplayViewModel.transactionId.Trim();

                contextEf.LangCode = bedDisplayViewModel.langCode;
                contextEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;
                contextEf.ShipTo = bedDisplayViewModel.shipTo;
                contextEf.TimeZone = bedDisplayViewModel.timeZone;

                bodyEf.RequestContext = contextEf;

                //Check Order Type
                if (orderType == "OR")
                {
                    foreach (var device in bedDisplayViewModel.devices)
                    {
                        var deviceEf = new DeviceEf();
                        deviceEf.IsSelected = device.IsSelected;
                        deviceEf.AssetTag = device.AssetTag.Trim();
                        deviceEf.DeviceId = device.DeviceId.Trim();
                        deviceEf.Stamp = DateTime.Now;
                        deviceEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;
                        deliveryEf.Devices.Add(deviceEf);
                    }
                }

                deliveryEf.DeliveryNumber = bedDisplayViewModel.deliveryNumber;
                deliveryEf.ShipDate = bedDisplayViewModel.shipDate;
                deliveryEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;

                orderEf.TransactionId = bedDisplayViewModel.transactionId.Trim();
                orderEf.CustomerDepId = bedDisplayViewModel.customerDepId;
                orderEf.Deliveries.Add(deliveryEf);
                orderEf.OrderDate = bedDisplayViewModel.orderDate;
                orderEf.OrderNumber = bedDisplayViewModel.orderNumber;
                orderEf.OrderType = orderType; // bedDisplayViewModel.orderType;
                orderEf.PoNumber = bedDisplayViewModel.poNumber;
                orderEf.DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId;

                bodyEf.Orders.Add(orderEf);
                bodyEf.RequestContext = contextEf;

                using (var _db = new _dbContext())
                {
                    _db.RequestBodiesEf.Add(bodyEf);
                    _db.SaveChanges();
                }

                // add service record to be processed by Windows Services for "Check Transaction Status"
                AddServiceRecord(result.deviceEnrollmentTransactionId, bedDisplayViewModel.timeZone, bedDisplayViewModel.depResellerId, bedDisplayViewModel.langCode, bedDisplayViewModel.shipTo, orderType, bedDisplayViewModel.orderNumber, bedDisplayViewModel.transactionId.Trim());
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        //==================================
        // Record Transaction History
        //==================================
        public void RecordTransactionHistory(BedResponse result, BedTransactionDisplayViewModel bedDisplayViewModel)
        {
            DateTime Stamp = DateTime.Now;

            if (bedDisplayViewModel.SubmitType == "ORDER")
            {
                string POSTransaction = bedDisplayViewModel.orderNumber.Substring(2);
                using (_dbContext _db = new _dbContext())
                {
                    foreach (var item in bedDisplayViewModel.devices)
                    {
                        TransactionHistory Transaction = new TransactionHistory
                        {
                            UserName = User.Identity.Name.ToString(),
                            TimeStamp = Stamp,
                            Device = item.DeviceId,
                            Details = bedDisplayViewModel.SubmitType,
                            DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId,
                            TransactionId = bedDisplayViewModel.transactionId,
                            POSTransaction = POSTransaction
                        };
                        _db.TransactionHistorys.Add(Transaction);
                    }
                    _db.SaveChanges();
                }
            }

            else if (bedDisplayViewModel.SubmitType == "RETURN")
            {
                string POSTransaction = bedDisplayViewModel.returnTrasnactionNumber;
                using (_dbContext _db = new _dbContext())
                {
                    foreach (var item in bedDisplayViewModel.devices)
                    {
                        if (item.IsReturn == true)
                        {
                            TransactionHistory Transaction = new TransactionHistory
                            {
                                UserName = User.Identity.Name.ToString(),
                                TimeStamp = Stamp,
                                Device = item.DeviceId,
                                Details = bedDisplayViewModel.SubmitType,
                                DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId,
                                TransactionId = bedDisplayViewModel.transactionId,
                                POSTransaction = POSTransaction
                            };
                            _db.TransactionHistorys.Add(Transaction);
                        }
                    }
                    _db.SaveChanges();
                }
            }
            else
            {
                string POSTransaction = bedDisplayViewModel.returnVoidNumber;
                using (_dbContext _db = new _dbContext())
                {
                    foreach (var item in bedDisplayViewModel.devices)
                    {
                        if (item.IsVoid == true)
                        {
                            TransactionHistory Transaction = new TransactionHistory
                            {
                                UserName = User.Identity.Name.ToString(),
                                TimeStamp = Stamp,
                                Device = item.DeviceId,
                                Details = bedDisplayViewModel.SubmitType,
                                DeviceEnrollmentTransactionId = result.deviceEnrollmentTransactionId,
                                TransactionId = bedDisplayViewModel.transactionId,
                                POSTransaction = POSTransaction
                            };
                            _db.TransactionHistorys.Add(Transaction);
                        }
                    }
                    _db.SaveChanges();
                }
            }
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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ClientCertificates = certificates;
                request.ContentType = "application/json";
                request.Method = "POST";
                request.ContentLength = jsonData.Length;
                var classLog = JsonConvert.DeserializeObject<BedRequest>(jsonData);
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

                    //Debug.WriteLine(responseText);
                    //var bedResponse = JsonConvert.DeserializeObject<BedResponse>(responseText);
                    //var bedJsonResponse = JsonConvert.SerializeObject(bedResponse, Formatting.Indented);
                    //Debug.WriteLine(bedJsonResponse);

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

        //==========================
        // Check For Enroll Devices
        // - Return a list of enrolled devices.
        //==========================
        public List<DeviceEf> CheckForEnrollDevices(string TransactionNumber)
        {
            List<DeviceEf> ReturnDeviceList = new List<DeviceEf>();
            using (var _db = new _dbContext())
            {
                ReturnDeviceList = _db.DevicesEf.Where(x => x.DeliveryEf.OrderEf.OrderNumber == TransactionNumber).ToList();
            }

            return ReturnDeviceList;
        }

        //==========================
        // Update Enroll Devices
        //==========================
        public void UpdateEnrollDevices(BedTransactionDisplayViewModel bedDisplayViewModel, string ReturnTransactionNumber)
        {
            string SubmitType = bedDisplayViewModel.SubmitType;
            var ReturnEnrollDevices = CheckForEnrollDevices("OR" + ReturnTransactionNumber);

            // Check for Enrolled Devices
            if (ReturnEnrollDevices != null)
            {
                // Check for Submit Type - Return
                if (SubmitType == "RETURN")
                {
                    _dbContext _db = new _dbContext();

                    // - Loop through all the return devices
                    foreach (var item in ReturnEnrollDevices)
                    {
                        // - Loop through devices being returned
                        foreach (var device in bedDisplayViewModel.devices)
                        {
                            // Compare matching devices - Enrolled Device & Return Device
                            if (item.DeviceId == device.DeviceId)
                            {
                                // If the return device is marked 'RETURN', update the enrolled device as return and de-enrolled the device.
                                if (device.IsReturn == true)
                                {
                                    item.IsSelected = false;
                                    item.IsReturn = true;
                                    _db.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    _db.SaveChanges();
                }
                // Submit Type - Void
                else
                {
                    _dbContext _db = new _dbContext();

                    //Split Order Number
                    //- You need to split order number because this will determine if the void was on a sales or return transaction.
                    //- If the count is 1, then it's a sale. If count is 2, then it's a return.
                    string[] VoidType = bedDisplayViewModel.orderNumber.Split('-');
                    int VoidTypeCount = VoidType.Count();

                    // - Loop through all the return devices
                    foreach (var item in ReturnEnrollDevices)
                    {
                        // - Loop through devices being returned
                        foreach (var device in bedDisplayViewModel.devices)
                        {
                            // Compare matching devices - Enrolled Device & Return Device
                            if (item.DeviceId == device.DeviceId)
                            {
                                if (VoidTypeCount == 1)
                                {
                                    // If the device is marked 'VOID', update the enrolled device as return and de-enrolled the device.
                                    if (device.IsVoid == true)
                                    {
                                        item.IsSelected = false;
                                        item.IsReturn = false;
                                        item.IsVoid = false;
                                        _db.Entry(item).State = EntityState.Modified;
                                    }
                                }
                                else
                                {
                                    // If the device is marked 'VOID', update the enrolled device as return and de-enrolled the device.
                                    if (device.IsVoid == true)
                                    {
                                        item.IsSelected = true;
                                        item.IsReturn = false;
                                        item.IsVoid = false;
                                        _db.Entry(item).State = EntityState.Modified;
                                    }
                                }
                            }
                        }
                    }
                    _db.SaveChanges();
                }
            }
        }
    }
}