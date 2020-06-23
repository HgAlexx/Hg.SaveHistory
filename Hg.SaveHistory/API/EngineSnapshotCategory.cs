using System;
using System.Linq;
using NLua;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCategory : IEquatable<EngineSnapshotCategory>
    {
        #region Fields & Properties

        // Main properties
        public int Id = -1;

        // Some other properties for more flexibility
        public int Level;
        public string Name;
        public string NameInternal;
        public string NameSafe;

        public LuaFunction OnEquals = null;
        public LuaFunction OnToString = null;

        #endregion

        #region Members

        public bool Equals(EngineSnapshotCategory other)
        {
            if (OnEquals != null && OnEquals.Call(this, other).First() is bool value)
            {
                return value;
            }

            return other != null && Id == other.Id;
        }

        public override string ToString()
        {
            if (OnToString != null && OnToString.Call(this).First() is string value)
            {
                return value;
            }

            return Name;
        }

        #endregion
    }
}