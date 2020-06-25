using System.Linq;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCustomValueBoolean : EngineSnapshotCustomValue<bool>
    {
        #region Members

        public override int CompareTo(EngineSnapshotCustomValueBase customValue)
        {
            if (customValue is EngineSnapshotCustomValueBoolean c)
            {
                return Value.CompareTo(c.Value);
            }

            return 0;
        }

        public override string ToString()
        {
            if (OnToString != null)
            {
                if (OnToString.Call(Value).First() is string value)
                {
                    return value;
                }
            }

            return Value ? "Yes" : "No";
        }

        #endregion
    }
}