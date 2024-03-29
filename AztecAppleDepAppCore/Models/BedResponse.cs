﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class BedResponse
    {
        public string transactionId { get; set; } // Unique transaction id returned in case of ShipTo error scenario
        public string deviceEnrollmentTransactionId { get; set; } // The unique identifier for the transaction, generated by ACC.
        public Enrolldevicesresponse enrollDevicesResponse { get; set; }
        public Enrolldeviceerrorresponse enrollDeviceErrorResponse { get; set; }
    }

    public class Enrolldeviceerrorresponse : BaseClass
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    public class Enrolldevicesresponse : BaseClass
    {
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
    }
}