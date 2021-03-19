using Hg.SaveHistory.API;

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
            string hash = HgUtility.HashFile(FileFullName);

            if (string.IsNullOrEmpty(hash))
            {
                return "";
            }

            return hash;
        }

        #endregion
    }
}