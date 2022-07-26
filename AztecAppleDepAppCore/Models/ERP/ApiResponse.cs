using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.ERP
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            ErpSerialNumberList = new List<string>();
            ErpDescriptionList = new List<string>();
            ErpPluList = new List<string>();
        }

        public string TransactionId { get; set; }   // Original transaction scanned to be lookedup

        public string ErpDepFlag { get; set; }
        public string ErpSaleDate { get; set; }
        public string ErpTransactionId { get; set; }
        public string ErpSerialCount { get; set; }
        public string PosTransNum { get; set; }     // returned transaction # from POS system // maybe different

        public List<string> ErpSerialNumberList { get; set; }
        public List<string> ErpDescriptionList { get; set; }
        public List<string> ErpPluList { get; set; }

        public ErrorEnum ErrNum { get; set; }
        public string ErrMsg { get; set; }
    }
}