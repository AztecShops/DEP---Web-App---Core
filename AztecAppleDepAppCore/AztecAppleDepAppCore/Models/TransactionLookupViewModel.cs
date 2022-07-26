using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class TransactionLookupViewModel : BaseClass
    {
        public TransactionLookupViewModel()
        {
            deviceIds = new List<string>();
            assetTags = new List<string>();
            devices = new List<DeviceEf>();
        }

        [Display(Name = "Transaction Number")]
        public string TransactionNo { get; set; }
        public string ReturnTransactionId { get; set; }
        public string TypeOfSearch { get; set; }
        public List<string> SerialNos { get; set; }
        public string SerialNoCount { get; set; }
        public string UpdateFlag { get; set; }
        public string OldSerialNo { get; set; }
        public string NewSerialNo { get; set; }
        public string BackupFlag { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMsg { get; set; }

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
    }
}