using System;
using System.Collections.Generic;
using System.Globalization;
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

        [JsonIgnore]
        public LuaFunction OnEquals = null;

        public DateTime SavedAt;

        public string SavedAtToStringFormat = "yyyy-MM-dd HH:mm:ss";
        public EngineSnapshotStatus Status = EngineSnapshotStatus.Active;

        [LuaHide]
        public bool Compressed { get; set; }

        [LuaHide]
        public bool HasScreenshot { get; set; }

        public string RelativePath { get; set; }

        [LuaHide]
        public string ScreenshotFilename { get; set; }

        [JsonIgnore]
        [LuaHide]
        public long Size { get; set; } = 0;

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
            return CustomValues.TryGetValue(key, out var value) ? value : null;
        }

        public override string ToString()
        {
            string value = "";
            if (!string.IsNullOrEmpty(SavedAtToStringFormat))
            {
                value += SavedAt.ToString(SavedAtToStringFormat);
            }
            else
            {
                value += SavedAt.ToString(CultureInfo.InvariantCulture);
            }

            return value;
        }

        #endregion
    }
}