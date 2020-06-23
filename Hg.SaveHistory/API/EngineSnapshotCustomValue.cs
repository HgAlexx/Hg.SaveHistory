using System.Collections.Generic;
using System.Linq;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCustomValue<T> : EngineSnapshotCustomValueBase
    {
        #region Fields & Properties

        public T Value;

        #endregion

        #region Members

        public override int CompareTo(EngineSnapshotCustomValueBase customValue)
        {
            if (customValue is EngineSnapshotCustomValue<T> c)
            {
                return Comparer<T>.Default.Compare(Value, c.Value);
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

            return Value.ToString();
        }

        #endregion
    }
}