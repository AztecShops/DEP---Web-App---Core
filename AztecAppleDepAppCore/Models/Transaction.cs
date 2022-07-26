using System.Collections.Generic;

namespace Dep
{
	public class Transaction
	{
		//SUBR_UPDATE
		public string TransactionNo { get; set; }
		public string ReturnTransactionId { get; set; }
		public string TypeOfSearch { get; set; }
		public List<string> SerialNos { get; set; }
		public string SerialNoCount { get; set; }

		// SUBR_SERIALNUMBER
		public string UpdateFlag { get; set; }
		public string OldSerialNo { get; set; }
		public string NewSerialNo { get; set; }
		public string BackupFlag { get; set; }

		public string ErrorCode { get; set; }
		public string ErrorMsg { get; set; }
	}
}
