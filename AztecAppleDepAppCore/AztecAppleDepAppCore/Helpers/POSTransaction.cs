using Dep;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Helpers
{
    public class POSTransaction
    {
        //==========================
        // GetPOSData
        // - Returns the data from Karen's subroutine.
        //==========================
        public Transaction GetPOSData(string TransactionNumber)
        {
            string result = DepTransaction.Update(TransactionNumber);
            Transaction transaction = JsonConvert.DeserializeObject<Transaction>(result);
            return transaction;
        }
    }
}