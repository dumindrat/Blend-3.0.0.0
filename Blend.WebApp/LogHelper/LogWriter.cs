using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Blend.WebApp.LogHelper
{
    public class LogWriter
    {
        private string m_exePath = string.Empty;
        private string logFolder = ConfigurationManager.AppSettings["StaticLogPath"];
        private string logFile = ConfigurationManager.AppSettings["StaticLogFile"];
        private string IsEnableLog4net = ConfigurationManager.AppSettings["IsEnableLog4Net"];


        public LogWriter()
        {
        }
        public void LogWrite(string logMessage, string trnsevent = "")
        {
            m_exePath = HttpContext.Current.Server.MapPath("~/") + logFolder;
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + logFile))
                {
                    Log(logMessage, w, trnsevent);
                }

            }
            catch (Exception ex)
            {
                //
            }
        }

        public void Log(string logMessage, TextWriter txtWriter, string trnEvent)
        {
            try
            {
                //txtWriter.Write("\r\nLog Entry : ");
                //txtWriter.Write("\rLog Entry : ");
                txtWriter.WriteLine("{0}. {1}. {2}. {3}.", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString(), trnEvent, logMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}