using System.Linq;
using HostLogger = Hg.SaveHistory.Utilities.Logger;

namespace Hg.SaveHistory.API
{
    public class Logger
    {
        #region Members

        public static void Debug(params object[] objects)
        {
            HostLogger.Debug(new object[] { "Script: " }.Concat(objects).ToArray());
        }

        public static void Error(params object[] objects)
        {
            HostLogger.Error(new object[] { "Script: " }.Concat(objects).ToArray());
        }

        public static void Information(params object[] objects)
        {
            HostLogger.Information(new object[] { "Script: " }.Concat(objects).ToArray());
        }

        public static void Warning(params object[] objects)
        {
            HostLogger.Warning(new object[] { "Script: " }.Concat(objects).ToArray());
        }

        #endregion
    }
}