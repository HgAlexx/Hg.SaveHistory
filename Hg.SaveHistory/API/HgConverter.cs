using System;

namespace Hg.SaveHistory.API
{
    public static class HgConverter
    {
        #region Members

        public static long DateTimeToUnix(DateTime datetime)
        {
            DateTimeOffset dateTimeOffset = datetime;
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        public static DateTime UnixToDateTime(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
        }

        #endregion
    }
}