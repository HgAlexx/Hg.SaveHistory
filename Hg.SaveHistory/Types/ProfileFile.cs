using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Hg.SaveHistory.API;
using Newtonsoft.Json;
using Logger = Hg.SaveHistory.Utilities.Logger;

namespace Hg.SaveHistory.Types
{
    public class ProfileFile
    {
        #region Fields & Properties

        // Name of engine script
        public string EngineScriptName;

        public string FilePath;

        public bool FirstTimeRun = true;

        // Display name of profile
        public string Name;

        public string RootFolder;

        public List<ProfileSetting> Settings = new List<ProfileSetting>();

        // Snapshots
        public List<EngineSnapshot> Snapshots = new List<EngineSnapshot>();

        public string SortKey = "SavedAt";

        // Settings
        public SortOrder SortOrder = SortOrder.Ascending;

        #endregion

        #region Members

        public static ProfileFile FromJson(string content)
        {
            ProfileFile profileFile =
                JsonConvert.DeserializeObject<ProfileFile>(content, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

            if (profileFile != null && string.IsNullOrEmpty(profileFile.SortKey))
                profileFile.SortKey = "SavedAt";

            return profileFile;
        }

        public static ProfileFile Load(string path)
        {
            if (File.Exists(path))
            {
                var profile = FromJson(File.ReadAllText(path));

                profile.FilePath = path;
                profile.RootFolder = Path.GetDirectoryName(path);

                return profile;
            }

            return null;
        }

        public void Release()
        {
            foreach (var snapshot in Snapshots)
            {
                foreach (var pair in snapshot.CustomValues)
                {
                    pair.Value.OnToString = null;
                }

                snapshot.OnEquals = null;
            }

            Snapshots.Clear();

            Settings.Clear();
        }

        public static bool Save(ProfileFile profile)
        {
            string content = ToJson(profile);

            try
            {
                if (File.Exists(profile.FilePath))
                {
                    string backupPath = profile.FilePath + ".bak";
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }

                    File.Move(profile.FilePath, backupPath);
                }

                File.WriteAllText(profile.FilePath, content);
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
                return false;
            }

            return true;
        }

        public static string ToJson(ProfileFile profileFile)
        {
            string content = JsonConvert.SerializeObject(profileFile, Formatting.Indented,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
            return content;
        }

        #endregion
    }
}