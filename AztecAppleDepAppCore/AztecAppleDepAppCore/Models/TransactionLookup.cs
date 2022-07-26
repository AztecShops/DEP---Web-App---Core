using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class TransactionLookup
    {
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
    }
}