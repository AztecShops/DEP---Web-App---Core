using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    [Table("DeliveriesEf")]
    public class DeliveryEf : BaseClass
    {
        public DeliveryEf()
        {
            Devices = new List<DeviceEf>();
        }
        [StringLength(250)]
        public string DeliveryNumber { get; set; }          // The delivery no. corresponding to the delivery

        public string ShipDate { get; set; }              // The timestamp of shipment of the delivery. This should include the date and time and should be in a standard UTC format.

        public virtual List<DeviceEf> Devices { get; set; }

        public virtual OrderEf OrderEf { get; set; }
    }
}