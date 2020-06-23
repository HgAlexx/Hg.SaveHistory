using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hg.SaveHistory.API
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionSource
    {
        Button, // manual user click on the main form buttons, game mostly not running or not focus
        HotKey, // manual user event, game mostly running and focus
        AutoBackup // automatic event, game running and surely focus
    }
}