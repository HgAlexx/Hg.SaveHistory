using System;
using System.IO;
using System.Windows.Forms;
using Hg.SaveHistory.Forms;
using Hg.SaveHistory.Utilities;

namespace Hg.SaveHistory
{
    internal static class Program
    {
        #region Members

        [STAThread]
        private static void Main()
        {
            Logger.ExceptionMode = LogMode.Release;
            Logger.Level = LogLevel.Information;
#if DEBUG
            Logger.ExceptionMode = LogMode.Debug;
            Logger.Level = LogLevel.Debug;
#endif
            Logger.LogName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            Logger.FilePath = Path.GetDirectoryName(Application.ExecutablePath);

            // Exception Handlers
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.CurrentDomain_UnhandledException;
            Application.ThreadException += ExceptionHandler.Application_ThreadException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }

        #endregion
    }
}