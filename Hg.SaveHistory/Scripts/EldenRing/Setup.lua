local engine, ns = ...

Logger.Information("Engine.Name=",engine.Name,", Engine.Title=",engine.Title)


-- Setup engine

-- client process
engine:AddProcessName("eldenring")


-- Setup settings

local setting = EngineSettingFolderBrowser()
setting.CanAutoDetect = true
setting.Name = "SourceFolder"
setting.Caption = "Saved Game Folder"
setting.Description = "Select the root folder where the game save files are located"
setting.HelpTooltip = "This must be the folder containing the .sl2 file"
setting.Kind = EngineSettingKind.Setup
setting.OnAutoDetect = function()
    local s = '%appdata%\\EldenRing'
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
        if (string.match(directory.Name, "^([%d]+)$")) then -- match steam id
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
        end
    end
    return r
end


engine.OnSetupSuggestProfileName = function()
    return "Elden Ring"
end


-- Runtime settings

setting = EngineSettingCheckbox()
setting.Name = "Screenshot"
setting.Caption = "AutoBackup: Screenshot"
setting.Description = ""
setting.HelpTooltip = "Take a Screenshot on auto backup"
setting.Kind = EngineSettingKind.Runtime
setting.Value = true
engine:AddSetting(setting)


-- Snapshot list definition

--[[
Predefine columns are:
- DateTime Saved At
- string Notes
--]]

--[[
local columnPlayed = EngineSnapshotColumnDefinition()
columnPlayed.Key = "PlayedTime"
columnPlayed.HeaderText = "Played Time"
engine:AddSnapshotColumnDefinition(columnPlayed)
--]]
