using AztecAppleDepApp.Models;
using Dep;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Helpers
{
    public class DBTransaction
    {
        public bool CheckForVoidedTransaction(string TransactionNumber)
        {
            string ReturnTransaction = string.Empty;
            string OriginalTransaction = string.Empty;
            POSTransaction POSItem = new POSTransaction();
            Transaction ReturnPOSData = new Transaction();
            List<OrderEf> lReturnOrder;

            TransactionNumber = TransactionNumber.Trim();
            ReturnPOSData = POSItem.GetPOSData(TransactionNumber);
            OriginalTransaction = ReturnPOSData.TransactionNo;
            ReturnTransaction = ReturnPOSData.ReturnTransactionId;

            using (var _db = new _dbContext())
            {
                // Return Order Information.
                lReturnOrder = _db.OrdersEf.Where(x => x.TransactionId == "TX" + TransactionNumber + "-VD" && x.OrderType == "VD").ToList();
            }

            if(lReturnOrder.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}