using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hg.SaveHistory.Types;

namespace Hg.SaveHistory.Managers
{
    public class EngineScriptManager
    {
        #region Fields & Properties

        public List<EngineScript> BackupEngines { get; } = new List<EngineScript>();

        #endregion

        #region Members

        public void ScanFolder(string path)
        {
            BackupEngines.Clear();

            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            foreach (DirectoryInfo dirInfo in directoryInfo.GetDirectories())
            {
                EngineScript engineScript = null;

                foreach (FileInfo fileInfo in dirInfo.GetFiles("*.toc", SearchOption.TopDirectoryOnly))
                {
                    string filename = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    if (filename == dirInfo.Name)
                    {
                        string fileRelativePath = Path.Combine(dirInfo.Name, fileInfo.Name);

                        engineScript = new EngineScript {Name = filename};


                        var engineScriptFile = new EngineScriptFile
                        {
                            FileName = fileRelativePath, FileFullName = fileInfo.FullName, IsToc = true
                        };

                        engineScript.Files.Add(engineScriptFile);

                        List<string> tocContent = File.ReadAllLines(fileInfo.FullName).ToList();
                        foreach (string line in tocContent)
                        {
                            string trimmedLine = line.Trim();
                            if (trimmedLine.StartsWith("##"))
                            {
                                if (TocReadValue(trimmedLine, "## Title:", out var value))
                                {
                                    engineScript.Title = value;
                                    continue;
                                }

                                if (TocReadValue(trimmedLine, "## Author:", out value))
                                {
                                    engineScript.Author = value;
                                    continue;
                                }

                                if (TocReadValue(trimmedLine, "## Description:", out value))
                                {
                                    if (!string.IsNullOrEmpty(engineScript.Description))
                                    {
                                        engineScript.Description += Environment.NewLine;
                                    }

                                    engineScript.Description += value;
                                    continue;
                                }
                            }

                            if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("##") && trimmedLine.EndsWith(".lua"))
                            {
                                fileRelativePath = Path.Combine(dirInfo.Name, trimmedLine);
                                string fileFullPath = Path.Combine(dirInfo.FullName, trimmedLine);

                                engineScriptFile = new EngineScriptFile
                                {
                                    FileName = fileRelativePath, FileFullName = fileFullPath, IsToc = false
                                };

                                engineScript.Files.Add(engineScriptFile);
                            }
                        }

                        break; // only one toc file per profile engine
                    }
                }

                if (engineScript != null && engineScript.IsValid())
                {
                    engineScript.IsAltered(true);
                    BackupEngines.Add(engineScript);
                }
            }
        }

        private static bool TocReadValue(string line, string header, out string value)
        {
            if (line.StartsWith(header))
            {
                value = line.Substring(header.Length + 1);
                return true;
            }

            value = "";
            return false;
        }

        #endregion
    }
}