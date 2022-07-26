using IBMU2.UODOTNET;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.UV
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
                var uvPath = ConfigurationManager.AppSettings["uvPath"].ToString();
                var uvLogin = ConfigurationManager.AppSettings["uvLogin"].ToString();
                var uvPass = ConfigurationManager.AppSettings["uvPass"].ToString();
                var uvIp = ConfigurationManager.AppSettings["uvIP"].ToString();
                var uvService = ConfigurationManager.AppSettings["uvService"].ToString();

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
            if (uos == null) return;
            UniObjects.CloseSession(uos);
            uos = null;
        }
    }
}