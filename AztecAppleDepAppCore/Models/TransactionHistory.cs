using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    [Table("TransactionHistory")]
    public class TransactionHistory
    {
        [Key]
        public int ID { get; set; }
        public string UserName { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Device { get; set; }
        public string Details { get; set; }
        public string DeviceEnrollmentTransactionId { get; set; }
        public string TransactionId { get; set; }
        public string POSTransaction { get; set; }
    }
}