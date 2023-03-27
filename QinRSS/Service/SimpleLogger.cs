using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QinRSS.Service
{
    public class SimpleLogger
    {
        private const string FILE_EXT = ".log";
        private readonly object fileLock = new object();
        private readonly string datetimeFormat;
        private readonly string logFilename;


        // 使用 Lazy<T> 类型实现延迟初始化
        private static readonly Lazy<SimpleLogger> lazy =
            new Lazy<SimpleLogger>(() => new SimpleLogger());


        // 公开静态属性，返回单例对象
        public static SimpleLogger Instance => lazy.Value;

        /// <summary>
        /// Initiate an instance of SimpleLogger class constructor.
        /// If log file does not exist, it will be created automatically.
        /// </summary>
        private SimpleLogger()
        {
            datetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            logFilename = Path.Combine(AppContext.BaseDirectory, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + FILE_EXT);

            // Log file header line
            string logHeader = logFilename + " is created.";
            if (!System.IO.File.Exists(logFilename))
            {
                WriteLine(System.DateTime.Now.ToString(datetimeFormat) + " " + logHeader);
            }
        }

        /// <summary>
        /// Log a DEBUG message
        /// </summary>
        /// <param name="text">Message</param>
        public void Debug(string text)
        {
            WriteFormattedLog(LogLevel.DEBUG, text);
        }

        /// <summary>
        /// Log an ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        /// <summary>
        /// Log a FATAL ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Fatal(string text)
        {
            WriteFormattedLog(LogLevel.FATAL, text);
        }

        /// <summary>
        /// Log an INFO message
        /// </summary>
        /// <param name="text">Message</param>
        public void Info(string text)
        {
            WriteFormattedLog(LogLevel.INFO, text);
        }

        public void Info(string text, params object[] args)
        {
            WriteFormattedLog(LogLevel.INFO, string.Format(text, args));
        }

        /// <summary>
        /// Log a TRACE message
        /// </summary>
        /// <param name="text">Message</param>
        public void Trace(string text)
        {
            WriteFormattedLog(LogLevel.TRACE, text);
        }

        /// <summary>
        /// Log a WARNING message
        /// </summary>
        /// <param name="text">Message</param>
        public void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        private void WriteLine(string text, bool append = false)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                Console.WriteLine(text);

                lock (fileLock)
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine(text);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void WriteFormattedLog(LogLevel level, string text)
        {
            string pretext;
            switch (level)
            {
                case LogLevel.TRACE:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [TRACE]";
                    break;
                case LogLevel.INFO:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [INFO]";
                    break;
                case LogLevel.DEBUG:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [DEBUG]";
                    break;
                case LogLevel.WARNING:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [WARNING]";
                    break;
                case LogLevel.ERROR:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [ERROR]";
                    break;
                case LogLevel.FATAL:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [FATAL]";
                    break;
                default:
                    pretext = "";
                    break;
            }

            WriteLine(pretext + text, true);
        }

        [System.Flags]
        private enum LogLevel
        {
            TRACE,
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }
    }
}
