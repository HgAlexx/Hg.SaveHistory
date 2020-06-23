using System.Collections.Generic;

namespace Hg.SaveHistory.API
{
    public class EngineSettingCombobox : EngineSetting
    {
        #region Fields & Properties

        public int Value = -1;
        public Dictionary<int, string> Values = new Dictionary<int, string>();

        #endregion
    }
}