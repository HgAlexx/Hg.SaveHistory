using NLua;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshotColumnDefinition
    {
        #region Fields & Properties

        public string HeaderText;
        public string Key;
        public int Order;

        public LuaFunction Sort;

        #endregion
    }
}