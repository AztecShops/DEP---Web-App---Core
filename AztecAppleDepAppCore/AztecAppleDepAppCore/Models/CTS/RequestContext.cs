using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.CTS
{
    public class RequestContext
    {
        public string shipTo { get; set; }
        public string timeZone { get; set; }
        public string langCode { get; set; }
    }
}