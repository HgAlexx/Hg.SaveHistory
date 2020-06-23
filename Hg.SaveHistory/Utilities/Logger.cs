using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hg.SaveHistory.Forms;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Utilities
{
    public delegate void LogEvent();

    public class Logger
    {
        #region

        public static LogMode ExceptionMode = LogMode.Debug;
        public static LogLevel Level = LogLevel.Error;
        public static string LogName = "appName";
        public static string FilePath = ".";

        private static readonly List<string> LogEntries = new List<string>();

        private static readonly object LogEntriesLock = new object();
        private static FormException _formException;

        private static string _fullPath;

        private static string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(_fullPath))
                {
                    _fullPath = Path.Combine(FilePath, LogName + ".log");
                }

                return _fullPath;
            }
        }

        #endregion

        #region

        public static void ClearLogs()
        {
            lock (LogEntriesLock)
            {
                LogEntries.Clear();
            }

            OnLog?.Invoke();
        }

        public static List<string> GetLogs()
        {
            lock (LogEntriesLock)
            {
                return LogEntries.ToList();
            }
        }

        private static string BuildString(params object[] objects)
        {
            string message = "";

            foreach (var o in objects)
            {
                if (o == null)
                {
                    message += "null";
                }
                else
                {
                    message += o.ToString();
                }
            }

            return message;
        }

        public static void Debug(params object[] objects)
        {
            Log(BuildString(objects), LogLevel.Debug);
        }

        public static void Error(params object[] objects)
        {
            Log(BuildString(objects), LogLevel.Error);
        }

        public static void Warning(params object[] objects)
        {
            Log(BuildString(objects), LogLevel.Warning);
        }

        public static void Information(params object[] objects)
        {
            Log(BuildString(objects), LogLevel.Information);
        }

        public static void Log(string message, LogLevel level)
        {
            string fullMessage = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff] ") + level + ": " + message;
            System.Diagnostics.Debug.WriteLine(fullMessage);

            if (level <= Level)
            {
                File.AppendAllText(FullPath, fullMessage + Environment.NewLine);

                lock (LogEntriesLock)
                {
                    LogEntries.Add(fullMessage);
                }

                OnLog?.Invoke();
            }
        }

        public static void LogException(Exception exception)
        {
            switch (ExceptionMode)
            {
                case LogMode.Debug:
                    LogExceptionDebug(exception);
                    break;
                case LogMode.Release:
                    LogExceptionDebug(exception);
                    LogExceptionDialog(exception);
                    break;
            }
        }

        private static void LogExceptionDebug(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine(exception.ToString(), "Exception");
            System.Diagnostics.Debug.WriteLine("");

            Log(exception.ToString(), LogLevel.Error);

            if (exception.InnerException != null)
            {
                LogExceptionDebug(exception.InnerException);
            }
        }

        private static void LogExceptionDialog(Exception exception)
        {
            var content = exception.ToString();

            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                content += Environment.NewLine;
                content += exception.ToString();
            }

            if (_formException == null)
            {
                _formException = new FormException();
            }

            _formException.ErrorDetails.Add(new Error {Title = exception.Message, Content = content});
            _formException.LoadCombobox();

            if (!_formException.Visible)
            {
                _formException.Show();
            }
        }

        public static event LogEvent OnLog;

        #endregion
    }
}