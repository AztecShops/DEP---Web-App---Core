using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AztecAppleDepApp.Models
{
    [Table("DevicesEf")]
    public class DeviceEf : BaseClass
    {
        public string DeviceId { get; set; }        // The Serial Number/IMEI or MEID number of the device.The value for this field should be entered in Upper case.
        public string AssetTag { get; set; }        // Additional info about the device.
        public bool IsSelected { get; set; }
        public bool IsVoid { get; set; }
        public bool IsReturn { get; set; }

        public virtual DeliveryEf DeliveryEf { get; set; }
    }
}