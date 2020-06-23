namespace Hg.SaveHistory.API
{
    public class EngineSnapshotCustomValueString : EngineSnapshotCustomValue<string>
    {
        #region Members

        public override int CompareTo(EngineSnapshotCustomValueBase customValue)
        {
            if (customValue is EngineSnapshotCustomValueString c)
            {
                return string.CompareOrdinal(Value, c.Value);
            }

            return 0;
        }

        #endregion
    }
}