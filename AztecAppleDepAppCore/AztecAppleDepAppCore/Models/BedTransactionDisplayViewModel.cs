using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class BedTransactionDisplayViewModel : BaseClass
    {
        public BedTransactionDisplayViewModel()
        {
            deviceIds = new List<string>();
            assetTags = new List<string>();
            devices = new List<DeviceEf>();
        }

        [Display(Name = "Customer DEP ID")]
        public string customerDepId { get; set; }
        [Display(Name = "Transaction ID")]
        public string transactionId { get; set; }
        [Display(Name = "DEP Reseller ID")]
        public string depResellerId { get; set; }
        [Display(Name = "Ship To")]
        public string shipTo { get; set; }
        [Display(Name = "Time Zone")]
        public string timeZone { get; set; }
        [Display(Name = "Language Code")]
        public string langCode { get; set; }
        [Display(Name = "Order Number")]
        public string orderNumber { get; set; }
        [Display(Name = "Order Date")]
        public string orderDate { get; set; }
        [Display(Name = "Order Type")]
        public string orderType { get; set; }
        [Display(Name = "Customer ID")]
        public string customerId { get; set; }
        [Display(Name = "P.O. Number")]
        public string poNumber { get; set; }
        [Display(Name = "Delivery Number")]
        public string deliveryNumber { get; set; }
        [Display(Name = "Ship Date")]
        public string shipDate { get; set; }

        public List<DeviceEf> devices { get; set; }

        public List<string> deviceIds { get; set; }
        public List<string> assetTags { get; set; }

        [Display(Name = "Account Manager")]
        public string AccountManager { get; set; }
        [Display(Name = "Client Contact")]
        public string ClientContact { get; set; }
        [Display(Name = "Client Email")]
        public string ClientEmail { get; set; }
        [Display(Name = "Original Transaction Number")]
        public string originalTransactionNumber { get; set; }
        [Display(Name = "Return Transaction Number")]
        public string returnTrasnactionNumber { get; set; }
        [Display(Name = "Void Transaction Number")]
        public string returnVoidNumber { get; set; }

        public string SubmitType { get; set; }
    }
}