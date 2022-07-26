using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.CTS
{
    public class Response
    {
        [Display(Name = "Device Enrollment Transaction ID")]
        public string deviceEnrollmentTransactionID { get; set; }

        public CheckTransactionErrorResponse[] checkTransactionErrorResponse { get; set; }

        [Display(Name = "Transaction ID")]
        public string transactionId { get; set; }

        [Display(Name = "Completed On")]
        public DateTime completedOn { get; set; }

        public Order[] orders { get; set; }

        [Display(Name = "Status Code")]
        public string statusCode { get; set; }
    }
}