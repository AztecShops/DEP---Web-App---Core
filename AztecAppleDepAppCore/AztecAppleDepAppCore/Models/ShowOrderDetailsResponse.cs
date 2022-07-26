using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class ShowOrderDetailsResponse
    {
        public string deviceEnrollmentTransactionID { get; set; }
        [Display(Name = "Responded On")]
        public DateTime respondedOn { get; set; }
        public DateTime completedOn { get; set; }
        public Order[] orders { get; set; }
        [Display(Name = "Status Code")]
        public string statusCode { get; set; }
        public ShowOrderErrorResponse showOrderErrorResponse { get; set; }
        public checkTransactionErrorResponse[] checkTransactionErrorResponse { get; set; }
    }

    public class Order
    {
        [Display(Name = "Order Number")]
        public string orderNumber { get; set; }
        [Display(Name = "Order Date")]
        public DateTime orderDate { get; set; }
        public string orderType { get; set; }
        public string customerId { get; set; }
        [Display(Name = "P.O. Number")]
        public string poNumber { get; set; }
        public List<Delivery> deliveries { get; set; }
        //public Delivery[] deliveries { get; set; }

        public string orderPostStatus { get; set; }
        public string orderPostStatusMessage { get; set; }

        [Display(Name = "Order Status Code")]
        public string showOrderStatusCode { get; set; }
        [Display(Name = "Order Status Message")]
        public string showOrderStatusMessage { get; set; }
    }

    public class Delivery
    {
        public string deliveryNumber { get; set; }
        public DateTime shipDate { get; set; }
        public List<Device> devices { get; set; }
        //public Device[] devices { get; set; }

        public string deliveryPostStatus { get; set; }
        public string deliveryPostStatusMessage { get; set; }

    }

    public class Device
    {
        public string deviceId { get; set; }
        public string assetTag { get; set; }

        public string devicePostStatus { get; set; }
        public string devicePostStatusMessage { get; set; }
    }

}

