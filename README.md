# Hg.SaveHistory

This tool helps generate snapshots of game save files, or any files for that matter.


---

### WARNING: Beta version 0.5.0

SaveHistory is currently in Open Beta, if you encounter any issue please report them.

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

## Supported Games and Applications

### Official Engines

- DOOM 2016
- DOOM Eternal


### Third Party Engines

- None yet


## Installation

Just unzip and run executable!

(or you can download the source code, examine and compile it yourself)

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
2. Setup the profile: each engine require its own settings
3. Choose a name and a location to save the profile
4. You are done!


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
  - "Highlight selected": Highlight select snapshot when app is not focus
  - "Choose highlight color": open the color selection window
- "Clear settings": this will reset all global settings
#### Tools
- "Open debug console": for debug purpose only
#### Help
- "Check for update": this will check online for new release
- "About": Provide cuteness, and also some information about the application

## F.A.Q.

1. Sometimes the Archive/Active/Nuke action fails

This can happens if the target folder is currently open in a windows explorer (even if not selected) or if you don't have write persmission to the destination.

This may be the case if steam is installed inside "Program Files". You can either move you're steam installation folder or launch SaveHistory as administrator.


## TODO:

- Write guide and API documentation for the creation of third party engines



## About Hg.SaveHistory

This tool is the second iteration of the same idea. It is based on Hg.DoomHistory which had most of the same features but was only working with Doom 2016.

Now the tool support a script system, using lua, which allow to add support for any game and any application!

Alpha testers:

- [RedW4rr10r](https://www.twitch.tv/redw4rr10r): The first bug slayer

- [x_ByteMe_x](https://www.twitch.tv/x_byteme_x): The suggestion slayer

- [IAmAllstin](https://www.twitch.tv/iamallstin): The origin slayer



## About [Hg.DoomHistory](https://github.com/HgAlexx/Hg.DoomHistory/)

I got the idea while watching [IAmAllstin](https://www.twitch.tv/iamallstin) streaming doom. He was spending a lot of time copy/pasting save files to be able to pratice part of the game for his Ultra-Nightmare run.

Since I was also planning to do Doom on Ultra-Nightmare, I was going to need a way to back my save files too.

The first beta tester was [RedW4rr10r](https://www.twitch.tv/redw4rr10r) who found the first bug and made good improvements suggestions.

[x_ByteMe_x](https://www.twitch.tv/x_byteme_x) who also made good improvements suggestions (global hot keys).


# Version History

## v0.6.0

- \* Add DLC support (new maps)
- \* Add more visual feedback for autobackup status
- \* Few other fixes and improvements here and there

## v0.5.0

- \* First public beta release
