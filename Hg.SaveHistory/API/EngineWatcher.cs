using System.IO;
using NLua;

namespace Hg.SaveHistory.API
{
    public class EngineWatcher
    {
        #region Fields & Properties

        public string Filter;
        public int NotifyFilter;

        public LuaFunction OnEvent;
        public string Path;
        public bool WatchChanged;

        public bool WatchCreated;
        public bool WatchDeleted;

        public bool WatchParent;
        public bool WatchRenamed;

        #endregion

        #region Members

        public EngineWatcher()
        {
            WatchCreated = false;
            WatchDeleted = false;
            WatchChanged = false;
            WatchRenamed = false;
            Filter = "*";
            NotifyFilter = (int) (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite);
            WatchParent = true;
            OnEvent = null;
            Path = null;
        }

        ~EngineWatcher()
        {
            OnEvent = null;
        }

        #endregion
    }
}