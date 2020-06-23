using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hash
{
    internal class Program
    {
        #region Fields & Properties

        private const string CodeFile = 
@"using System.Collections.Generic;

// THIS FILE IS GENERATED: DO NOT EDIT
namespace Hg.SaveHistory
{
    public class ScriptsHash
    {
        public static List<string> Officials = new List<string>()
        {
REPLACE_NAMES
        };

        public static Dictionary<string, string> FilesHashes = new Dictionary<string, string>()
        {
REPLACE_HASHES
        };
    }
}
";

        #endregion

        #region Members

        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            DirectoryInfo sourceDirectory = new DirectoryInfo(args[0]);
            string outputFile = args[1];
            SHA256 sha256 = SHA256.Create();

            List<string> names = new List<string>();
            List<string> pairs = new List<string>();
            foreach (var directoryInfo in sourceDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                names.Add("            \"" + directoryInfo.Name + "\"");
                foreach (var fileInfo in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    string fileName = Path.Combine(directoryInfo.Name, fileInfo.Name);
                    try
                    {
                        var fileStream = fileInfo.Open(FileMode.Open);
                        fileStream.Position = 0;
                        var hashValue = sha256.ComputeHash(fileStream);
                        var builder = new StringBuilder();
                        foreach (var t in hashValue)
                        {
                            builder.Append(t.ToString("x2"));
                        }

                        pairs.Add("            {\"" + fileName.Replace("\\", "\\\\") + "\",\"" + builder + "\"}");
                        fileStream.Close();
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"I/O Exception: {e.Message}");
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine($"Access Exception: {e.Message}");
                    }
                }
            }

            var content = CodeFile.Replace("REPLACE_HASHES", string.Join(",\n", pairs));
            content = content.Replace("REPLACE_NAMES", string.Join(",\n", names));
            File.WriteAllText(outputFile, content);
        }

        #endregion
    }
}