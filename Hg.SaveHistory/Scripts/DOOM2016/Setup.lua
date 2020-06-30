local engine, ns = ...

Logger.Debug(engine.Name)
Logger.Debug(engine.Title)
Logger.Debug(engine.Description)

-- Setup engine
engine:AddProcessName("DOOMx64")
engine:AddProcessName("DOOMx64vk")


-- Setup settings

local setting = EngineSettingFolderBrowser()
setting.CanAutoDetect = true
setting.Name = "SourceFolder"
setting.Caption = "Saved Game Folder"
setting.Description = "Select the root folder where the game save files are located"
setting.HelpTooltip = "This must be the folder containing the GAME-AUTOSAVEX folders"
setting.Kind = EngineSettingKind.Setup
setting.OnAutoDetect = function()
    local s = '%USERPROFILE%\\Saved Games\\id Software\\DOOM\\base\\savegame.user'
    local path = Environment.ExpandEnvironmentVariables(s)
    Logger.Debug(path)

    if (not Directory.Exists(path)) then
        return ""
    end

    local directoryInfo = DirectoryInfo(path)
    if directoryInfo then
        Logger.Debug("directoryInfo is ok")
    end

    local dirs = directoryInfo:GetDirectories()

    for i = 0, dirs.Length-1 do
        directory = dirs[i]
        if (string.match(directory.Name, "^([%d]+)$")) then
            directoryInfo = nil
            local fullname = directory.FullName
            directory = nil
            return fullname
        end
        directory = nil
    end

    directoryInfo = nil
    return ""
end
engine:AddSetting(setting)


local settingSlot = EngineSettingCombobox()
settingSlot.Name = "SlotIndex"
settingSlot.Caption = "Slot"
settingSlot.Description = "Select which slot to backup"
settingSlot.HelpTooltip = ""
settingSlot.Kind = EngineSettingKind.Setup
settingSlot.Values:Add(1, "Slot 1")
settingSlot.Values:Add(2, "Slot 2")
settingSlot.Values:Add(3, "Slot 3")
settingSlot.Value = 1
engine:AddSetting(settingSlot)


engine.OnSetupValidate = function()
    local r = true
    for i = 0, engine.Settings.Count-1 do
        local s = engine.Settings[i]
        if s.Kind == EngineSettingKind.Setup then
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
    return r
end


engine.OnSetupSuggestProfileName = function()
    if settingSlot.Value == 1 then
        return "DOOM 2016 - Slot 1"
    end
    if settingSlot.Value == 2 then
        return "DOOM 2016 - Slot 2"
    end
    if settingSlot.Value == 3 then
        return "DOOM 2016 - Slot 3"
    end
    return "DOOM 2016"
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
setting.HelpTooltip = "Work only if the game is set to OpenGL or Windowed mode"
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
