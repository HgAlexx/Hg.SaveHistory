using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static void LogException(Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            switch (ExceptionMode)
            {
                case LogMode.Debug:
                    LogExceptionDebug(exception, memberName, sourceFilePath, sourceLineNumber);
                    break;
                case LogMode.Release:
                    LogExceptionDebug(exception, memberName, sourceFilePath, sourceLineNumber);
                    LogExceptionDialog(exception);
                    break;
            }
        }

        private static void LogExceptionDebug(Exception exception, string memberName = "", string sourceFilePath = "",
            int sourceLineNumber = 0)
        {
            System.Diagnostics.Debug.WriteLine("");
            if (!string.IsNullOrEmpty(sourceFilePath))
            {
                System.Diagnostics.Debug.WriteLine("File: " + sourceFilePath);
            }

            if (!string.IsNullOrEmpty(memberName))
            {
                System.Diagnostics.Debug.WriteLine("Function: " + memberName);
            }

            if (sourceLineNumber > 0)
            {
                System.Diagnostics.Debug.WriteLine("Line: " + sourceLineNumber);
            }

            System.Diagnostics.Debug.WriteLine(exception.ToString(), "Exception");
            System.Diagnostics.Debug.WriteLine("");

            if (!string.IsNullOrEmpty(sourceFilePath))
            {
                Log("File: " + sourceFilePath, LogLevel.Error);
            }

            if (!string.IsNullOrEmpty(memberName))
            {
                Log("Function: " + memberName, LogLevel.Error);
            }

            if (sourceLineNumber > 0)
            {
                Log("Line: " + sourceLineNumber, LogLevel.Error);
            }

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