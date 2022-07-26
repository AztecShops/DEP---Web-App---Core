using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.CTS
{
    public class Delivery
    {
        [Display(Name = "Delivery Number")]
        public string deliveryNumber { get; set; }
        [Display(Name = "Delivery Post Status")]
        public string deliveryPostStatus { get; set; }
        [Display(Name = "Delivery Post Status Message")]
        public string deliveryPostStatusMessage { get; set; }
        public Device[] devices { get; set; }
    }
}