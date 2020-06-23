using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hg.SaveHistory.Types
{
    public class EngineScriptFile
    {
        #region Fields & Properties

        public bool Altered { get; set; }
        public string FileFullName { get; set; }
        public string FileName { get; set; }

        public string Hash => HashFile();
        public bool IsToc { get; set; }

        #endregion

        #region Members

        private string HashFile()
        {
            if (File.Exists(FileFullName))
            {
                FileInfo fileInfo = new FileInfo(FileFullName);

                SHA256 sha256 = SHA256.Create();
                StringBuilder builder = new StringBuilder();
                using (FileStream fileStream = fileInfo.Open(FileMode.Open))
                {
                    fileStream.Position = 0;
                    byte[] hashValue = sha256.ComputeHash(fileStream);

                    foreach (var t in hashValue)
                    {
                        builder.Append(t.ToString("x2"));
                    }
                }

                return builder.ToString();
            }

            return "";
        }

        #endregion
    }
}