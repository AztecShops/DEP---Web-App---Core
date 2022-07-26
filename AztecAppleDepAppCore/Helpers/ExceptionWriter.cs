using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using AztecAppleDepApp;
using Microsoft.AspNetCore.Http;
using AztecAppleDepApp.Helpers;

namespace AztecAppleDepApp.Helpers
{
    public class ExceptionWriter
    {
        public void WriteErrorToFile(Exception ex)
        {
            string TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string FileName = TimeStamp + ".txt";
            /*string targetFolder = HttpContextHelper.Current.MyServer.MapPath("~/Logs/");*/
            string targetFolder = MyServer.MapPath("~/Logs/");
            string targetPath = Path.Combine(targetFolder, FileName);

            // Create a new file     
            using (StreamWriter sw = File.CreateText(targetPath))
            {
                sw.WriteLine("---------------------------------------------------");
                sw.WriteLine("New file created: {0}", DateTime.Now.ToString());
                sw.WriteLine("The following error(s):");
                sw.WriteLine("---------------------------------------------------");

                while (ex != null)
                {
                    sw.WriteLine(ex.GetType().FullName);
                    sw.WriteLine("Message : " + ex.Message);
                    sw.WriteLine("StackTrace : " + ex.StackTrace);
                    ex = ex.InnerException;
                }
            }
        }

        public bool WriteErrorToFileString(string ex)
        {
            string TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string FileName = TimeStamp + ".txt";
            /*string targetFolder = HttpContextHelper.Current.Server.MapPath("~/Logs/");*/
            string targetFolder = MyServer.MapPath("~/Logs/");
            string targetPath = Path.Combine(targetFolder, FileName);

            // Create a new file     
            using (StreamWriter sw = File.CreateText(targetPath))
            {
                sw.WriteLine("---------------------------------------------------");
                sw.WriteLine("New file created: {0}", DateTime.Now.ToString());
                sw.WriteLine("The following error(s):");
                sw.WriteLine("---------------------------------------------------");
                sw.WriteLine(ex);
            }

            return true;
        }
    }
}