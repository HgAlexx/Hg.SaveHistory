using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLua;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshot : IEquatable<EngineSnapshot>
    {
        #region Fields & Properties

        public int CategoryId = -1;

        public Dictionary<string, EngineSnapshotCustomValueBase> CustomValues = new Dictionary<string, EngineSnapshotCustomValueBase>();

        public string Notes;

        [JsonIgnore] public LuaFunction OnEquals = null;

        public DateTime SavedAt;

        public string SavedAtToStringFormat = "yyyy-MM-dd HH:mm:ss";
        public EngineSnapshotStatus Status = EngineSnapshotStatus.Active;

        [LuaHide] public bool Compressed { get; set; }

        [LuaHide] public bool HasScreenshot { get; set; }

        public string RelativePath { get; set; }

        [LuaHide] public string ScreenshotFilename { get; set; }

        #endregion

        #region Members

        public bool Equals(EngineSnapshot other)
        {
            if (OnEquals != null && OnEquals.Call(this, other).First() is bool value)
            {
                return value;
            }

            return other != null && SavedAt == other.SavedAt;
        }

        public EngineSnapshotCustomValueBase CustomValueByKey(string key)
        {
            if (CustomValues.ContainsKey(key))
            {
                return CustomValues[key];
            }

            return null;
        }

        #endregion
    }
}