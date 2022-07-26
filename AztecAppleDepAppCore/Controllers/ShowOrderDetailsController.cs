using AztecAppleDepApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using AztecAppleDepApp.Helpers;

namespace AztecAppleDepApp.Controllers
{
    [Route("ShowOrderDetails")]
    public class ShowOrderDetailsController : Controller
    {
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchOrder([Bind(Prefix = "TransactionNumber")]string OrderNumber)
        {
            var result = new List<SearchResultViewModel>();
            var request = new SodRequest();

            using (var _db = new _dbContext())
            {
                //Return Transaction History Record
                var TranHisRec = _db.TransactionHistorys.Where(x => x.POSTransaction == OrderNumber.Trim()).FirstOrDefault();

                string ReturnTransctionID = "";

                if (TranHisRec != null)
                {
                    ReturnTransctionID = TranHisRec.TransactionId;
                }
                //Return Order Record 
                var recs = _db.OrdersEf.Where(x => x.TransactionId == ReturnTransctionID).OrderByDescending(x => x.Stamp).ToList();

                //If Record is not null and has a record, attach data to the 'request' object. This will be use to send off to Apple.
                if (recs != null && recs.Count > 0)
                {
                    request.depResellerId = recs[0].RequestBodyEf.DepResellerId;
                    request.orderNumbers.Add(recs[0].OrderNumber);
                    request.requestContext.langCode = recs[0].RequestBodyEf.RequestContext.LangCode;
                    request.requestContext.shipTo = recs[0].RequestBodyEf.RequestContext.ShipTo;
                    request.requestContext.timeZone = recs[0].RequestBodyEf.RequestContext.TimeZone;
                }
                else
                {
                    request.depResellerId = "8B30C50";
                    request.orderNumbers.Add("41624");
                    request.requestContext.langCode = "en";
                    request.requestContext.shipTo = "0000022176";
                    request.requestContext.timeZone = "420";
                }
            }

            var jsonRequest = JsonConvert.SerializeObject(request, Formatting.Indented);

            //Send jsonRequest to Apple and get Response back.
            var jsonResponse = ShowOrderDetails(jsonRequest);

            var response = new ShowOrderDetailsResponse();

            // Check if the jsonResponse from Apple is not null and De-serialize the json object and assign it to the 'response' object. Other wise, just return an empty 'response' object.
            if (jsonResponse != null)
            {
                response = JsonConvert.DeserializeObject<ShowOrderDetailsResponse>(jsonResponse);
            }

            return PartialView("Result", response);
        }

        #region PROCESS_DEP
        private string ShowOrderDetails(string jsonData)
        {
            try
            {
                // Get the Path of the SSL Cert.
                var sslPath = MyServer.MapPath("~/") + Properties.Settings.Default.CertPath + Properties.Settings.Default.CertName;

                // Create a new Cert Collection
                X509Certificate2Collection certificates = new X509Certificate2Collection();

                // Your Cert with the PW.
                var myCert = new X509Certificate2(sslPath, @"Hadaf00x!@");
                certificates.Add(myCert);

                //(Sender, Cert, Chain, Error)
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                // Create web request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("https://api-applecareconnect.apple.com/enroll-service/1.0/show-order-details"));

                // Attach the certificate to the request
                request.ClientCertificates = certificates;

                // Make content type JSON
                request.ContentType = "application/json";

                // POST Method
                request.Method = "POST";

                // Attach the length of the Json Data
                request.ContentLength = jsonData.Length;
                
                //Send the Json Data Away to Apple
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = jsonData;

                    streamWriter.Write(json);
                    streamWriter.Flush();
                }

                // Create web Response
                var httpResponse = (HttpWebResponse)request.GetResponse();

                // Get the response back from Apple
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    Console.WriteLine(responseText);
                    return responseText;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        #endregion PROCESS_DEP
    }
}