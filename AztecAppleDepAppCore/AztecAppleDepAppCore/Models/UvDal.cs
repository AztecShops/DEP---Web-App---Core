using IBMU2.UODOTNET;
using System;
using System.Configuration;

namespace Dep
{
	public class UvDal
	{
		public char Vm = 'ý';
		public char Am = 'þ';
		public char Sm = 'ü';

		public UniSession UvConnect()
		{
			UniSession uos;
			try
			{
				var uvIp = ConfigurationManager.AppSettings["uvIp"];
				var uvLogin = ConfigurationManager.AppSettings["uvLogin"];
				var uvPass = ConfigurationManager.AppSettings["uvPass"];
				var uvPath = ConfigurationManager.AppSettings["uvPath"];
				var uvService = ConfigurationManager.AppSettings["uvService"];
				uos = UniObjects.OpenSession(uvIp, uvLogin, uvPass, uvPath, uvService);
			}
			catch (Exception)
			{
				uos = null;
			}
			return uos;
		}

		public void UvDisconnect(UniSession uos)
		{
			if (uos != null)
			{
				UniObjects.CloseSession(uos);
				uos = null;
			}
		}
	}
}
