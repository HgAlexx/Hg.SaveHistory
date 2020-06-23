//
// File imported from my old Hg.Common project
//

using System;
using System.Threading;

namespace Hg.SaveHistory.Utilities
{
    public class ExceptionHandler
    {
        #region Members

        public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Logger.LogException(ex);
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception) e.ExceptionObject;
            Logger.LogException(ex);
        }

        #endregion
    }
}