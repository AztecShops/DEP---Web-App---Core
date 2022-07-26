using System.Collections.Generic;

namespace AztecAppleDepApp.Models
{
    public class BedRequest
    {
        public BedRequest() { orders = new List<BedOrder>(); }

        public BedRequestcontext requestContext { get; set; }
        public string transactionId { get; set; }
        public string depResellerId { get; set; }
        public List<BedOrder> orders { get; set; }
    }

    public class BedRequestcontext
    {
        public string shipTo { get; set; }
        public string timeZone { get; set; }
        public string langCode { get; set; }
    }

    public class BedOrder
    {
        public BedOrder() { deliveries = new List<BedDelivery>(); }

        public string orderNumber { get; set; }
        public string orderDate { get; set; }
        public string orderType { get; set; }
        public string customerId { get; set; }
        public string poNumber { get; set; }
        public List<BedDelivery> deliveries { get; set; }
    }

    public class BedDelivery
    {
        public BedDelivery() { devices = new List<BedDevice>(); }

        public string deliveryNumber { get; set; }
        public string shipDate { get; set; }
        public List<BedDevice> devices { get; set; }
    }

    public class BedDevice
    {
        public string deviceId { get; set; }
        public string assetTag { get; set; }
    }

}