using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hg.SaveHistory.Utilities
{
    public static class DirectoryInfoExtension
    {
        #region Members

        public static long GetDirectorySize(this DirectoryInfo directoryInfo, bool recursive = true)
        {
            long directorySize = 0;
            if (directoryInfo == null || !directoryInfo.Exists)
            {
                return directorySize;
            }

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                Interlocked.Add(ref directorySize, fileInfo.Length);
            }

            if (recursive)
            {
                Parallel.ForEach(directoryInfo.GetDirectories(), subDirectory =>
                    Interlocked.Add(ref directorySize, GetDirectorySize(subDirectory)));
            }

            return directorySize;
        }

        #endregion
    }
}