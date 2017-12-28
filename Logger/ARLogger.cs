using System;
using System.IO;
using log4net;
using log4net.Config;

namespace Anthony.Logger
{

    public class ARLogger
    {
        private ILog mLog4NetLog;

        private ARLogger()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "ARLogger.dll.config");
            FileInfo configFile = new FileInfo(path);
            XmlConfigurator.Configure(configFile);
        }

        /// <summary>
        /// Get Logger instance with the class full name
        /// </summary>
        /// <returns></returns>
        /// <example>
        /// Type type = MethodBase.GetCurrentMethod().DeclaringType;
        /// 
        /// Logger logger= Logger.GetInstance(type);
        /// 
        /// // Output class full name in log
        /// logger.Info("Info");
        /// </example>
        public static ARLogger GetInstance(Type type)
        {
            ARLogger logger = new ARLogger();
            logger.mLog4NetLog = LogManager.GetLogger(type);
            return logger;
        }

        public void Info(string message)
        {
            this.mLog4NetLog.Info(message);
        }

        public void Info(string formatStr, params object[] argsmessage)
        {
            this.Info(this.GetFinalMessage(formatStr, argsmessage));
        }

        public void Warn(string message)
        {
            this.mLog4NetLog.Warn(message);
        }

        public void Warn(string formatStr, params object[] argsmessage)
        {
            this.Warn(this.GetFinalMessage(formatStr, argsmessage));
        }

        public void Error(string message)
        {
            this.mLog4NetLog.Error(message);
        }

        public void Error(string formatStr, params object[] argsmessage)
        {
            this.Error(this.GetFinalMessage(formatStr, argsmessage));
        }

        private string GetFinalMessage(string formatStr, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return formatStr;
            }
            else
            {
                return string.Format(formatStr, args);
            }
        }
    }
}
