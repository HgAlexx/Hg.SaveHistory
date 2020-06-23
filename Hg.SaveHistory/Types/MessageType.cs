using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hg.SaveHistory.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageType
    {
        None,
        Information,
        Question,
        Warning,
        Error
    }
}