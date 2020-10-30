using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Hg.SaveHistory.API
{
    public static class HgUtility
    {
        #region Members

        public static string HashFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);

                    SHA256 sha256 = SHA256.Create();
                    StringBuilder builder = new StringBuilder();
                    using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Position = 0;
                        byte[] hashValue = sha256.ComputeHash(fileStream);

                        foreach (var t in hashValue)
                        {
                            builder.Append(t.ToString("x2"));
                        }
                    }

                    // ReSharper disable once RedundantAssignment
                    fileInfo = null;

                    return builder.ToString();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            return null;
        }

        public static string HashString(string input)
        {
            try
            {
                if (input != null)
                {
                    SHA256 sha256 = SHA256.Create();
                    StringBuilder builder = new StringBuilder();

                    byte[] hashValue = sha256.ComputeHash(Encoding.Default.GetBytes(input));

                    foreach (var t in hashValue)
                    {
                        builder.Append(t.ToString("x2"));
                    }

                    return builder.ToString();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            return null;
        }

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