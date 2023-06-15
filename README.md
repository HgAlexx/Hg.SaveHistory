# Hg.SaveHistory

This tool helps generate snapshots of game save files, or any files for that matter.


---

If you encounter any issue please report them!

You'll find a log file named Hg.SaveHistory.log inside the application folder.

Thank you!

---


## Features

- Global Hot keys
- Manual backup
- Manual restore
- Manual archive (zip)
- Manual delete
- Automatic backup (if supported by engine)
- Can take screenshot on automatic backup (if supported by engine)
- Events Sound notifications
- Attach notes to snapshots
- Automatic snaphots cleanup


## Supported Games and Applications

### Official Engines

#### Games
- DOOM 2016
- DOOM Eternal, with DLC1 and DLC2
- Satisfactory (client and server)
- Elden Ring

#### Applications
- None yet

### Third Party Engines
- None yet


## Installation

Just unzip and run executable!

(or you can download the source, examine and compile it yourself)

## Setup

### Global settings

On first run, the app will ask where you want to save the app settings, either:
- Default: In windows default location for app settings: %LOCALAPPDATA%\Hg\Hg.SaveHistory
- Portable: In the folder app, next to the executable file

#### You're done!


## Usage

### Create a backup profile

Start by creating a backup profile using the built-in wizard.

1. Choose a engine
2. Setup the profile: each engine requires its own settings
3. Choose a name and a location to save the profile
4. Be sure to read the engine README if one is provided!
5. You are done!


### Open an existing backup profile

Click "Open a backup profile" and choose a .shp file previously created by the wizard.


### Pinned & Recent backup profiles

On the home page you will see:
- Recently opened backup profiles
- Pinned backup profiles

To pin a profile, click the pin icon in the recent profiles list.

To unpin a profile, click the trash can icon.


### Menu
#### File
- "New Profile": Create a new profile
- "Open Profile": Open an existing profile
- "Close Profile": Close the active profile
- "Exit": close the application
#### Settings
- "Hot keys":
  - "Enabled": Active or deactive the hot keys
  - "Sound": Play a sound on successful or failed action
  - "Assign hot keys": Open the hot keys settings window
- "Notification mode": this allow you to choose between messagebox or statusbar notifications
- "Screenshot quality": choose between Png, Jpg and Gif
- "Snapshots"
  - "Auto backup sound notification": play a sound on successful auto backup
  - "Auto select last backup": Switch to Active tab and highlight last snapshot on auto backup
  - "Highlight selected": Highlight select snapshot when app is not focused
  - "Choose highlight color": open the color selection window
- "Others"
  - "Save window size and position"
  - "Snap to screen edges"
- "Clear settings": this will reset all global settings
#### Profile
- "Auto cleanup snapshots":
  - "Enabled": Active or deactive auto cleanup of snapshots
  - "Configure": Open the auto cleanup settings window
- "Show nuked snapshots": Open nuked snapshots window which allow to forget them
#### Tools
- "Open debug console": for debug purpose only
#### Help
- "Check for update": this will check online for new release
- "About": Provide cuteness, and also some information about the application

## F.A.Q.

1. Sometimes the Archive/Active/Nuke action fails

This can happens if the target folder is currently open in a windows explorer (even if not selected) or if you don't have write persmission to the destination.

This may be the case if steam is installed inside "Program Files". You can either move your steam installation folder or launch SaveHistory as administrator.


2. Screenshots are not working

Sadly I didn't found a way to take screenshot of vulkan fullscreen games, so for these only windowed will work, or using opengl.

This apply to Doom 2016 and Doom Eternal.


## TODO:

- Write guide and API documentation for the creation of third party engines



## About Hg.SaveHistory

This tool is the second iteration of the same idea. It is based on [Hg.DoomHistory](https://github.com/HgAlexx/Hg.DoomHistory/) which had most of the same features but was only working with Doom 2016.

Now the tool support a script system, using lua, which allow to add support for any game and any application!

Alpha testers:

- [RedW4rr10r](https://www.twitch.tv/redw4rr10r): The first bug slayer

- [ByteMe](https://www.twitch.tv/byteme): The suggestion slayer

- [IAmAllstin](https://www.twitch.tv/iamallstin): The origin slayer


# Version History


## v0.8.0

- \+ Added Auto Cleanup Snapshots feature
- \+ Added window to forget Nuked snapshots
- \+ Remember last selected category per profile
- \* Updated NuGet packages
- \* Other small improvements & fixes

## v0.7.6

- \+ Added support for Elden ring
- \* Updated NuGet packages
- \* Other small improvements

## v0.7.5

- \* Updated NuGet packages
- \* DOOM Eternal: remove filter on "-BACKUP" file
- \* Satisfactory: fix issue when more than 3 autosaves are kept (server)

## v0.7.4

- \* Satisfactory: add small sleep on autobackup event before processing the file
- \* Fix ByteMe name and stream link in About box

## v0.7.3

- \* Fix issue #2: Unable to change screenshot settings
- \+ Add support for Satisfactory dedicated server

## v0.7.2

- \* Remember window size and location
- \* Window now snap to edge of screen
- \* Small improvement on Satisfactory script
- \* Code clean up

## v0.7.1

- \+ Add README feature
- \+ Add README content for all 3 engines
- \* Update NuGet packages
- \* Small fixes

## v0.7.0

- \* Add support for Satisfactory and DOOM Eternal DLC2
- \* Add wait cursor on manual backup
- \* Fix snapshots sorting
- \* Screenshot only if the process window is focused
- \* Tests: refactor to support Satisfactory cases
- \* Typo

## v0.6.0

- \* Add DLC support (new maps)
- \* Add more visual feedback for autobackup status
- \* Few other fixes and improvements here and there

## v0.5.0

- \* First public beta release
