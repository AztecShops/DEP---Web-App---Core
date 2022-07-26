using IBMU2.UODOTNET;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Dep
{
	public class DepTransaction
	{
		//=====================================================================
		// Update
		// - Returns transaction id, backup flag, and list of serial numbers
		//=====================================================================
		public static string Update(string transactionId)
		{
			Transaction transaction = new Transaction()
			{
				TransactionNo = transactionId,
				SerialNos = new List<string>()
			};

			var uv = new UvDal();
			UniSession uSession = null;

			try
			{
				uSession = uv.UvConnect();

				if (uSession == null)
				{
					transaction.ErrorCode = "100";
					transaction.ErrorMsg = "Cannot connect to UV Server [" + ConfigurationManager.AppSettings["uvIp"] + "]";

					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}

				UniSubroutine uniSub = uSession.CreateUniSubroutine("SUBR_DEP_UPDATE", 6);
				uniSub.SetArg(0, "TRANSID" + uv.Vm + "RE.TRANSID" + uv.Vm + "TOS" + uv.Vm + "L.SERIAL" + uv.Vm + "SERIAL.CNT");     // Header
				uniSub.SetArg(1, transaction.TransactionNo);    // Parameter(s)
				uniSub.Call();

				// 3: "(TRANSACTIONID) not on file"
				transaction.ErrorCode = uniSub.GetArg(4);

				if (transaction.ErrorCode != "-1")
				{
					transaction.ErrorMsg = uniSub.GetArg(5);
					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}
				else
				{
					var ReturnDataHeader = uniSub.GetArg(2);
					var ReturnData = uniSub.GetArg(3);

					string[] data = ReturnData.Split(uv.Vm);

					transaction.TransactionNo = data[0];
					transaction.ReturnTransactionId = data[1];
					transaction.TypeOfSearch = data[2];
					transaction.SerialNos = data[3].Split(new[] { uv.Sm }).ToList();
					transaction.SerialNoCount = data[4];
					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}
			}
			catch (Exception ex)
			{
				transaction.ErrorCode = "101";
				transaction.ErrorMsg = ex.Message;
				return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
			}
			finally
			{
				uv.UvDisconnect(uSession);
			}
		}

		//========================================================================
		// CashFileRead
		// - Calls Karen's SUBR_DEP_SERIALNUMBER, UPDATE FLAG = 0 (return data)
		// - Returns transaction id, backup flag, and list of serial numbers
		//========================================================================
		public static string CashFileRead(string transactionNo)
		{
			Transaction transaction = new Transaction
			{
				TransactionNo = transactionNo,
				UpdateFlag = "0",
				SerialNos = new List<string>()
			};

			var uv = new UvDal();
			UniSession uSession = null;

			try
			{
				uSession = uv.UvConnect();

				if (uSession == null)
				{
					transaction.ErrorCode = "100";
					transaction.ErrorMsg = "Cannot connect to UV Server [" + ConfigurationManager.AppSettings["uvIp"] + "]";

					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}

				UniSubroutine uniSub = uSession.CreateUniSubroutine("SUBR_DEP_SERIALNUMBER", 6);
				uniSub.SetArg(0, "TRANSID" + uv.Vm + "UPDT.FLAG" + uv.Vm + "OLD.SERIAL" + uv.Vm + "NEW.SERIAL");     // Header
				uniSub.SetArg(1, transaction.TransactionNo + uv.Vm + transaction.UpdateFlag);    // Parameter(s)
				uniSub.Call();

				// 3: "(TRANSACTIONID) not on file"
				transaction.ErrorCode = uniSub.GetArg(4);

				if (transaction.ErrorCode != "-1")
				{
					transaction.ErrorMsg = uniSub.GetArg(5);
					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}
				else
				{
					var ReturnDataHeader = uniSub.GetArg(2);
					var ReturnData = uniSub.GetArg(3);
					string[] data = ReturnData.Split(uv.Vm);

					transaction.TransactionNo = data[0];
					transaction.BackupFlag = data[1];
					transaction.SerialNos = data[2].Split(new[] { uv.Sm }).ToList();
					transaction.SerialNoCount = data[3];
					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}
			}
			catch (Exception ex)
			{
				transaction.ErrorCode = "101";
				transaction.ErrorMsg = ex.Message;
				return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
			}
			finally
			{
				uv.UvDisconnect(uSession);
			}
		}

		//=============================================================================
		// CashFileUpdate
		// - Calls Karen's SUBR_DEP_SERIALNUMBER, UPDATE FLAG = 1 (update data)
		// - Returns transaction id, backup flag, and list of updated serial numbers
		//=============================================================================
		public static string CashFileUpdate(string transactionId, string oldSerialNo, string newSerialNo)
		{
			Transaction transaction = new Transaction()
			{
				TransactionNo = transactionId,
				UpdateFlag = "1",
				OldSerialNo = oldSerialNo,
				NewSerialNo = newSerialNo
			};

			var uv = new UvDal();
			UniSession uSession = null;

			try
			{
				uSession = uv.UvConnect();

				if (uSession == null)
				{
					transaction.ErrorCode = "100";
					transaction.ErrorMsg = "Cannot connect to UV Server [" + ConfigurationManager.AppSettings["uvIp"] + "]";

					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}

				UniSubroutine uniSub = uSession.CreateUniSubroutine("SUBR_DEP_SERIALNUMBER", 6);
				uniSub.SetArg(0, "TRANSID" + uv.Vm + "UPDT.FLAG" + uv.Vm + "OLD.SERIAL" + uv.Vm + "NEW.SERIAL"); // Header
				uniSub.SetArg(1, transaction.TransactionNo + uv.Vm + transaction.UpdateFlag + uv.Vm + oldSerialNo + uv.Vm + newSerialNo); // Parameter(s)
				uniSub.Call();

				transaction.ErrorCode = uniSub.GetArg(4);

				if (transaction.ErrorCode != "-1")
				{
					transaction.ErrorMsg = uniSub.GetArg(5);
					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}
				else
				{
					var ReturnDataHeader = uniSub.GetArg(2);
					var ReturnData = uniSub.GetArg(3);

					string[] data = ReturnData.Split(uv.Vm);

					transaction.TransactionNo = data[0];
					transaction.UpdateFlag = data[1];
					transaction.SerialNos = data[2].Split(new[] { uv.Sm }).ToList();
					transaction.SerialNoCount = data[3];
					return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
				}
			}
			catch (Exception ex)
			{
				transaction.ErrorCode = "101";
				transaction.ErrorMsg = ex.Message;
				return Newtonsoft.Json.JsonConvert.SerializeObject(transaction);
			}
			finally
			{
				uv.UvDisconnect(uSession);
			}
		}
	}
}