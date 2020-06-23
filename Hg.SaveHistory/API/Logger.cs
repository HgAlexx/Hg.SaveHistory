using HostLogger = Hg.SaveHistory.Utilities.Logger;

namespace Hg.SaveHistory.API
{
    public class Logger
    {
        #region Members

        public static void Debug(params object[] objects)
        {
            HostLogger.Debug(objects);
        }

        public static void Error(params object[] objects)
        {
            HostLogger.Error(objects);
        }

        public static void Information(params object[] objects)
        {
            HostLogger.Information(objects);
        }

        public static void Warning(params object[] objects)
        {
            HostLogger.Warning(objects);
        }

        #endregion
    }
}