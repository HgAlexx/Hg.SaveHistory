using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Hg.SaveHistory.API
{
    public static class HgUtility
    {
        #region Members

        public static bool IsValidFileName(string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (filename.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidPath(string path)
        {
            foreach (char c in Path.GetInvalidPathChars())
            {
                if (path.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static string SafeFileName(string filename)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, ' ');
            }

            return filename;
        }

        public static string SafePath(string path)
        {
            foreach (char c in Path.GetInvalidPathChars())
            {
                path = path.Replace(c, ' ');
            }

            return path;
        }

        public static void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        public static bool StringEndsWith(string input, string value)
        {
            return input.EndsWith(value);
        }

        public static string[] StringSplit(string input, string token, StringSplitOptions options)
        {
            return input.Split(token.ToCharArray(), options);
        }

        public static bool StringStartsWith(string input, string value)
        {
            return input.StartsWith(value);
        }

        public static string StringTrim(string input)
        {
            return input.Trim();
        }

        #endregion
    }
}