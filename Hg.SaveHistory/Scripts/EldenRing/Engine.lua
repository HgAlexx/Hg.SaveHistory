local engine, ns = ...

local snapshotsBySavedAt = function(savedAt)
    Logger.Debug("snapshotsBySavedAt")

    local count = engine.Snapshots.Count
    for i = 0, count-1 do
        local snapshot = engine.Snapshots[i]

        if snapshot.SavedAt == savedAt then
            return snapshot
        end
    end

    return nil
end

local categories = {}
local categoryIds = {}
local refreshCategories = function()
    Logger.Debug("refreshCategories")

    local cat = nil
    if not categories[0] then
        cat = EngineSnapshotCategory()
        cat.Id = 0
        cat.Name = "All"
        engine.Categories:Add(cat)
        categories[0] = cat
    end

    engine:CategoriesChanges()
end

local snapshotsFolder = ""
local sourceFolder = ""

local snapshotBackup = function(actionSouce)
    Logger.Information("snapshotBackup: ", actionSouce)

    if Directory.Exists(sourceFolder) then
        local filePath = Path.Combine(sourceFolder, "ER0000.sl2")
        local savedAt = File.GetLastWriteTime(filePath)
        local snapshot = snapshotsBySavedAt(savedAt)

        if snapshot == nil then
            snapshot = EngineSnapshot()
            snapshot.SavedAt = savedAt

            -- if needed, set snapshot.SavedAtToStringFormat

            local saveAtSafe = savedAt:ToString("yyyy-MM-dd HH.mm.ss")
            local targetPath = saveAtSafe

            -- save relative path to snapshot
            snapshot.RelativePath = targetPath

            -- full path
            targetPath = Path.Combine(snapshotsFolder, targetPath)

            Logger.Debug("sourcePath=" .. sourceFolder)
            Logger.Debug("targetPath=" .. targetPath)


            local canCopy = function(filename, mode) -- canCopy callback
                if mode == BackupHelperCanCopyMode.Copy then
                    return not ns.StringEndsWith(filename, ".vdf")
                end
                return true
            end

            Logger.Debug("BackupHelper.CopyFiles: start")
            local copyFiles = BackupHelper.CopyFiles(
                sourceFolder,
                targetPath,
                canCopy,
                nil,
                true,
                false
            )
            Logger.Debug("BackupHelper.CopyFiles: end")

            if copyFiles then
                -- autobackup: screenshot
                if actionSouce == ActionSource.AutoBackup then
                    local screenshot = engine:SettingByName("Screenshot").Value
                    if screenshot then
                        local bounds = engine.ScreenshotHelper:GetWindowBounds()
                        if bounds then
                            local capture = engine.ScreenshotHelper:Capture(bounds, snapshot)
                            if capture then
                                Logger.Debug("snapshot: screenshot success")
                            else
                                Logger.Warning("snapshot: screenshot failed")
                            end
                        end
                    end
                end

                Logger.Debug("snapshot: adding to list")
                engine.Snapshots:Add(snapshot)

                if actionSouce == ActionSource.AutoBackup or actionSouce == ActionSource.HotKey then
                    -- it is up to the engine to set LastSnapshot to enable auto select feature
                    engine.LastSnapshot = snapshot
                end

                -- trigger UI refresh
                engine:SnapshotsChanges()
            else
                return false
            end
        else
            if snapshot.Status == EngineSnapshotStatus.Deleted then
                Logger.Information("snapshot: restored to active")
                snapshot.Status = EngineSnapshotStatus.Active
                if actionSouce == ActionSource.AutoBackup or actionSouce == ActionSource.HotKey then
                    -- it is up to the engine to set LastSnapshot to enable auto select feature
                    engine.LastSnapshot = snapshot
                    -- trigger UI refresh
                    engine:SnapshotsChanges()
                end
            else
                Logger.Information("snapshot: already known")
            end
        end

        return true
    end

    return false
end


local snapshotRestore = function(actionSouce, snapshot)
    Logger.Information("snapshotRestore: ", actionSouce, ", ", snapshot)

    local sourcePath = Path.Combine(snapshotsFolder, snapshot.RelativePath)

    Logger.Debug("sourcePath=" .. sourcePath)
    Logger.Debug("targetPath=" .. sourceFolder)

    local canCopy = function(filename, mode) -- canCopy callback
        if mode == BackupHelperCanCopyMode.Backup then
            return not ns.StringEndsWith(filename, ".vdf")
        end
        return true
    end

    Logger.Debug("BackupHelper.CopyFiles: start")
    local copyFiles = BackupHelper.CopyFiles(
        sourcePath,
        sourceFolder,
        canCopy,
        nil,
        true,
        true
    )
    Logger.Debug("BackupHelper.CopyFiles: end")

    if copyFile then
        Logger.Debug("snapshot restored")
    else
        return false
    end

    return true
end

local _watcherLastEvent = nil
local _watcherLastEventName = nil
local watcherOnEvent = function(eventType, event)
    Logger.Debug("watcherOnEvent: event.Name = ", event.Name, ", eventType = ", eventType)

    if string.match(event.Name, "^.+%.bak$") then
        Logger.Debug("watcherOnEvent: excluded file")
        return
    end

    -- if the event name is the same as the previous one, check timespan
    if _watcherLastEventName == event.Name then
        if _watcherLastEvent ~= nil then
            local timeSpan = DateTime.UtcNow:Subtract(_watcherLastEvent)
            if timeSpan.TotalSeconds <= 3 then
                Logger.Debug("watcherOnEvent: waiting a bit to prevent double event on same save")
                return -- prevent double event on same save
            end
        end
    end
    _watcherLastEventName = event.Name

    -- actionable event occured
    _watcherLastEvent = DateTime.UtcNow

    -- wait a bit to be sure file is saved to disk
    HgUtility.Sleep(250)

    -- backup
    local status = snapshotBackup(ActionSource.AutoBackup)
    engine:AutoBackupOccurred(status)
end

engine.OnOpened = function()
    Logger.Debug("OnOpened")

    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]

        -- if needed, set snapshot.OnEquals

        -- if needed, handle other customValue
    end

    refreshCategories()
end


local watcher = nil

engine.OnInitialized = function()
    Logger.Debug("OnInitialized")

    --  Target backup folder
    snapshotsFolder = engine.SnapshotsFolder
    Logger.Debug("snapshotsFolder=" .. snapshotsFolder)

    sourceFolder =  engine:SettingByName("SourceFolder").Value
    Logger.Debug("sourceFolder=" .. sourceFolder)

    watcher = EngineWatcher()
    watcher.Path = sourceFolder
    watcher.WatchParent = false
    watcher.Filter = "*.sl2"
    --watcher.WatchCreated = true
    watcher.WatchChanged = true
    --watcher.WatchRenamed = true
    watcher.OnEvent = watcherOnEvent

    engine:SetupWatcher(watcher)
end

engine.OnLoaded = function()
    Logger.Debug("OnLoaded")

    engine:CategoriesChanges()
    engine:SnapshotsChanges()
end

engine.OnClosing = function()
    Logger.Debug("OnClosing")

    return true
end

engine.OnClosed = function()
    Logger.Debug("OnClosed")

    if watcher then
        watcher.OnEvent = nil
    end
    watcher = nil

    categories = nil
end

engine.OnActionSnapshotBackup = function(actionSource)
    return snapshotBackup(actionSource)
end

engine.OnActionSnapshotRestore = function(actionSource, snapshot)
    return snapshotRestore(actionSource, snapshot)
end

engine.ReadMe = function()
    local content =
[[Elden Ring

Manual backup:
This will backup all available save files in the save folder.
If a save file has already been backed-up, it is ignored.

Automatic backup:
When active, everytime a save file changes, a backup will be made.

Manual restore:
This will copy the selected snapshot file into your save folder.
You will need to be on the game intro screen, or restart the game to reload the save file.


Enjoy!]]

    return content;
end
