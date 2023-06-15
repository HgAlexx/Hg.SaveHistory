using System;

namespace Hg.SaveHistory.Types
{
    [Flags]
    public enum AutoCleanupMode
    {
        None,
        ByAge,
        ByCount,
        BySize,
        ByTotalSize
    }
}