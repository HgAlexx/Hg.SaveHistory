using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hg.SaveHistory.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HotKeyAction
    {
        CategoryPrevious,
        CategoryNext,

        SnapshotFirst,
        SnapshotLast,
        SnapshotPrevious,
        SnapshotNext,
        SnapshotRestore,
        SnapshotDelete,
        SnapshotBackup,

        SettingSwitchAutoBackup
    }
}