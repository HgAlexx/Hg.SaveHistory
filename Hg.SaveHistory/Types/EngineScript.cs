using System.Collections.Generic;
using System.Linq;

namespace Hg.SaveHistory.Types
{
    public class EngineScript
    {
        #region Fields & Properties

        public string Author { get; set; }

        public string Description { get; set; }

        public List<EngineScriptFile> Files { get; set; }
        public string Name { get; set; }

        public bool Official => IsOfficial();
        public string Title { get; set; }

        #endregion

        #region Members

        public EngineScript()
        {
            Files = new List<EngineScriptFile>();
            Description = "";
            Author = "Unknown";
        }

        public bool IsAltered(bool doHash = false)
        {
            if (doHash)
            {
                ReHash();
            }

            return Official && Files.Any(backupEngineFile => backupEngineFile.Altered);
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(Title) &&
                   Files.Count >= 1;
        }

        public void ReHash()
        {
            if (Official)
            {
                foreach (EngineScriptFile backupEngineFile in Files)
                {
                    backupEngineFile.Altered = false;
                    if (!ScriptsHash.FilesHashes.ContainsKey(backupEngineFile.FileName))
                    {
                        backupEngineFile.Altered = true;
                        break;
                    }

                    if (backupEngineFile.Hash != ScriptsHash.FilesHashes[backupEngineFile.FileName])
                    {
                        backupEngineFile.Altered = true;
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            if (Official)
            {
                string status = IsAltered() ? "Altered!" : "Valid";
                return $"{Title} ({status}) by {Author}";
            }

            return $"{Title} by {Author}";
        }

        private bool IsOfficial()
        {
            if (ScriptsHash.Officials.Contains(Name))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}