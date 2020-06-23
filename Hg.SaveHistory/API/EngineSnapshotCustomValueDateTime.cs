using System;

namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCustomValueDateTime : EngineSnapshotCustomValue<DateTime>
    {
        #region Members

        public EngineSnapshotCustomValueDateTime()
        {
            ToStringFormat = "yyyy-MM-dd HH:mm:ss";
        }

        public override int CompareTo(EngineSnapshotCustomValueBase customValue)
        {
            if (customValue is EngineSnapshotCustomValueDateTime c)
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