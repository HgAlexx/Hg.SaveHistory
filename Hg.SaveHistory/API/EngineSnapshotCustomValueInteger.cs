namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCustomValueInteger : EngineSnapshotCustomValue<int>
    {
        #region Members

        public override int CompareTo(EngineSnapshotCustomValueBase customValue)
        {
            if (customValue is EngineSnapshotCustomValueInteger c)
            {
                return Value.CompareTo(c.Value);
            }

            return 0;
        }

        #endregion
    }
}