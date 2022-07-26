using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.CTS
{
    public class Device
    {
        public string deviceId { get; set; }
        public string devicePostStatus { get; set; }
        public string devicePostStatusMessage { get; set; }
    }
}