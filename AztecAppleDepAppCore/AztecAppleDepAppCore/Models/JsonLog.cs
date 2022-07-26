using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    [Table("JsonLogs")]
    public class JsonLog : BaseClass
    {
        public string JsonData { get; set; }
        public string TransRef { get; set; }     // POS transaction reference
        public string DataDirection { get; set; }   // inbound / outbound - Request / Response
        public string TransactionType { get; set; } // BED, CTS, SOD = Bulk Enroll Device, Check Transaction Status, Show Order Details
        public string OrderType { get; set; }   // OR:Order, RE:Return, OV:Override, VD:Void
    }
}