using NLua;

namespace Hg.SaveHistory.API
{
    public class EngineSettingFolderBrowser : EngineSetting
    {
        #region Fields & Properties

        public bool CanAutoDetect = false;
        public LuaFunction OnAutoDetect = null;
        public string Value = "";

        #endregion
    }
}