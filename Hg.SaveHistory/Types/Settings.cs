using System.Collections.Generic;
using System.Drawing;

namespace Hg.SaveHistory.Types
{
    public class Settings
    {
        #region Fields & Properties

        public MessageMode NotificationMode;

        public bool AutoBackupSoundNotification { get; set; }

        public bool AutoSelectLastSnapshot { get; set; }

        public bool HighlightSelectedSnapshot { get; set; }

        public Color HighlightSelectedSnapshotColor { get; set; }

        public bool HotKeysActive { get; set; }

        public bool HotKeysSound { get; set; }

        public List<HotKeyToAction> HotKeyToActions { get; set; }

        public List<string> PinnedProfiles { get; set; }

        public List<string> RecentProfiles { get; set; }

        public ScreenshotQuality ScreenshotQuality { get; set; }

        #endregion

        #region Members

        public Settings()
        {
            HotKeyToActions = new List<HotKeyToAction>();

            AutoBackupSoundNotification = false;
            AutoSelectLastSnapshot = true;

            HighlightSelectedSnapshot = true;
            HighlightSelectedSnapshotColor = Color.DeepSkyBlue;

            HotKeysActive = false;
            HotKeysSound = false;

            NotificationMode = MessageMode.Status;

            ScreenshotQuality = ScreenshotQuality.Png;

            RecentProfiles = new List<string>();
            PinnedProfiles = new List<string>();
        }

        #endregion
    }
}