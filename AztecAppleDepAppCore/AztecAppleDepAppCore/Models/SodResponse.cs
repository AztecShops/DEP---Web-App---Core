using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class SodResponse
    {
        public SodResponse() { orders = new List<BedOrder>(); }

        public string deviceEnrollmentTransactionID { get; set; }
        public DateTime respondedOn { get; set; }
        public DateTime completedOn { get; set; }
        public List<BedOrder> orders { get; set; }
        public string statusCode { get; set; }
        public ShowOrderErrorResponse showOrderErrorResponse { get; set; }
        public checkTransactionErrorResponse[] checkTransactionErrorResponse { get; set; }
    }

    public class ShowOrderErrorResponse
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    public class checkTransactionErrorResponse
    {
        public string errorMessage { get; set; }
        public string errorCode { get; set; }
    }

}