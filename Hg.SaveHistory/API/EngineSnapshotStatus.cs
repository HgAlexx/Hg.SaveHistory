using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hg.SaveHistory.API
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EngineSnapshotStatus
    {
        Active,
        Archived,
        Deleted,
        Nuked
    }
}