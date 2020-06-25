using System;
using Microsoft.Win32;

namespace Hg.SaveHistory.API
{
    public static class HgSteamHelper
    {
        #region Members

        public static string SteamId3ToSteamId64(string id)
        {
            try
            {
                if (ulong.TryParse(id, out var l))
                {
                    return (l + 76561197960265728UL).ToString();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            return null;
        }


        public static string SteamInstallPath()
        {
            try
            {
                var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam");
                if (registryKey != null)
                {
                    if (registryKey.GetValue("InstallPath") is string path)
                    {
                        return path;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                Utilities.Logger.LogException(exception);
            }

            return null;
        }

        #endregion
    }
}