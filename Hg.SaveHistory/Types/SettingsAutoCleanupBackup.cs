using System;

namespace Hg.SaveHistory.Types
{
    public class SettingsAutoCleanupBackup
    {
        #region Fields & Properties

        public TimeSpan? Age { get; set; }

        public int? Count { get; set; }
        public bool Enabled { get; set; }

        public AutoCleanupMode Modes { get; set; }

        public bool PerCategory { get; set; }

        public long? Size { get; set; }

        public long? TotalSize { get; set; }

        #endregion

        #region Members

        public SettingsAutoCleanupBackup()
        {
            Enabled = false;
            PerCategory = true;
            Modes = AutoCleanupMode.ByTotalSize | AutoCleanupMode.ByCount;
            Age = null;
            Count = 50;
            Size = null;
            TotalSize = 1_000 * 1_024 * 1_024; // 1 000 Mo
        }

        #endregion
    }
}