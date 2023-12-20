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

        public string LastUsedProfilePath { get; set; }

        public Point? Location { get; set; }

        public bool MinimizedToTray { get; set; }

        public bool ShowTrayNotification { get; set; }

        public bool OpenLastUsedProfileOnStartup { get; set; }

        public List<string> PinnedProfiles { get; set; }

        public List<string> RecentProfiles { get; set; }

        public bool SaveSizeAndPosition { get; set; }

        public ScreenshotQuality ScreenshotQuality { get; set; }

        public Size? Size { get; set; }

        public bool SnapToScreenEdges { get; set; }

        public bool StartMinimized { get; set; }

        public bool StartWithWindows { get; set; }

        #endregion

        #region Members

        public Settings()
        {
            HotKeyToActions = new List<HotKeyToAction>();
            Location = null;
            Size = null;

            AutoBackupSoundNotification = false;
            AutoSelectLastSnapshot = true;

            HighlightSelectedSnapshot = true;
            HighlightSelectedSnapshotColor = Color.DeepSkyBlue;

            HotKeysActive = false;
            HotKeysSound = false;

            NotificationMode = MessageMode.Status;

            ScreenshotQuality = ScreenshotQuality.Png;

            SaveSizeAndPosition = true;
            SnapToScreenEdges = true;

            StartWithWindows = false;
            StartMinimized = false;

            MinimizedToTray = false;
            ShowTrayNotification = true;

            OpenLastUsedProfileOnStartup = false;
            LastUsedProfilePath = "";

            RecentProfiles = new List<string>();
            PinnedProfiles = new List<string>();
        }

        #endregion
    }
}