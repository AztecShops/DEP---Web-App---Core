using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    public class BaseClass
    {
        public int ID { get; set; }
        public DateTime Stamp { get; set; }
        public string ObjNum { get; set; }
        public bool Status { get; set; }
        public string DepTransactionId { get; set; }
        public bool IsRecordProcessed { get; set; }
        public string DeviceEnrollmentTransactionId { get; set; }
        public string Note { get; set; }
    }
}