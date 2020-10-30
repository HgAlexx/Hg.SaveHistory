local engine, ns = ...

-- link internal map name to level number
local mapsLevels = {
    ["game/sp/e1m1_intro/e1m1_intro"] = 1,
    ["game/sp/e1m2_battle/e1m2_battle"] = 2,
    ["game/sp/e1m3_cult/e1m3_cult"] = 3,
    ["game/sp/e1m4_boss/e1m4_boss"] = 4,
    ["game/sp/e2m1_nest/e2m1_nest"] = 5,
    ["game/sp/e2m2_base/e2m2_base"] = 6,
    ["game/sp/e2m3_core/e2m3_core"] = 7,
    ["game/sp/e2m4_boss/e2m4_boss"] = 8,
    ["game/sp/e3m1_slayer/e3m1_slayer"] = 9,
    ["game/sp/e3m2_hell/e3m2_hell"] = 10,
    ["game/sp/e3m2_hell_b/e3m2_hell_b"] = 11,
    ["game/sp/e3m3_maykr/e3m3_maykr"] = 12,
    ["game/sp/e3m4_boss/e3m4_boss"] = 13,
    ["game/hub/hub"] = 14,
    ["game/dlc/e4m1_rig/e4m1_rig"] = 1,
    ["game/dlc/e4m2_rig/e4m2_swap"] = 2,
    ["game/dlc/e4m3_rig/e4m3_mcity"] = 3,
    ["game/dlc/hub/hub"] = 99
}

-- difficulties
local difficultyValues = {
    "I'm too young to die",
    "Hurt me plenty",
    "Ultra-Violence",
    "Nightmare",
    "Ultra-Nightmare"
}

-- slot folder name
local slotFolderNames = {
    "GAME-AUTOSAVE0",
    "GAME-AUTOSAVE1",
    "GAME-AUTOSAVE2",
    "DLC1-AUTOSAVE3",
    "DLC1-AUTOSAVE4",
    "DLC1-AUTOSAVE5"
}

local difficultyToString = function(diff)
    if diff and (diff >= 0) and (diff < 5) and difficultyValues[diff+1] then
        return difficultyValues[diff+1]
    end
    return "unknown"
end

local deathToString = function(death)
    if death and death == true then
        return "🕱"
    end
    return ""
end

-- to string function for category class
local categoryToString = function(category)
    if category.Level > 0 then
        local s = "#" .. category.Level
        if string.len(s) == 2 then
            s = s .. " "
        end
        return s .. ": " .. category.Name
    else
        return category.Name
    end
end

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


local getMapFullSafeName = function(snapshot)
    local mapDesc = snapshot:CustomValueByKey("MapDesc").Value
    local level = snapshot.CategoryId
    local safeMap = tostring(level) .. " - " .. HgUtility.SafePath(mapDesc)

    Logger.Debug("getMapFullSafeName: ", safeMap)

    return safeMap
end

local categories = {}
local refreshCategories = function()
    Logger.Debug("refreshCategories")

    local cat = nil
    if not categories[0] then
        cat = EngineSnapshotCategory()
        cat.Id = 0
        cat.Name = "All"
        cat.Level = 0
        cat.OnToString = categoryToString
        engine.Categories:Add(cat)
        categories[0] = cat
    end

    local count = engine.Categories.Count

    -- build category list from snapshots
    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]

        local mapName = snapshot:CustomValueByKey("MapName").Value
        local mapDesc = snapshot:CustomValueByKey("MapDesc").Value
        local level = mapsLevels[mapName] or 0

        cat = nil
        if level > 0 and not categories[level] then
            cat = EngineSnapshotCategory()
            cat.Id = level
            cat.Name = mapDesc or "unknown"
            cat.Level = level
            cat.OnToString = categoryToString
            engine.Categories:Add(cat)
            categories[level] = cat
        end
    end

    engine.Categories:Sort(function(c1, c2)
        return c1.Level - c2.Level
    end)

    engine:CategoriesChanges()
end

local parseGameDetailFile = function(lines, saveAt)

    Logger.Debug("parseGameDetailFile")

    local snapshot = EngineSnapshot()
    snapshot.SavedAt = saveAt

    for i = 0, lines.Length-1 do
        v = lines[i]

        -- Logger.Debug(i .. ": " .. v)

        v = HgUtility.StringTrim(v)

        local values = HgUtility.StringSplit(v, "=", StringSplitOptions.None)

        if values and (values.Length == 2) then
            if values[0] == "difficulty" then
                local custom = EngineSnapshotCustomValueInteger()
                custom.Caption = "Difficulty"
                custom.ShowInDetails = true
                custom.Value = tonumber(values[1])
                custom.OnToString = difficultyToString
                snapshot.CustomValues:Add("Difficulty", custom)
            end
            if values[0] == "mapDesc" then
                local custom = EngineSnapshotCustomValueString()
                custom.Caption = "Map Description"
                custom.ShowInDetails = true
                custom.Value = tostring(values[1])
                snapshot.CustomValues:Add("MapDesc", custom)
            end
            if values[0] == "mapName" then
                local mapName = tostring(values[1])
                local level = mapsLevels[mapName] or 0
                if level == 0 then
                    Logger.Warning("parseGameDetailFile: mapName not found!")
                end
                snapshot.CategoryId = level

                local custom = EngineSnapshotCustomValueString()
                custom.Caption = "Map Name"
                custom.ShowInDetails = true
                custom.Value = mapName
                snapshot.CustomValues:Add("MapName", custom)
            end
            if values[0] == "time" then
                local playedTime = TimeSpan(0, 0, tonumber(values[1]))
                local custom = EngineSnapshotCustomValueTimeSpan()
                custom.Caption = "Played Time"
                custom.ShowInDetails = true
                custom.Value = playedTime
                snapshot.CustomValues:Add("PlayedTime", custom)
            end
            if values[0] == "diedLastGame" then
                local custom = EngineSnapshotCustomValueBoolean()
                custom.Caption = "Death"
                custom.ShowInDetails = true
                custom.Value = (tonumber(values[1]) == 1)
                custom.OnToString = deathToString
                snapshot.CustomValues:Add("Death", custom)
            end
        end
    end

    return snapshot
end



local platform = nil
local snapshotsFolder = ""
local sourceFolder = ""
local slotIndex =  nil
local slotPath = ""
local userIdentifier = nil
local fileDecryptBase = nil

local snapshotBackup = function(actionSouce)
    Logger.Information("snapshotBackup: ", actionSouce)

    -- slotPath: source
    -- snapshotsFolder: target

    if Directory.Exists(slotPath) then
        local gameDetailsFilePath = Path.Combine(slotPath, "game.details")

        local savedAt = File.GetLastWriteTime(gameDetailsFilePath)
        local snapshot = snapshotsBySavedAt(savedAt)

        if snapshot == nil then
            local fileKey = fileDecryptBase .. "game.details"
            -- TODO: convert this into lua function (with .NET call for crypto, or pure lua)
            local fileContent = HgScriptSpecific.DOOMEternal_Decrypt(fileKey, gameDetailsFilePath)

            if fileContent == nil then
                Logger.Error("snapshotBackup: unable to decrypt game.details")
                return false
            end

            -- .NET version
            local lines = HgUtility.StringSplit(fileContent, "\n\r", StringSplitOptions.RemoveEmptyEntries)

            -- lua version
            -- local lines = ns.StringSplit(fileContent, "[^\r\n]+")

            Logger.Debug("snapshot parsing")
            snapshot = parseGameDetailFile(lines, savedAt)
            Logger.Debug("snapshot parsed")

            -- if needed, set snapshot.SavedAtToStringFormat

            if actionSouce == ActionSource.AutoBackup then
                -- base game
                if snapshot.CategoryId == 14 then
                    local skipFortress = engine:SettingByName("SkipFortress").Value
                    if skipFortress then
                        Logger.Debug("snapshot: Fortress checkpoint, skipping by user choice")
                        return true
                    end
                end
                -- DLC game
                if snapshot.CategoryId == 99 then
                    local skipFortress = engine:SettingByName("SkipFortress").Value
                    if skipFortress then
                        Logger.Debug("snapshot: Fortress checkpoint, skipping by user choice")
                        return true
                    end
                end

                local customDeath = snapshot:CustomValueByKey("Death").Value
                local includeDeath = engine:SettingByName("IncludeDeath").Value
                if customDeath and not includeDeath then
                    Logger.Debug("snapshot: death checkpoint, skipping")
                    return true
                end
            end

            -- get safe map name
            local targetPath = getMapFullSafeName(snapshot)

            local saveAtSafe = savedAt:ToString("yyyy-MM-dd HH.mm.ss")
            targetPath = Path.Combine(targetPath, saveAtSafe)

            -- save relative path to snapshot
            snapshot.RelativePath = targetPath

            local slotFolderName = slotFolderNames[slotIndex]
            targetPath = Path.Combine(targetPath, slotFolderName)

            -- full path
            targetPath = Path.Combine(snapshotsFolder, targetPath)

            Logger.Debug("sourcePath=" .. slotPath)
            Logger.Debug("targetPath=" .. targetPath)

            local canCopy = function(filename, mode) -- canCopy callback
                if mode == BackupHelperCanCopyMode.Copy then
                    return not ns.StringEndsWith(filename, "-BACKUP")
                end
            end

            Logger.Debug("BackupHelper.CopyFiles: start")
            local copyFiles = BackupHelper.CopyFiles(
                slotPath,
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

                -- it is up to the engine to set LastSnapshot to enable auto select feature
                engine.LastSnapshot = snapshot

                -- rebuild category list
                refreshCategories()

                -- trigger UI refresh
                engine:SnapshotsChanges()
            else
                return false
            end
        else
            if snapshot.Status == EngineSnapshotStatus.Deleted then
                Logger.Information("snapshot: restored to active")
                snapshot.Status = EngineSnapshotStatus.Active
                -- it is up to the engine to set LastSnapshot to enable auto select feature
                engine.LastSnapshot = snapshot
                -- trigger UI refresh
                engine:SnapshotsChanges()
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
    local slotFolderName = slotFolderNames[slotIndex]
    sourcePath = Path.Combine(sourcePath, slotFolderName)

    Logger.Debug("sourcePath=" .. sourcePath)
    Logger.Debug("targetPath=" .. slotPath)

    Logger.Debug("BackupHelper.CopyFiles: start")
    local copyFiles = BackupHelper.CopyFiles(
        sourcePath,
        slotPath,
        nil,
        nil,
        true,
        true
    )
    Logger.Debug("BackupHelper.CopyFiles: end")

    if copyFiles then
        Logger.Debug("snapshot restored")
    else
        return false
    end

    return true
end


local _checkpointStartTime = nil
local _checkpointBuffer = ""
local _isGameDuration = false

local watcherOnEvent = function(eventType, event)
    Logger.Debug("watcherOnEvent: event.Name = ", event.Name)

    if _checkpointStartTime ~= nil then
        local timeSpan = DateTime.UtcNow:Subtract(_checkpointStartTime)

        -- 3 seconds might be too small for some computers, we'll see if we need to add this into a runtime setting
        if timeSpan.TotalSeconds >= 3 then
            Logger.Debug("watcherOnEvent: Too much time since last event, _checkpointBuffer was ", _checkpointBuffer)

            -- Too much time since last event, reset states
            _checkpointBuffer = ""
            _checkpointStartTime = nil
            _isGameDuration = false
        end
    end

    if not _isGameDuration and event.Name == "game_duration.dat" then
        _isGameDuration = true
        _checkpointBuffer = _checkpointBuffer .. "G"
    end

    if _checkpointStartTime == nil and _checkpointBuffer ~= "" then
        _checkpointStartTime = DateTime.UtcNow
    end

    if event.Name == "game.details" then
        -- here we go
        Logger.Debug("watcherOnEvent: game.details.verify event, _checkpointBuffer = ", _checkpointBuffer)

        -- wait a bit to be sure file is saved to disk
        HgUtility.Sleep(100)

        -- backup
        local status = snapshotBackup(ActionSource.AutoBackup)
        engine:AutoBackupOccurred(status)

        _isGameDuration = false

        _checkpointBuffer = ""
        _checkpointStartTime = nil
    end

end

engine.OnOpened = function()
    Logger.Debug("OnOpened")

    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]

        -- if needed, set snapshot.OnEquals

        local customValue = nil

        customValue = snapshot:CustomValueByKey("Difficulty")
        if customValue then
            -- set customValue.OnToString
            customValue.OnToString = difficultyToString
        end

        customValue = snapshot:CustomValueByKey("Death")
        if customValue then
            -- set customValue.OnToString
            customValue.OnToString = deathToString
        end

        -- if needed, handle other customValue

    end

    refreshCategories()
end

local NotifyFilters_FileName = 1
local NotifyFilters_DirectoryName = 2

local watcher = nil

engine.OnInitialized = function()
    Logger.Debug("OnInitialized")

    platform =  engine:SettingByName("Platform").Value
    Logger.Debug("platform=" .. platform)

    --  Target backup folder
    snapshotsFolder = engine.SnapshotsFolder
    Logger.Debug("snapshotsFolder=" .. snapshotsFolder)

    sourceFolder =  engine:SettingByName("SourceFolder").Value
    Logger.Debug("sourceFolder=" .. sourceFolder)

    slotIndex =  engine:SettingByName("SlotIndex").Value
    Logger.Debug("slotIndex=" .. slotIndex)

    -- full source path (where files are)
    local slotFolderName = slotFolderNames[slotIndex]
    slotPath = Path.Combine(sourceFolder, slotFolderName)
    Logger.Debug("slotPath=" .. slotPath)

    userIdentifier =  engine:SettingByName("UserIdentifier").Value
    Logger.Debug("userIdentifier=" .. userIdentifier)

    if platform == 1 then
        local steamId = HgSteamHelper.SteamId3ToSteamId64(userIdentifier)
        fileDecryptBase = steamId .. "MANCUBUS"
    end

    if platform == 2 then
        fileDecryptBase = userIdentifier .. "PAINELEMENTAL"
    end

    Logger.Debug("fileDecryptBase=" .. fileDecryptBase)

    watcher = EngineWatcher()
    watcher.Path = slotPath
    watcher.WatchParent = true
    watcher.Filter = "*"
    watcher.NotifyFilter = NotifyFilters_FileName
    watcher.WatchRenamed = true
    watcher.WatchChanged = true
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
