using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    [Table("RequestBodiesEf")]
    public class RequestBodyEf : BaseClass
    {
        public RequestBodyEf()
        {
            Orders = new List<OrderEf>();
        }

        public virtual RequestContextEf RequestContext { get; set; }

        public string DepResellerId { get; set; }

        public string TransactionId { get; set; }       // Unique transaction ID provided by the reseller

        public virtual List<OrderEf> Orders { get; set; }
    }
}
