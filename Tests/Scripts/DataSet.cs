using System.Collections.Generic;

namespace Tests.Scripts
{
    public class DataSet
    {
        #region Fields & Properties

        public string Author { get; set; }
        public bool CanAutoDetect { get; set; }

        public string DataRoot { get; set; }
        public int FileCount { get; set; }
        public string Name { get; set; }
        public string ProfileName { get; set; }
        public Dictionary<string, object> Settings { get; set; }

        public string SourceFolder { get; set; }
        public string SuggestProfileName { get; set; }
        public string Title { get; set; }

        #endregion
    }
}