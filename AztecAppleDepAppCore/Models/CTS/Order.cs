using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.CTS
{
    public class Order
    {
        [Display(Name = "Order Number")]
        public string orderNumber { get; set; }
        [Display(Name = "Order Post Status")]
        public string orderPostStatus { get; set; }
        [Display(Name = "Order Post Status Message")]
        public string orderPostStatusMessage { get; set; }
        public Delivery[] deliveries { get; set; }
    }
}