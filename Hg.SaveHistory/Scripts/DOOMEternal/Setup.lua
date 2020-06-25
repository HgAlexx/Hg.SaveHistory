local engine, ns = ...

Logger.Debug(engine.Name)
Logger.Debug(engine.Title)
Logger.Debug(engine.Description)

-- Setup engine
engine:AddProcessName("DOOMEternalx64vk")


-- Setup settings

local settingPlatfom = EngineSettingCombobox()
settingPlatfom.Name = "Platform"
settingPlatfom.Caption = "Platform"
settingPlatfom.Description = "Select on which platform you own DOOM Eternal"
settingPlatfom.HelpTooltip = ""
settingPlatfom.Kind = EngineSettingKind.Setup
settingPlatfom.Values:Add(1, "Steam")
settingPlatfom.Values:Add(2, "Bethesda")
settingPlatfom.Value = 1;
engine:AddSetting(settingPlatfom)


local settingFolder = EngineSettingFolderBrowser()
settingFolder.CanAutoDetect = true
settingFolder.Name = "SourceFolder"
settingFolder.Caption = "Saved Game Folder"
settingFolder.Description = "Select the root folder where the game save files are located"
settingFolder.HelpTooltip = "This must be the folder containing the GAME-AUTOSAVEX folders"
settingFolder.Kind = EngineSettingKind.Setup
settingFolder.OnAutoDetect = function()

    if settingPlatfom.Value == 1 then
        Logger.Debug("Platform is Steam")

        local path = HgSteamHelper.SteamInstallPath()

        if path ~= nil then
            path = Path.Combine(path, "userdata");
            if (not Directory.Exists(path)) then
                return "" -- steam userdata not found
            end
            
            local directoryInfo = DirectoryInfo(path)
            if directoryInfo then
                Logger.Debug("directoryInfo is ok")
            end

            -- \Steam\userdata\<steamid>\782330\remote

            local dirs = directoryInfo:GetDirectories()
            for i = 0, dirs.Length-1 do
                directory = dirs[i]
                if (string.match(directory.Name, "^([%d]+)$")) then                    
                    local finalPath = Path.Combine(directory.FullName, "782330", "remote")
                    if Directory.Exists(finalPath) then
                        return finalPath
                    end
                end
            end
        end

        return ""
    end

    if settingPlatfom.Value == 2 then
        Logger.Debug("Platform is Bethesda")

	    local varPath = '%USERPROFILE%\\Saved Games\\id Software\\DOOMEternal\\base\\savegame'
        local path = Environment.ExpandEnvironmentVariables(varPath)
	    
        Logger.Debug(path)

        if (not Directory.Exists(path)) then
            return ""
        end

        local directoryInfo = DirectoryInfo(path)
        if directoryInfo then
            Logger.Debug("directoryInfo is ok")
        end
    
        -- \Saved Games\id Sofware\DOOMEternal\base\savegame\<guid>

        local dirs = directoryInfo:GetDirectories()
        for i = 0, dirs.Length-1 do
            directory = dirs[i]
            local guid = HgConverter.StringToGuid(directory.Name)
            if guid ~= nil then
                return directory.FullName
            end
        end
    end

	return ""
end
engine:AddSetting(settingFolder)


local settingSlot = EngineSettingCombobox()
settingSlot.Name = "SlotIndex"
settingSlot.Caption = "Slot"
settingSlot.Description = "Select which slot to backup"
settingSlot.HelpTooltip = ""
settingSlot.Kind = EngineSettingKind.Setup
settingSlot.Values:Add(1, "Slot 1")
settingSlot.Values:Add(2, "Slot 2")
settingSlot.Values:Add(3, "Slot 3")
settingSlot.Value = 1;
engine:AddSetting(settingSlot)

local settingId = EngineSettingTextbox()
settingId.Name = "UserIdentifier"
settingId.Caption = "UserIdentifier"
settingId.Description = ""
settingId.HelpTooltip = ""
settingId.Kind = EngineSettingKind.Hidden
settingId.Value = "";
engine:AddSetting(settingId)


engine.OnSetupValidate = function()
    Logger.Debug("OnSetupValidate")

    -- reset user id on validate in case platform or source folder changed
    settingId.Value = ""

    local r = true
    for i = 0, engine.Settings.Count-1 do
        local s = engine.Settings[i]
        if s.Kind == EngineSettingKind.Setup then
            if s.Name == "Platform" then
                if s.Value <= 0 then
                    r = false
                end
            end
            if s.Name == "SourceFolder" then
                if s.Value == "" or (not Directory.Exists(s.Value)) then
                    r = false
                end
            end
            if s.Name == "SlotIndex" then
                if s.Value <= 0 then
                    r = false
                end
            end
        end
    end

    if r then
        local platform = settingPlatfom.Value
        local folder = settingFolder.Value

        Logger.Debug("platform=", platform)
        Logger.Debug("folder=", folder)
        
        if platform == 1 then -- steam            
            -- \Steam\userdata\<steamid>\782330\remote

            local match = string.match(folder, 'userdata\\([%d]+)\\782330')
            if match then
                Logger.Debug("Identifier=", match)
                settingId.Value = match
            end
        end

        if platform == 2 then -- bethesda
            -- \Saved Games\id Sofware\DOOMEternal\base\savegame\<guid>
            local match = string.match(folder, 'DOOMEternal\\base\\savegame\\([%d%-a-fA-F]+)')
            if match then
                Logger.Debug("Identifier=", match)
                settingId.Value = match
            end            
        end

        if settingId.Value == "" then
            return false
        end
    end

    return r
end


engine.OnSetupSuggestProfileName = function()
    if settingSlot.Value == 1 then
        return "DOOM Eternal - Slot 1"
    end
    if settingSlot.Value == 2 then
        return "DOOM Eternal - Slot 2"
    end
    if settingSlot.Value == 3 then
        return "DOOM Eternal - Slot 3"
    end
    return "DOOM Eternal"
end

-- Runtime settings

setting = EngineSettingCheckbox()
setting.Name = "IncludeDeath"
setting.Caption = "AutoBackup: Include death"
setting.Description = ""
setting.HelpTooltip = "Also do a backup on death checkpoint"
setting.Kind = EngineSettingKind.Runtime
setting.Value = false
engine:AddSetting(setting)


setting = EngineSettingCheckbox()
setting.Name = "Screenshot"
setting.Caption = "AutoBackup: Screenshot"
setting.Description = ""
setting.HelpTooltip = "Work only if the game is set to Windowed mode"
setting.Kind = EngineSettingKind.Runtime
setting.Value = false
engine:AddSetting(setting)


setting = EngineSettingCheckbox()
setting.Name = "SkipFortress"
setting.Caption = "AutoBackup: Skip fortress checkpoint"
setting.Description = ""
setting.HelpTooltip = "If checked all checkpoint when inside the fortress of Doom will be skipped"
setting.Kind = EngineSettingKind.Runtime
setting.Value = false
engine:AddSetting(setting)


-- Snapshot list definition

--[[
Predefine columns are:
- DateTime Saved At
- string Notes
--]]

local columnDiff = EngineSnapshotColumnDefinition()
columnDiff.Key = "Difficulty"
columnDiff.HeaderText = "Difficulty"
engine:AddSnapshotColumnDefinition(columnDiff)

local columnPlayed = EngineSnapshotColumnDefinition()
columnPlayed.Key = "PlayedTime"
columnPlayed.HeaderText = "Played Time"
engine:AddSnapshotColumnDefinition(columnPlayed)

local columnDeath = EngineSnapshotColumnDefinition()
columnDeath.Key = "Death"
columnDeath.HeaderText = "🕱"
engine:AddSnapshotColumnDefinition(columnDeath)
