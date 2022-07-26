using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class ErpApiResponse
    {
        public ErpApiResponse()
        {
            SerialNums = new List<string>();
            AssetNums = new List<string>();
        }

        public string DepCustomerName { get; set; }

        public int ErrNum { get; set; }
        public string ErrMsg { get; set; }
        public string PosTransNum { get; set; }

        // Request Context
        public string ShipTo { get; set; }

        // orders
        public string OrderNum { get; set; }
        public string OrderDate { get; set; }
        public string OrderType { get; set; }       // OR, RE, VO, OV
        public string CustomerDepId { get; set; }
        public string PoNum { get; set; }

        // deliveries
        public string DeliveryNum { get; set; }
        public string ShipDate { get; set; }

        // devices
        public List<string> SerialNums { get; set; }
        public List<string> AssetNums { get; set; }

    }
}