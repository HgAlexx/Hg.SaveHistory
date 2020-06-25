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

        public static Guid? StringToGuid(string guid)
        {
            try
            {
                if (Guid.TryParse(guid, out var g))
                {
                    return g;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            return null;
        }

        public static DateTime UnixToDateTime(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
        }

        #endregion
    }
}