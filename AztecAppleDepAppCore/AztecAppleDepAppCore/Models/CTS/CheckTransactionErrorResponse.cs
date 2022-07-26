using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.CTS
{
    public class CheckTransactionErrorResponse
    {
        public string errorMessage { get; set; }
        public string errorCode { get; set; }
    }
}