using System;
using System.Collections.Generic;

namespace Tests.Scripts
{
    public class DataSetWatcherDoom
    {
        public List<string> Files;
        public string SnapshotDeath { get; set; }
        public string SnapshotDifficulty { get; set; }
        public string SnapshotMapDesc { get; set; }
        public string SnapshotMapName { get; set; }
        public string SnapshotPlayedTime { get; set; }
        public DateTime SnapshotSaveAt { get; set; }
    }
}