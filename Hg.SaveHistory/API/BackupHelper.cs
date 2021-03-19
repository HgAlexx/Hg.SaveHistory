using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NLua;

namespace Hg.SaveHistory.API
{
    public static class BackupHelper
    {
        #region Members

        public static bool CopyFile(string sourceFullName, string targetFullName, bool overrideIfExists, bool withBackup)
        {
            Logger.Information(MethodBase.GetCurrentMethod().DeclaringType.Name, ".", MethodBase.GetCurrentMethod().Name);

            Logger.Debug("sourcePath: ", sourceFullName);
            Logger.Debug("targetPath: ", targetFullName);
            Logger.Debug("overrideIfExists: ", overrideIfExists);
            Logger.Debug("withBackup: ", withBackup);

            try
            {
                if (!File.Exists(sourceFullName))
                {
                    return false;
                }

                string targetDirectory = Path.GetDirectoryName(targetFullName);
                if (targetDirectory == null)
                {
                    return false;
                }

                Directory.CreateDirectory(targetDirectory);

                bool canRestore = false;
                if (withBackup)
                {
                    try
                    {
                        string name = targetFullName + ".hg.bak";
                        if (File.Exists(targetFullName))
                        {
                            File.Move(targetFullName, name);

                            Thread.Sleep(100);

                            canRestore = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        Utilities.Logger.Error("CopyFile, withBackup failed: ", exception.Message);
                        Utilities.Logger.LogException(exception);
                    }
                }

                try
                {
                    File.Copy(sourceFullName, targetFullName, overrideIfExists);

                    Thread.Sleep(100);

                    if (withBackup)
                    {
                        string name = targetFullName + ".hg.bak";
                        if (File.Exists(name))
                        {
                            File.Delete(name);
                            canRestore = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Logger.Error("CopyFile, files copy failed: ", ex.Message);
                    Utilities.Logger.LogException(ex);

                    if (withBackup && canRestore)
                    {
                        // Clean up
                        if (File.Exists(targetFullName))
                        {
                            File.Delete(targetFullName);
                        }

                        Thread.Sleep(100);

                        // Restore backup
                        string name = targetFullName + ".hg.bak";
                        if (File.Exists(name))
                        {
                            File.Move(name, targetFullName);
                            canRestore = false;
                        }
                    }

                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                Utilities.Logger.Error("CopyFile: ", exception.Message);
                Utilities.Logger.LogException(exception);
            }

            return false;
        }


        public static bool CopyFiles(string sourcePath, string targetPath, LuaFunction canCopy, LuaFunction mustWait, bool overrideIfExists,
            bool withBackup)
        {
            Logger.Information(MethodBase.GetCurrentMethod().DeclaringType.Name, ".", MethodBase.GetCurrentMethod().Name);

            Logger.Debug("sourcePath: ", sourcePath);
            Logger.Debug("targetPath: ", targetPath);
            Logger.Debug("canCopy: ", canCopy);
            Logger.Debug("mustWait: ", mustWait);
            Logger.Debug("overrideIfExists: ", overrideIfExists);
            Logger.Debug("withBackup: ", withBackup);

            try
            {
                if (!Directory.Exists(sourcePath))
                {
                    return false;
                }

                Directory.CreateDirectory(targetPath);

                var source = new DirectoryInfo(sourcePath);
                var target = new DirectoryInfo(targetPath);

                if (mustWait != null)
                {
                    int tries = 0;
                    while (tries < 10)
                    {
                        bool needToWait = false;
                        foreach (var fileInfo in source.GetFiles())
                        {
                            if (mustWait.Call(fileInfo.Name).First() is bool b && b)
                            {
                                Utilities.Logger.Information("CopyFiles: mustWait returned true for file ", fileInfo.Name, ", wait a bit");
                                needToWait = true;
                                break;
                            }
                        }

                        if (needToWait)
                        {
                            tries++;
                            Thread.Sleep(100);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                bool canRestore = false;
                if (withBackup)
                {
                    try
                    {
                        foreach (var fileInfo in target.GetFiles())
                        {
                            if (canCopy == null || canCopy.Call(fileInfo.Name, BackupHelperCanCopyMode.Backup).First() is bool b && b)
                            {
                                string name = fileInfo.FullName + ".hg.bak";
                                fileInfo.MoveTo(name);
                            }
                        }

                        Thread.Sleep(100);

                        canRestore = true;
                    }
                    catch (Exception exception)
                    {
                        Utilities.Logger.Error("CopyFiles, withBackup failed: ", exception.Message);
                        Utilities.Logger.LogException(exception);
                    }
                }

                try
                {
                    foreach (var fileInfo in source.GetFiles())
                    {
                        if (canCopy == null || canCopy.Call(fileInfo.Name, BackupHelperCanCopyMode.Copy).First() is bool b && b)
                        {
                            fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), overrideIfExists);
                        }
                    }

                    Thread.Sleep(100);

                    if (withBackup)
                    {
                        foreach (var fileInfo in target.GetFiles())
                        {
                            if (fileInfo.Name.EndsWith(".hg.bak"))
                            {
                                fileInfo.Delete();
                                canRestore = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.Logger.Error("CopyFiles, files copy failed: ", ex.Message);
                    Utilities.Logger.LogException(ex);

                    if (withBackup && canRestore)
                    {
                        // Clean up
                        foreach (var fileInfo in target.GetFiles())
                        {
                            if (!fileInfo.Name.EndsWith(".hg.bak") &&
                                (canCopy == null ||
                                 canCopy.Call(fileInfo.Name, BackupHelperCanCopyMode.Copy).First() is bool b && b))
                            {
                                fileInfo.Delete();
                            }
                        }

                        Thread.Sleep(100);

                        // Restore backup
                        foreach (var fileInfo in target.GetFiles())
                        {
                            if (fileInfo.Name.EndsWith(".hg.bak"))
                            {
                                fileInfo.MoveTo(fileInfo.FullName.Replace(".hg.bak", ""));
                            }
                        }
                    }

                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                Utilities.Logger.Error("CopyFiles: ", exception.Message);
                Utilities.Logger.LogException(exception);
            }

            return false;
        }

        #endregion
    }
}