using AztecAppleDepApp.Helpers;
using AztecAppleDepApp.Models;
using AztecAppleDepApp.Models.CTS;
using Dep;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace AztecAppleDepApp.Controllers
{
    public class CheckTransactionStatusController : Controller
    {
        // GET: CheckTransactionStatus
        public ActionResult Index()
        {
            return View();
        }

        // POST: /Search
        // - Search for the transaction and return the information.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(string TransactionNumber)
        {
            ExceptionWriter WriteError = new ExceptionWriter();
            try
            {
                string DeviceEnrollmentTransactionID = string.Empty;
                string OrderType = string.Empty;
                string TransNo = string.Empty;
                string ReturnTransNo = string.Empty;
                string Last4ReturnTransNo = string.Empty;
                List<OrderEf> ReturnOrderList;
                Request NewRequest = new Request();
                RequestContext NewRequestContext = new RequestContext();
                POSTransaction POSItem = new POSTransaction();
                Transaction ReturnPOSData = new Transaction();

                TransactionNumber = TransactionNumber.Trim();
                ReturnPOSData = POSItem.GetPOSData(TransactionNumber);
                OrderType = ReturnPOSData.TypeOfSearch;
                TransNo = ReturnPOSData.TransactionNo;

                if (OrderType == "OR")
                {
                    TransactionNumber = TransNo;
                }
                else if (OrderType == "RE")
                {
                    Last4ReturnTransNo = ReturnPOSData.ReturnTransactionId.Substring(ReturnPOSData.ReturnTransactionId.Length - 4);
                    ReturnTransNo = TransNo + "-" + Last4ReturnTransNo;
                    TransactionNumber = ReturnTransNo;
                }
                else
                {
                    TransactionNumber = TransNo;
                }

                using (var _db = new _dbContext())
                {
                    // Return Order Information.
                    ReturnOrderList = _db.OrdersEf.Where(x => x.OrderNumber == "OR" + TransactionNumber).ToList();
                }

                // If order exist, create a JSON request string and send it off to Apple. 
                // Apple will return a JSON response and display the result.
                if (ReturnOrderList.Count > 0)
                {
                    foreach (var item in ReturnOrderList)
                    {
                        DeviceEnrollmentTransactionID = item.DeviceEnrollmentTransactionId;
                    }

                    NewRequest.depResellerId = "8B30C50";
                    NewRequest.deviceEnrollmentTransactionId = DeviceEnrollmentTransactionID;
                    NewRequestContext.shipTo = "0000022176";
                    NewRequestContext.timeZone = "420";
                    NewRequestContext.langCode = "en";
                    NewRequest.requestContext = NewRequestContext;

                    var JsonRequest = JsonConvert.SerializeObject(NewRequest, Formatting.Indented);
                    var JsonResponse = CheckTransactionStatus(JsonRequest);
                    var Response = JsonConvert.DeserializeObject<Response>(JsonResponse);

                    return PartialView("Result", Response);
                }
                // Order doesn't exist, return nothing.
                else
                {
                    return PartialView("Result");
                }
            }
            catch (Exception ex)
            {
                //ExceptionWriter WriteError = new ExceptionWriter();
                WriteError.WriteErrorToFile(ex);
                return PartialView("Result");
            }
        }

        // ===========================
        // Check Transaction Status
        // - Sends a JSON string request to Apple and return with a JSON string response fro Apple.
        // ===========================
        public string CheckTransactionStatus(string JsonData)
        {
            ExceptionWriter WriteError = new ExceptionWriter();
            var sslPath = MyServer.MapPath("~/") + Properties.Settings.Default.CertPath + Properties.Settings.Default.CertName;
            try
            {
                X509Certificate2Collection certificates = new X509Certificate2Collection();
                var myCert = new X509Certificate2(sslPath, @"Hadaf00x!@");
                certificates.Add(myCert);
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                // Create web request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://api-applecareconnect.apple.com/enroll-service/1.0/check-transaction-status"));

                request.ClientCertificates = certificates;
                request.ContentType = "application/json";
                request.Method = "POST";
                request.ContentLength = JsonData.Length;

                // Send the Json Data Away to Apple
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = JsonData;

                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                // Create web Response
                var httpResponse = (HttpWebResponse)request.GetResponse();

                // Get the response back from Apple
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    return responseText;
                }
            }
            catch (Exception ex)
            {
                WriteError.WriteErrorToFile(ex);
                return null;
            }
        }
    }
}