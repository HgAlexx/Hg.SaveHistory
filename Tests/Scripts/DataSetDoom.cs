using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Tests.Scripts
{
    public class DataSetDoom : DataSet
    {
        #region Fields & Properties

        public Dictionary<int, DataSetWatcherDoom> Watchers { get; set; }

        #endregion

        #region Members

        public static IEnumerable<DataSetDoom> DataSets()
        {
            string rand = DateTime.Now.ToString("HH-mm-ss-fff");
            string data = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "DOOM2016");
            string path = Path.Combine(data, "Simulation", rand);

            var set = new DataSetDoom
            {
                DataRoot = data,
                SourceFolder = path,
                Name = @"DOOM2016",
                Title = @"DOOM 2016",
                Author = @"HgAlexx",
                FileCount = 4,
                Settings = new Dictionary<string, object>
                {
                    {"SourceFolder", path},
                    {"SlotIndex", 1}
                },
                CanAutoDetect = true,
                ProfileName = "DOOM2016",
                SuggestProfileName = "DOOM 2016 - Slot 1",
                Watchers = new Dictionary<int, DataSetWatcherDoom>()
            };

            var watcher = new DataSetWatcherDoom()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 19, 03, 24, 03),
                SnapshotDifficulty = "I'm too young to die",
                SnapshotMapDesc = "The UAC",
                SnapshotMapName = "game/sp/intro/intro",
                SnapshotPlayedTime = "0:02:46",
                SnapshotDeath = ""
            };
            watcher.Files = new List<string>
            {
                "checkpoint.dat",
                "checkpoint_mapstart.dat",
                "game_duration.dat",
                "checkpoint_alt.dat",
                "game.details",
                "game.details.verify"
            };
            set.Watchers.Add(1, watcher);

            watcher = new DataSetWatcherDoom()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 19, 03, 24, 03),
                SnapshotDifficulty = "I'm too young to die",
                SnapshotMapDesc = "The UAC",
                SnapshotMapName = "game/sp/intro/intro",
                SnapshotPlayedTime = "0:02:46",
                SnapshotDeath = "🕱"
            };
            watcher.Files = new List<string>
            {
                "game_duration.dat",
                "checkpoint_alt.dat",
                "game.details",
                "game.details.verify"
            };
            set.Watchers.Add(2, watcher);

            yield return set;


            data = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "DOOMEternal", @"Steam");
            path = Path.Combine(data, "Simulation", rand, "userdata", "13371337", "782330");
            set = new DataSetDoom
            {
                DataRoot = data,
                SourceFolder = path,
                Name = @"DOOMEternal",
                Title = @"DOOM Eternal",
                Author = @"HgAlexx",
                FileCount = 4,
                Settings = new Dictionary<string, object>
                {
                    {"Platform", 1},
                    {"SourceFolder", path},
                    {"SlotIndex", 1}
                },
                CanAutoDetect = true,
                ProfileName = "DOOMEternal_Steam",
                SuggestProfileName = "DOOM Eternal - Slot 1",
                Watchers = new Dictionary<int, DataSetWatcherDoom>()
            };

            watcher = new DataSetWatcherDoom()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 27, 01, 16, 44, 948),
                SnapshotDifficulty = "Hurt me plenty",
                SnapshotMapDesc = "Cultist Base",
                SnapshotMapName = "game/sp/e1m3_cult/e1m3_cult",
                SnapshotPlayedTime = "1:40:32",
                SnapshotDeath = "🕱"
            };
            watcher.Files = new List<string>
            {
                "game.details",
            };
            set.Watchers.Add(1, watcher);

            yield return set;


            data = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "DOOMEternal", @"Bethesda");
            path = Path.Combine(data, "Simulation", rand, "DOOMEternal", "base", "savegame", "d8d67efe-5bae-453a-aad5-08a71028d4dd");
            set = new DataSetDoom
            {
                DataRoot = data,
                SourceFolder = path,
                Name = @"DOOMEternal",
                Title = @"DOOM Eternal",
                Author = @"HgAlexx",
                FileCount = 4,
                Settings = new Dictionary<string, object>
                {
                    {"Platform", 2},
                    {"SourceFolder", path},
                    {"SlotIndex", 1}
                },
                CanAutoDetect = true,
                ProfileName = "DOOMEternal_Bethesda",
                SuggestProfileName = "DOOM Eternal - Slot 1",
                Watchers = new Dictionary<int, DataSetWatcherDoom>()
            };

            watcher = new DataSetWatcherDoom()
            {
                SnapshotSaveAt = new DateTime(2020, 06, 27, 0, 44, 12, 444),
                SnapshotDifficulty = "Hurt me plenty",
                SnapshotMapDesc = "Cultist Base",
                SnapshotMapName = "game/sp/e1m3_cult/e1m3_cult",
                SnapshotPlayedTime = "1:40:32",
                SnapshotDeath = "🕱"
            };
            watcher.Files = new List<string>
            {
                "game.details",
            };
            set.Watchers.Add(1, watcher);

            yield return set;
        }

        #endregion
    }
}