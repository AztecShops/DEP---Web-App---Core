using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class DepTransactionDetailViewModel
    {
        public DepTransactionDetailViewModel()
        {
            BodyEf = new RequestBodyEf();
            ContextEf = new RequestContextEf();
            OrderEf = new OrderEf();
            DeliveryEf = new DeliveryEf();
            DeviceEfs = new List<DeviceEf>();
        }

        public RequestBodyEf BodyEf { get; set; }
        public RequestContextEf ContextEf { get; set; }
        public OrderEf OrderEf { get; set; }
        public DeliveryEf DeliveryEf { get; set; }
        public List<DeviceEf> DeviceEfs { get; set; }
    }
}