using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    [Table("OrdersEf")]
    public class OrderEf : BaseClass
    {
        public OrderEf()
        {
            Deliveries = new List<DeliveryEf>();
        }
        public string OrderNumber { get; set; }     //Order number pertaining to the order (NOT POS Transaction #)

        public string TransactionId { get; set; }       // Unique transaction ID provided by the reseller - POS Transaction #


        public string OrderDate { get; set; }     // The timestamp when the order was created. This should include the date and time and should be in a standard UTC format.

        public String OrderType { get; set; }       // Order type could be OR - Normal, RE - Return, VD - Void, OV - Override

        public string CustomerDepId { get; set; }

        public string PoNumber { get; set; }        // The PO. Number corresponding to the order

        public virtual List<DeliveryEf> Deliveries { get; set; }

        public virtual RequestBodyEf RequestBodyEf { get; set; }


    }
}