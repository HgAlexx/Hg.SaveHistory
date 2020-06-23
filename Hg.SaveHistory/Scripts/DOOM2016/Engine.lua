local engine, ns = ...

-- link internal map name to level number
local mapsLevels = {
    ["game/sp/intro/intro"] = 1,
    ["game/sp/resource_ops/resource_ops"] = 2,
    ["game/sp/resource_ops_foundry/resource_ops_foundry"] = 3,
    ["game/sp/surface1/surface1"] = 4,
    ["game/sp/argent_tower/argent_tower"] = 5,
    ["game/sp/blood_keep/blood_keep"] = 6,
    ["game/sp/surface2/surface2"] = 7,
    ["game/sp/bfg_division/bfg_division"] = 8,
    ["game/sp/lazarus/lazarus"] = 9,
    ["game/sp/lazarus_2/lazarus_2"] = 9,
    ["game/sp/blood_keep_b/blood_keep_b"] = 10,
    ["game/sp/blood_keep_c/blood_keep_c"] = 11,
    ["game/sp/polar_core/polar_core"] = 12,
    ["game/sp/titan/titan"] = 13
}

-- difficulties
local difficultyValues = {
    "I'm too young to die",
    "Hurt me plenty",
    "Ultra-Violence",
    "Nightmare",
    "Ultra-Nightmare"
}

local difficultyToString = function(diff)
    if diff and (diff >= 0) and (diff < 5) and difficultyValues[diff+1] then
        return difficultyValues[diff+1]
    end
    return "unknown"
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

local snapshotGetCustomValue = function(snapshot, key)
    if snapshot.CustomValues:ContainsKey(key) then
        return snapshot.CustomValues[key]
    end 
    return nil
end

local snapshotsContainsSavedAt = function(savedAt)
    Logger.Debug("snapshotsContainsSavedAt")

    local count = engine.Snapshots.Count;    
    for i = 0, count-1 do
        local snapshot = engine.Snapshots[i]
        
        if snapshot.SavedAt == savedAt then
            return true
        end
    end

    return false
end

local categories = {}
local getCategoryById = function(id)
    if categories[id] then
        return categories[id]
    end
    return nil
end

local getMapFullSafeName = function(snapshot)
    local mapDesc = snapshotGetCustomValue(snapshot, "MapDesc").Value
    local level = snapshot.CategoryId
    local safeMap = tostring(level) .. " - " .. HgUtility.SafePath(mapDesc)
    
    Logger.Debug("getMapFullSafeName: ", safeMap)

    return safeMap
end

local refreshCategories = function()
    Logger.Debug("refreshCategories")

    local cat = getCategoryById(0)
    if not cat then
        cat = EngineSnapshotCategory()
        cat.Id = 0
        cat.Name = "All"
        cat.Level = 0
        cat.OnToString = categoryToString
        engine.Categories:Add(cat)
        categories[0] = cat
    end

    local count = engine.Categories.Count;
    
    -- build category list from snapshots
    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]

        local mapName = snapshotGetCustomValue(snapshot, "MapName").Value
        local mapDesc = snapshotGetCustomValue(snapshot, "MapDesc").Value
        local level = mapsLevels[mapName] or 0
        
        cat = getCategoryById(level)

        if level > 0 and not cat then
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

local parseGameDetailFile = function(lines)
    
    Logger.Debug("parseGameDetailFile")

    local snapshot = EngineSnapshot()

    for i = 0, lines.Length-1 do
        v = lines[i]

        -- Logger.Debug(i .. ": " .. v)

        v = HgUtility.StringTrim(v)

        local values = HgUtility.StringSplit(v, "=", StringSplitOptions.None)

        if values and (values.Length == 2) then
            if values[0] == "date" then
                local datetime = HgConverter.UnixToDateTime(tonumber(values[1]))
                snapshot.SavedAt = datetime
            end
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
        end
    end

    return snapshot
end

local getSavedAtFromGameDetailFile = function(lines)
    
    Logger.Debug("getSavedAtFromGameDetailFile")

    for i = 0, lines.Length-1 do
        v = lines[i]
        v = HgUtility.StringTrim(v)

        local values = HgUtility.StringSplit(v, "=", StringSplitOptions.None)

        if values and (values.Length == 2) then
            if values[0] == "date" then
                local datetime = HgConverter.UnixToDateTime(tonumber(values[1]))
                return datetime
            end
        end
    end

    return nil
end


local snapshotsFolder = ""
local sourceFolder = ""
local slotIndex =  nil
local slotPath = ""

local snapshotBackup = function(actionSouce, isDeath)
    Logger.Debug("snapshotBackup")

    -- slotPath: source
    -- snapshotsFolder: target

    if Directory.Exists(slotPath) then
        local gameDetailsFilePath = Path.Combine(slotPath, "game.details");
        local fileContent = File.ReadAllText(gameDetailsFilePath);
        
        -- .NET version
        local lines = HgUtility.StringSplit(fileContent, "\n\r", StringSplitOptions.RemoveEmptyEntries)

        -- lua version
        -- local lines = ns.StringSplit(fileContent, "[^\r\n]+")

        local savedAt = getSavedAtFromGameDetailFile(lines)
        
        if not snapshotsContainsSavedAt(savedAt) then
            
            Logger.Debug("snapshot parsing")
            local snapshot = parseGameDetailFile(lines)
            Logger.Debug("snapshot parsed")

            -- if needed, set snapshot.SavedAtToStringFormat

            local custom = EngineSnapshotCustomValueString()            
            custom.Caption = "Death"
            custom.ShowInDetails = true
            custom.Value = ""
            if isDeath then
                custom.Value = "🕱"
            end
            snapshot.CustomValues:Add("Death", custom)

            -- get safe map name
            local targetPath = getMapFullSafeName(snapshot)

            local saveAtSafe = savedAt:ToString("yyyy-MM-dd HH.mm.ss");
            targetPath = Path.Combine(targetPath, saveAtSafe)

            -- save relative path to snapshot
            snapshot.RelativePath = targetPath

            -- "GAME-AUTOSAVE" + (_id - 1);
            targetPath = Path.Combine(targetPath, "GAME-AUTOSAVE" .. (slotIndex - 1))

            -- full path
            targetPath = Path.Combine(snapshotsFolder, targetPath)

            Logger.Debug("sourcePath=" .. slotPath)
            Logger.Debug("targetPath=" .. targetPath)

            local canCopy = function(filename, mode) -- canCopy callback
                if mode == BackupHelperCanCopyMode.Copy then
                    return not ns.StringEndsWith(filename, ".temp") and not ns.StringEndsWith(filename, ".temp.verify")
                end
            end
            local mustWait = function(filename) -- mustWait callback
                return ns.StringEndsWith(filename, ".temp") or ns.StringEndsWith(filename, ".temp.verify")
            end

            Logger.Debug("BackupHelper.CopyFiles: start")
            local copyFiles = BackupHelper.CopyFiles(
                slotPath, 
                targetPath, 
                canCopy,
                mustWait,
                true,
                false
            )
            Logger.Debug("BackupHelper.CopyFiles: end")

            if copyFiles then

                -- autobackup: screenshot
                if actionSouce == ActionSource.AutoBackup then
                    local screenshot = engine:GetSettingByName("Screenshot").Value
                    if screenshot then
                        local bounds = engine.ScreenshotHelper:GetWindowBounds()
                        if bounds then
                            local capture = engine.ScreenshotHelper:Capture(bounds, snapshot)
                            if capture then
                                Logger.Debug("snapshot: screenshot success")
                            else
                                Logger.Debug("snapshot: screenshot failed")
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
            Logger.Debug("snapshot: already known")
        end

        return true    
    end

    return false
end


local snapshotRestore = function(actionSouce, snapshot)
    Logger.Debug("snapshotRestore")

    local sourcePath = Path.Combine(snapshotsFolder, snapshot.RelativePath)
    sourcePath = Path.Combine(sourcePath, "GAME-AUTOSAVE" .. (slotIndex - 1))

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
local _isCheckPoint = false;
local _isCheckPointAlt = false;
local _isGameDuration = false;
local _isCheckPointMapStart = false;

local watcherOnEvent = function(eventType, event)
    
    Logger.Debug("watcherOnEvent: event.Name = ", event.Name)

    if _checkpointStartTime ~= nil then
        local timeSpan = DateTime.UtcNow:Subtract(_checkpointStartTime)

        -- 3 seconds might be too small for some computers, we'll see if we need to add this into a runtime setting
        if timeSpan.TotalSeconds >= 3 then
            if _checkpointBuffer == "AG" then
                Logger.Warning("AutoBackup: cannot backup checkpoints while replaying missions")
            else
                Logger.Debug("watcherOnEvent: Too much time since last event, _checkpointBuffer was ", _checkpointBuffer)
            end
            
            -- Too much time since last event, reset states
            _checkpointBuffer = "";
            _checkpointStartTime = nil;
            _isCheckPoint = false;
            _isCheckPointAlt = false;
            _isGameDuration = false;
            _isCheckPointMapStart = false;
        end
    end

    if not _isGameDuration and event.Name == "game_duration.dat" then
        _isGameDuration = true
        _checkpointBuffer = _checkpointBuffer .. "G";
    end

    if not _isCheckPointMapStart and event.Name == "checkpoint_mapstart.dat" then
        _isCheckPointMapStart = true
        _checkpointBuffer = _checkpointBuffer .. "M";
    end

    -- Mostly in case of death
    if not _isCheckPointAlt and event.Name == "checkpoint_alt.dat" then
        _isCheckPointAlt = true
        _checkpointBuffer = _checkpointBuffer .. "A";
    end

    -- Proper checkpoint
    if not _isCheckPoint and event.Name == "checkpoint.dat" then
        _isCheckPoint = true
        _checkpointBuffer = _checkpointBuffer .. "C";
    end

    if _checkpointStartTime == nil and _checkpointBuffer ~= "" then
        _checkpointStartTime = DateTime.UtcNow
    end

    if event.Name == "game.details.verify" then
        -- here we go
        Logger.Debug("watcherOnEvent: game.details.verify event, _checkpointBuffer = ", _checkpointBuffer)

        if ns.StringStartsWith(_checkpointBuffer, "CGMA") then
            Logger.Debug("watcherOnEvent: map change")
            -- Map change checkpoint (usually)
            -- do nothing

        elseif ns.StringStartsWith(_checkpointBuffer, "CMGA") then
            Logger.Debug("watcherOnEvent: normal checkpoint")
            -- Normal checkpoint (usually)
            
            -- backup
            local status = snapshotBackup(ActionSource.AutoBackup, false)
            engine:AutoBackupOccurred(status)

        elseif _checkpointBuffer == "GA" then
            Logger.Debug("watcherOnEvent: death checkpoint")
            -- Death checkpoint (usually)
            
            -- backup
            local includeDeath = engine:GetSettingByName("IncludeDeath").Value
            if includeDeath then
                local status = snapshotBackup(ActionSource.AutoBackup, true)
                engine:AutoBackupOccurred(status)
            end
        end

        _isCheckPoint = false
        _isCheckPointAlt = false
        _isGameDuration = false
        _isCheckPointMapStart = false

        _checkpointBuffer = ""
        _checkpointStartTime = nil
    end

end

engine.OnOpen = function()
    Logger.Debug("OnOpen")

    for i = 0, engine.Snapshots.Count-1 do
        local snapshot = engine.Snapshots[i]

        -- if needed, set snapshot.OnEquals

        local customValue = nil

        customValue = snapshotGetCustomValue(snapshot, "Difficulty")
        if customValue then
            -- set customValue.OnToString
            customValue.OnToString = difficultyToString
        end

        -- if needed, handle other customValue

    end
end

local NotifyFilters_FileName = 1
local NotifyFilters_DirectoryName = 2

engine.OnInitialize = function()
    Logger.Debug("OnInitialize")

    --  Target backup folder
    snapshotsFolder = engine.SnapshotsFolder
    Logger.Debug("snapshotsFolder=" .. snapshotsFolder)

    sourceFolder =  engine:GetSettingByName("SourceFolder").Value
    Logger.Debug("sourceFolder=" .. sourceFolder)

    slotIndex =  engine:GetSettingByName("SlotIndex").Value
    Logger.Debug("slotIndex=" .. slotIndex)

    -- full source path (where files are)
    slotPath = Path.Combine(sourceFolder, "GAME-AUTOSAVE" .. (slotIndex - 1))
    Logger.Debug("slotPath=" .. slotPath)
    
    local watcher = EngineWatcher()
    watcher.Path = slotPath
    watcher.WatchParent = true
    watcher.Filter = "*"
    watcher.NotifyFilter = NotifyFilters_FileName
    watcher.WatchRenamed = true
    watcher.OnEvent = watcherOnEvent
    
    engine:SetupWatcher(watcher)
    
    engine.Categories:Clear()

    refreshCategories()

    engine:SnapshotsChanges()
end

engine.OnActionSnapshotBackup = function(actionSource)
    -- manual backup cannot be detected as death or not
    return snapshotBackup(actionSource, false)
end

engine.OnActionSnapshotRestore = function(actionSource, snapshot)
    return snapshotRestore(actionSource, snapshot)
end
