using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    [Table("DepServices")]
    public class DepService : BaseClass
    {
        //public string DeviceEnrollmentTransactionId { get; set; }
        public string ShipTo { get; set; }
        public string LangCode { get; set; }
        public string TimeZone { get; set; }
        public string DepResellerId { get; set; }

        public bool ReadyToProcess { get; set; }    // new record / ready to be processed
        public bool IsInProcess { get; set; }       // record is being processed
        public bool IsProcessed { get; set; }       // record is processed

        public string OrderType { get; set; }   // OR, VD, OV, RE
        public string OrderNumber { get; set; }     // NOT POS Transaction Number
        public string TransactionId { get; set; }       // Unique transaction ID provided by the reseller - POS Transaction #


        public int ProcessCounter { get; set; }     // how many times this record was processed
        public string ProcessStatus { get; set; }   // Status of process
    }
}