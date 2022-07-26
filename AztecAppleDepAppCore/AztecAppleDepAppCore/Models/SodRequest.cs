using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class SodRequest
    {
        public SodRequest()
        {
            orderNumbers = new List<string>();
            requestContext = new BedRequestcontext();
        }

        public BedRequestcontext requestContext { get; set; }
        public string depResellerId { get; set; }
        public List<string> orderNumbers { get; set; }
    }
}