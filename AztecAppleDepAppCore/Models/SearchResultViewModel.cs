using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class SearchResultViewModel
    {
        public string RecId { get; set; }
        public string DepTransactionId { get; set; }
        public string DepTransactionType { get; set; }
        public string DepTransactionDate { get; set; }
        public string DepTransactoinNote { get; set; }
    }
}