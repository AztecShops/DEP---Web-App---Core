using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class BedTransactionSearchViewModel : BaseClass
    {
        public string TransactionLocation { get; set; }     // POS, DEP
    }
}