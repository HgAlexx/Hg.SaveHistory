using Newtonsoft.Json;
using NLua;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCustomValueBase
    {
        #region Fields & Properties

        public string Caption = "";

        [JsonIgnore]
        public LuaFunction OnToString = null;

        public bool ShowInDetails = false;

        [JsonIgnore]
        public string ToStringFormat;

        #endregion

        #region Members

        public virtual int CompareTo(EngineSnapshotCustomValueBase customValue)
        {
            return string.CompareOrdinal(Caption, customValue.Caption);
        }

        #endregion
    }
}