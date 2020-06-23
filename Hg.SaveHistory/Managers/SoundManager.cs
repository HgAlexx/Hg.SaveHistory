using System.Media;
using Hg.SaveHistory.Properties;

namespace Hg.SaveHistory.Managers
{
    public class SoundManager
    {
        #region Fields & Properties

        private static readonly SoundPlayer SoundPlayerDummy;
        private static readonly SoundPlayer SoundPlayerError;
        private static readonly SoundPlayer SoundPlayerSuccess;

        #endregion

        #region Members

        static SoundManager()
        {
            // Only here to speed up the replay of the other SoundPlayer
            SoundPlayerDummy = new SoundPlayer(Resources.empty);
            SoundPlayerDummy.Load();
            SoundPlayerSuccess = new SoundPlayer(Resources.success);
            SoundPlayerSuccess.Load();
            SoundPlayerError = new SoundPlayer(Resources.error);
            SoundPlayerError.Load();
        }

        public static void PlayError()
        {
            SoundPlayerError.Play();
        }

        public static void PlaySuccess()
        {
            SoundPlayerSuccess.Play();
        }

        public static void PreLoad()
        {
            SoundPlayerDummy.Play();
        }

        #endregion
    }
}