using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class DepCustomer : BaseClass
    {
        public string CustomerId { get; set; }      // from POS system ... this can be same as CustomerDepId
        public string DepId { get; set; }
        public string AccountManager { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string TimeZone { get; set; }
        public string ShipTo { get; set; }
        public string LangCode { get; set; }
    }
}