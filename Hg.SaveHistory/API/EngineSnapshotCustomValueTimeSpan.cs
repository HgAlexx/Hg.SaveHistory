using System;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCustomValueTimeSpan : EngineSnapshotCustomValue<TimeSpan>
    {
        #region Members

        public EngineSnapshotCustomValueTimeSpan()
        {
            ToStringFormat = "g";
        }

        public override int CompareTo(EngineSnapshotCustomValueBase customValue)
        {
            if (customValue is EngineSnapshotCustomValueTimeSpan c)
            {
                return Value.CompareTo(c.Value);
            }

            return 0;
        }

        public override string ToString()
        {
            if (ToStringFormat != null)
            {
                return Value.ToString(ToStringFormat);
            }

            return base.ToString();
        }

        #endregion
    }
}