using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Tests.Scripts
{
    public class DataSetWatcherSatisfactory
    {
        public List<string> Files;
        public string SnapshotAutosave { get; set; }
        public string SnapshotSaveVersion { get; set; }
        public string SnapshotBuildVersion { get; set; }
        public string SnapshotSessionName { get; set; }
        public string SnapshotPlayedTime { get; set; }
        public DateTime SnapshotSaveAt { get; set; }
    }

    public class DataSetSatisfactory : DataSet
    {
        #region Fields & Properties

        public Dictionary<int, DataSetWatcherSatisfactory> Watchers { get; set; }

        #endregion

        #region Members

        public static IEnumerable<DataSetSatisfactory> DataSets()
        {
            string rand = DateTime.Now.ToString("HH-mm-ss-fff");
            string data = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "Satisfactory");
            string path = Path.Combine(data, "Simulation", rand);

            var set = new DataSetSatisfactory
            {
                DataRoot = data,
                SourceFolder = path,
                Name = @"Satisfactory",
                Title = @"Satisfactory",
                Author = @"HgAlexx",
                FileCount = 4,
                Settings = new Dictionary<string, object>
                {
                    {"SourceFolder", path},
                },
                CanAutoDetect = true,
                ProfileName = "Satisfactory",
                SuggestProfileName = "Satisfactory",
                Watchers = new Dictionary<int, DataSetWatcherSatisfactory>()
            };

            var watcher = new DataSetWatcherSatisfactory()
            {
                // 2021-03-18 18.24.16
                SnapshotSaveAt = new DateTime(2021, 03, 18, 18, 24, 16),
                SnapshotBuildVersion = "147217",
                SnapshotSaveVersion = "25",
                SnapshotSessionName = "Test",
                SnapshotPlayedTime = "0:02:19",
                SnapshotAutosave = ""
            };

            watcher.Files = new List<string>
            {
                "Test.sav",
            };
            set.Watchers.Add(1, watcher);

            watcher = new DataSetWatcherSatisfactory()
            {
                // 2021-03-18 18.25.01
                SnapshotSaveAt = new DateTime(2021, 03, 18, 18, 25, 01),
                SnapshotBuildVersion = "147217",
                SnapshotSaveVersion = "25",
                SnapshotSessionName = "Test",
                SnapshotPlayedTime = "0:03:05",
                SnapshotAutosave = "✓"
            };

            watcher.Files = new List<string>
            {
                "Test_autosave_0.sav",
            };
            set.Watchers.Add(2, watcher);


            yield return set;
        }

        #endregion
    }
}