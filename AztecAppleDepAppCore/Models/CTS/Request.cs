using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.CTS
{
    public class Request
    {
        public RequestContext requestContext { get; set; }
        public string depResellerId { get; set; }
        public string deviceEnrollmentTransactionId { get; set; }
    }
}