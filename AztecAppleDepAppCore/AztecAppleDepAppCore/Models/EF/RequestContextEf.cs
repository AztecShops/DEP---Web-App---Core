using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    [Table("RequestContextsEf")]
    public class RequestContextEf : BaseClass
    {
        [StringLength(50)]
        public string ShipTo { get; set; }

        [StringLength(50)]
        public string TimeZone { get; set; }

        [StringLength(50)]
        public string LangCode { get; set; }

        //public virtual RequestBodyEf RequestBody { get; set; }
    }
}