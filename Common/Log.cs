using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Fuzzware.Common
{
    public class LoggedException : Exception
    {
        public LoggedException(string Message) : base(Message)
        {
            
        }
    }

    public class LogEventArgs : EventArgs
    {
        string sMessage;
        Log.LogType eLogType;

        public LogEventArgs(String LogMessage, Log.LogType LogType)
        {
            sMessage = LogMessage;
            eLogType = LogType;
        }
        public string Message
        {
            get { return sMessage; }
        }
        public Log.LogType LogType
        {
            get { return eLogType; }
        }
    }

    public static class Log
    {
        private static String CurrentDate;
        //private static String LogFileDirectory;
        private static String LogFileName;
        private static object lockobj = new object();
        private static Stream oUserDefinedStream;

        public delegate void LogEventHandler(object sender, LogEventArgs a);

        /// <summary>
        /// Gets called when a message is written to the log.
        /// </summary>
        public static event LogEventHandler LogEvent;

        public enum LogType
        {
            LogOnlyInfo,
            Status,
            Info,
            Warning,
            Error,
            UnexpectedError
        }

        public static void SetOutputStream(Stream oStream)
        {
            oUserDefinedStream = oStream;
        }

        private static Stream GetOutputStream()
        {
            if (null != oUserDefinedStream)
                return oUserDefinedStream;
            return new FileStream(LogFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }

        private static void CloseStream(Stream oStream)
        {
            // Only close the stream if its not the user one
            if (null == oUserDefinedStream)
            {
                oStream.Flush();
                oStream.Close();
            }
        }

        //public static string LogDirectory
        //{
        //    set 
        //    {
        //        // Make sure there are no invalid path characters
        //        if ((null != value) && (-1 == value.IndexOfAny(Path.GetInvalidPathChars())))
        //        {
        //            LogFileDirectory = value;
        //            // Need to make sure the directory exists
        //            if (!Directory.Exists(LogFileDirectory))
        //                Directory.CreateDirectory(LogFileDirectory);
        //        }
        //    }
        //}

        public static void Write(String Title, String Message, LogType Type)
        {
            String Date = DateTime.Now.Year + "." + DateTime.Now.Month.ToString("00") + "." + DateTime.Now.Day.ToString("00");
            String Time = DateTime.Now.Hour.ToString("00") + "." + DateTime.Now.Minute.ToString("00") + "." + DateTime.Now.Second.ToString("00");
            if (String.IsNullOrEmpty(LogFileName))
            {
                // Create the Log File
                LogFileName = "Log-" + Date + " " + Time + ".txt";
                //if (!String.IsNullOrEmpty(LogFileDirectory))
                //    LogFileName = Path.Combine(LogFileDirectory, LogFileName);
            }
            if (String.IsNullOrEmpty(CurrentDate))
                CurrentDate = Date;

            String LogMessage = "";

            if (!CurrentDate.Equals(Date, StringComparison.CurrentCultureIgnoreCase))
            {
                LogMessage += (Time + " Info - " + Date + Environment.NewLine);
                CurrentDate = Date;
            }

            // Write the current time
            LogMessage += (Time + " ");
            // Write the type of message
            bool Unexpected = false;
            bool Expected = false;
            // Colour the output
            ConsoleColor eConsoleColor = Console.ForegroundColor;
            switch (Type)
            {
                case LogType.UnexpectedError:
                    LogMessage += ("UNEXPECTED ERROR - ");
                    Unexpected = true;
                    eConsoleColor = ConsoleColor.Red;
                    break;
                case LogType.Error:
                    LogMessage += ("ERROR - ");
                    Expected = true;
                    eConsoleColor = ConsoleColor.Red;
                    break;
                case LogType.Info:
                case LogType.LogOnlyInfo:
                    LogMessage += "Info - ";
                    break;
                case LogType.Warning:
                    LogMessage += "Warning - ";
                    eConsoleColor = ConsoleColor.Yellow;
                    break;
                case LogType.Status:
                    LogMessage += "Status - ";
                    eConsoleColor = ConsoleColor.Blue;
                    break;
            }


            lock (lockobj)
            {
                try
                {
                    if (Type != LogType.LogOnlyInfo)
                    {
                        ConsoleColor eCurrentConsoleColour = Console.ForegroundColor;
                        if (Console.BackgroundColor != eConsoleColor)
                            Console.ForegroundColor = eConsoleColor;
                        
                        Console.WriteLine(LogMessage + Message);

                        Console.ForegroundColor = eCurrentConsoleColour;
                    }

                    if (null != LogEvent)
                    {
                        LogEvent(null, new LogEventArgs(LogMessage + Message, Type));
                    }
                    else
                    {
                        // For using to work the encapsulated object needs to support IDisposable which FileStream does, but StreamWriter doesn't
                        using (FileStream fs = new FileStream(LogFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        //using (StreamWriter sw = File.AppendText(LogFileName))
                        //using (StreamWriter sw = new StreamWriter(new FileStream(LogFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                        {
                            StreamWriter sw = new StreamWriter(fs);
#if DEBUG
                            sw.WriteLine(LogMessage + Title + " - " + Message);
#else
                            sw.WriteLine(LogMessage + " - " + Message);
#endif
                            sw.Flush();
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            if(Unexpected)
                throw new LoggedException("Logged unhandled or unexpected error.");
            if (Expected)
                throw new LoggedException("Logged unrecoverable error.");
        }

        public static void Write(String Message, Byte[] Bytes, LogType Type)
        {
            StringBuilder ByteMessage = new StringBuilder();
            int BytesToShow = 16;

            for (int i = 0; i < Bytes.Length; i += BytesToShow)
            {
                ByteMessage.AppendFormat("{0,8:X8}\t", i);
                for (int j = 0; j < BytesToShow; j++)
                {
                    if ((i + j) < Bytes.Length)
                        ByteMessage.AppendFormat("{0,2:X2} ", Bytes[i + j]);
                    else
                        ByteMessage.AppendFormat("   ");
                }
                ByteMessage.Append("\t");
                for (int j = 0; (j < BytesToShow) && ((i + j) < Bytes.Length); j++)
                {
                    if ((Bytes[i + j] > 31) && (Bytes[i + j] < 127))
                        ByteMessage.AppendFormat("{0,1:c}", System.Convert.ToChar(Bytes[i + j]));
                    else
                        ByteMessage.Append(".");
                }
                ByteMessage.Append("\n");
            }

            Log.Write(Message, "\n" + ByteMessage.ToString(), Type);
        }

        public static void Write(Exception e)
        {
            Log.Write(e.Source, e.ToString(), LogType.UnexpectedError);
        }

        public static void Write(MethodBase Method, String Message, LogType Type)
        {
            Log.Write(Method.DeclaringType + "." + Method.Name, Message, Type);
        }
    }
}
