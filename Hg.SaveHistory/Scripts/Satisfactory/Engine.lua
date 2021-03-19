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


local getSafeSessionName = function(snapshot)
    local sessionName = snapshot:CustomValueByKey("SessionName").Value
    local safeName = HgUtility.SafePath(sessionName)

    Logger.Debug("getSafeSessionName: ", safeName)

    return safeName
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

    -- build category list from snapshots
    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]
        local sessionName = snapshot:CustomValueByKey("SessionName").Value
        local catid = snapshot.CategoryId or 0

        cat = nil
        if catid > 0 and not categories[catid] then
            cat = EngineSnapshotCategory()
            cat.Id = catid
            cat.Name = sessionName
            engine.Categories:Add(cat)
            categories[catid] = cat
        end
    end

    engine.Categories:Sort(function(c1, c2)
        if c1.Id == 0 then
            return 1
        end
        if c2.Id == 0 then
            return 1
        end
        return c1:CompareTo(c2)
    end)

    engine:CategoriesChanges()
end

local getMaxCategoryId = function()
    local maxCatId = 0
    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]
        local id = snapshot.CategoryId
        if id > maxCatId then
            maxCatId = id
        end
    end
    return maxCatId + 1
end

local parseSaveFile = function(saveFileHeader)

    Logger.Debug("parseSaveFile")

    local snapshot = EngineSnapshot()

    -- saved at
    snapshot.SavedAt = saveFileHeader.SavedAt

    -- session name
    local sessionName = saveFileHeader.SessionName

    local catid = 0
    if categoryIds[sessionName] then
        catid = categoryIds[sessionName]
    else
        catid = getMaxCategoryId()
        categoryIds[sessionName] = catid
    end
    snapshot.CategoryId = catid

    local custom = EngineSnapshotCustomValueString()
    custom.Caption = "Session Name"
    custom.ShowInDetails = true
    custom.Value = sessionName
    snapshot.CustomValues:Add("SessionName", custom)

    local custom = EngineSnapshotCustomValueInteger()
    custom.Caption = "Save Version"
    custom.ShowInDetails = true
    custom.Value = saveFileHeader.SaveVersion
    snapshot.CustomValues:Add("SaveVersion", custom)

    local custom = EngineSnapshotCustomValueInteger()
    custom.Caption = "Build Version"
    custom.ShowInDetails = true
    custom.Value = saveFileHeader.BuildVersion
    snapshot.CustomValues:Add("BuildVersion", custom)

    local playedTime = saveFileHeader.PlayedTime
    local custom = EngineSnapshotCustomValueTimeSpan()
    custom.Caption = "Played Time"
    custom.ShowInDetails = true
    custom.Value = playedTime
    snapshot.CustomValues:Add("PlayedTime", custom)

    return snapshot
end

local snapshotsFolder = ""
local sourceFolder = ""

local snapshotBackupOne = function(actionSouce, filename, isAutosave)
    Logger.Information("snapshotBackupOne: ", filename, ", ", isAutosave)

    local gameSavePath = Path.Combine(sourceFolder, filename)

    if Directory.Exists(sourceFolder) and File.Exists(gameSavePath) then
        local saveFileHeader = HgScriptSpecific.Satisfactory_GetHeaderData(gameSavePath)

        local savedAt = saveFileHeader.SavedAt
        local snapshot = snapshotsBySavedAt(savedAt)

        if snapshot == nil then
            Logger.Debug("snapshot parsing")
            snapshot = parseSaveFile(saveFileHeader)
            Logger.Debug("snapshot parsed")

            -- if needed, set snapshot.SavedAtToStringFormat
            local custom = EngineSnapshotCustomValueString()
            custom.Caption = "Original file name"
            custom.ShowInDetails = true
            custom.Value = filename
            snapshot.CustomValues:Add("OriginalFileName", custom)

            local custom = EngineSnapshotCustomValueString()
            custom.Caption = "Autosave"
            custom.ShowInDetails = true
            custom.Value = ""
            if isAutosave == true then
                custom.Value = "✓"
            end
            snapshot.CustomValues:Add("Autosave", custom)

            -- get safe session name
            local safeSessionName = getSafeSessionName(snapshot)
            local targetPath = safeSessionName

            local saveAtSafe = savedAt:ToString("yyyy-MM-dd HH.mm.ss")
            targetPath = Path.Combine(targetPath, saveAtSafe)

            -- save relative path to snapshot
            snapshot.RelativePath = targetPath

            -- full path
            targetPath = Path.Combine(snapshotsFolder, targetPath)
            targetPath = Path.Combine(targetPath, safeSessionName .. ".sav")

            Logger.Debug("sourcePath=" .. gameSavePath)
            Logger.Debug("targetPath=" .. targetPath)

            Logger.Debug("BackupHelper.CopyFile: start")
            local copyFile = BackupHelper.CopyFile(
                gameSavePath,
                targetPath,
                true,
                false
            )
            Logger.Debug("BackupHelper.CopyFile: end")

            if copyFile then

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

local snapshotBackup = function(actionSouce, filename, isAutosave)
    Logger.Information("snapshotBackup: ", actionSouce, ", ", filename, ", ", isAutosave)

    if actionSouce == ActionSource.Button then
        local files = Directory.GetFiles(sourceFolder, "*.sav")
        local listPath = {}
        for i = 0, files.Length-1 do
            local name = Path.GetFileName(files[i])
            table.insert(listPath, name)
        end
        files = nil

        local result = true
        for k, v in pairs(listPath) do
            result = result and snapshotBackupOne(actionSouce, v, false)
        end

        -- rebuild category list
        refreshCategories()

        -- trigger UI refresh
        engine:SnapshotsChanges()

        return result
    end

    if actionSouce == ActionSource.HotKey then
        local source = DirectoryInfo(sourceFolder);
        local files = source:GetFiles("*.sav")

        local file = files[0]
        local max = file.LastWriteTime
        for i = 1, files.Length-1 do
            if files[i].LastWriteTime > max then
                file = files[i]
                max = file.LastWriteTime
            end
        end
--        files:Sort(function(f1, f2)
--            return DateTime.Compare(f2.LastWriteTime, f1.LastWriteTime)
--        end)
        local name = file.Name
        files = nil
        source = nil

        local result = snapshotBackupOne(actionSouce, name, isAutosave)

        -- rebuild category list
        refreshCategories()

        -- trigger UI refresh
        engine:SnapshotsChanges()

        return result
    end

    if actionSouce == ActionSource.AutoBackup then
        local result = snapshotBackupOne(actionSouce, filename, isAutosave)

        -- rebuild category list
        refreshCategories()

        -- trigger UI refresh
        engine:SnapshotsChanges()

        return result
    end

    return false
end


local snapshotRestore = function(actionSouce, snapshot)
    Logger.Information("snapshotRestore: ", actionSouce, ", ", snapshot)

    local sourcePath = Path.Combine(snapshotsFolder, snapshot.RelativePath)
    local safeSessionName = getSafeSessionName(snapshot)
    sourcePath = Path.Combine(sourcePath, safeSessionName .. ".sav")

    local saveAtSafe = snapshot.SavedAt:ToString("yyyy-MM-dd HH.mm.ss")
    local now = DateTime.Now
    local nowSafe = now:ToString("yyyyMMdd-HHmmss")
    local filename = safeSessionName .. "_" .. nowSafe .. ".sav"
    local gameSavePath = Path.Combine(sourceFolder, filename)

    Logger.Debug("sourcePath=" .. sourcePath)
    Logger.Debug("targetPath=" .. gameSavePath)

    Logger.Debug("BackupHelper.CopyFile: start")
    local copyFile = BackupHelper.CopyFile(
        sourcePath,
        gameSavePath,
        true,
        true
    )
    Logger.Debug("BackupHelper.CopyFile: end")

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

    local isAutosave = false

    if string.match(event.Name, "^.+_autosave_[012]?_BAK[0-9]+%.sav$") then
        Logger.Debug("watcherOnEvent: excluded file")
        return
    end

    if string.match(event.Name, "^.+_autosave_[012]?%.sav$") then
        isAutosave = true
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

    local includeAutosave = engine:SettingByName("IncludeAutosave").Value
    if not isAutosave or includeAutosave then -- if auto save
        -- actionable event occured
        _watcherLastEvent = DateTime.UtcNow
        -- backup
        local status = snapshotBackup(ActionSource.AutoBackup, event.Name, isAutosave)
        engine:AutoBackupOccurred(status)
    end
end

engine.OnOpened = function()
    Logger.Debug("OnOpened")

    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]

        -- all snapshot must be saved with session name and catid
        local sessionName = snapshot:CustomValueByKey("SessionName").Value
        local catid = snapshot.CategoryId

        -- build categoryIds list
        if not categoryIds[sessionName] then
            categoryIds[sessionName] = catid
        end

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
    watcher.Filter = "*.sav"
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

engine.OnActionSnapshotBackup = function(actionSource, isAutosave)
    return snapshotBackup(actionSource, nil, isAutosave)
end

engine.OnActionSnapshotRestore = function(actionSource, snapshot)
    return snapshotRestore(actionSource, snapshot)
end
