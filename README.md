# Keyboard2Xinput
Creates virtual Xbox 360 gamepads backed by keyboard inputs.

This project was created to fill a very specific need: using an I-PAC (or keyboard) to play recent Windows games that only accept XInput as input and/or do not allow 2 players on the keyboard.

Has been successfully tested with Astebreed, BlazBlue: Calamity Trigger, BlazBlue: Chronophantasma Extend, BlazBlue: Continuum Shift Extend, Broforce, Darius Burst Chronicle Saviours, DoDonPachi Resurrection, Mortal Kombat X, PAC-MAN Championship Edition DX+, Raiden IV: OverKill, Street Fighter V, The Bug Butcher, Ultimate Marvel vs. Capcom 3.

## Requirements
* The awesome ViGEm Bus Driver (https://github.com/ViGEm/ViGEmBus). Follow installation instructions carefully if you're on Windows 7 (https://github.com/ViGEm/ViGEmBus/releases).
* Microsoft Visual C++ Redistributable for Visual Studio 2015 **32bit**

  Be sure to download & install the 32 bit version!

## Download & Installation
Download the latest zip from https://gitlab.com/SchwingSK/Keyboard2Xinput/-/tags

Make sure that ViGEm Bus Driver is installed (see Requirements).

Unzip & launch Keyboard2XinputGui.exe

## Configuration
Example mapping.ini file:
```ini
[startup]
enabled = true
[config]
Subtract = enableToggle
Multiply = exit
[pad1]
Up = UP
Down = DOWN
Left = LEFT
Right = RIGHT
LControlKey = A
LMenu = B
LShiftKey = X
W = Y
X = RB
Space = LB
V = RT
C = LT
D5 = START
D1 = BACK

[pad2]
R = UP
F = DOWN
D = LEFT
G = RIGHT
S = B
Q = A
I = Y
Z = X
K = RB
A = LB
L = RT
J = LT
D6 = START
D2 = BACK
```
The [startup] section defines:
- enabled : if true, the pads are created and keys will be intercepted. This was the behavior before 1.2.0, and is still the default behavior if this section is not defined.

    If false, the pads are created but keys will not be intercepted.

The [config] section defines:
- enableToggle : the key (here the minus key from the keypad) that toggles the interception of keys. Can be handy if you have a real keyboard connected and want to disable momentarily the interception, without quitting your game. This does NOT disconnect the gamepads.
- enable : the key that enables key interception.
- disable : the key that disables key interception.
- exit : the key (here the multiply key from the keypad) that exits the program. This has been added because I use AutoHotKey to launch & kill Keyboard2Xinput, and could not figure out how to catch the exit process event (if there's one) triggered by AHK. While exiting by killing the process does work, it leaves the notification icon lingering until the mouse hovers over it. Having an exit key resolves this problem.
- config0, config1, etc... allows to switch between mutiple mapping files. When using this, config0 points to you main mapping file, config1 points to a file named *mapping1.ini*, located in the same folder as your main mapping file; config2 points to *mapping2.ini*, and so on.
   
  When using multiple mapping files, **only** the main mapping file can contain [startup] and [config] sections (otherwise the key for changing mappings could vary according to the selected mapping, and it would be a configuration  (and troubleshooting) nightmare). 
  Look in [samples/mappings/Multiple](samples/mappings/Multiple) for  example files.


Each [pad*n*] section defines a configuration for a pad. Each maps a key to a button/axis of the Xbox 360 gamepad. The keys and values are **case-sensitive**.

### keys reference:
The list of keys is taken from .net's enum `System.Windows.Forms.Keys`. See [virtual key names](virtualKeyNames.md) for the complete list of mappable keyboard keys.

### buttons/axes reference:
All buttons and axes must be in uppercase
#### Buttons:
value | description
----- | ------
UP    | Up
DOWN  | Down
LEFT  | Left
RIGHT | Right
A     | A
B     | B
X     | X
Y     | Y
START | Start
BACK  | Back
GUIDE | Guide
LB    | Left Shoulder Button
LTB   | Left Thumb Button
RB    | Right Shoulder Button
RTB   | Right Thumb Button

#### Axes:
value | description
----- | ------
LT    | Left Trigger
RT    | Right Trigger
LUP   | Left Thumb Stick Up
LDOWN | Left Thumb Stick Down
LLEFT | Left Thumb Stick Left
LRIGHT| Left Thumb Stick Right
RUP   | Right Thumb Stick Up
RDOWN | Right Thumb Stick Down
RLEFT | Right Thumb Stick Left
RRIGHT| Right Thumb Stick Right

## Usage
Run Keyboard2XinputGui.exe. An icon should appear in the Windows System tray; There is no main window. By right-clicking the icon, you can toggle key interception, see the about box and exit the program.

By default, the program looks for a file named *mapping.ini* in the same folder. This behavior can be changed by giving the path of your mapping file to the program as a parameter.

## Examples
### Mappings
You can find some reference mappings for IPAC2 & IPAC4 in [samples/mappings](samples/mappings) 
### AutoHotKey scripts
#### Steam
See script [samples/AHKscripts/steamXinput.ahk](samples/AHKscripts/steamXinput.ahk).

To run Steam games, you need to have their appId (see [https://steamdb.info/apps/](https://steamdb.info/apps/) to find you games appIds). Unfortunatley, steam doesn't return the handle of the game, so you can't use the classic AHK approach to monitor the game status. The best approach I've found (yay internet!) is to look in the Windows Registry, as Steam keeps each game status in there.

So basically this script:
- Runs k2xi
- Runs the game via Steam by passing the game's AppId to Steam.exe
- Waits until the game is launched by monitoring the Windows registry
- Moves the mouse out of the way, because some games don't hide the mouse cursor
- Waits until the game is closed by monitoring the Windows registry
- Closes k2xi by pushing the key that's mapped to 'exit'
- Gives back focus to Attract-Mode (change or disable if you don't use this Front-end)

The resulting executable takes the appId as a parameter.
For some old games that only detect gamepads when starting, you can add a sleep between the launch of kb2xi and the game.


## Troubleshooting
A file named k2x.log should be created each time the program runs. It contains detailed information on the keys pressed (even if they're not mapped).
You can hop in [ArcadeControls.com forums](http://forum.arcadecontrols.com/index.php/topic,158047.0.html) if you run into problems, I'll do my best to help you.

### The client launches but closes shortly after
If you have the following error in k2x.log:

    2019-08-19 17:22:09,557 [1] FATAL Keyboard2XinputGui.Program- System.DllNotFoundException: Unable to load DLL 'vigemclient.dll': The specified module could not be found. (Exception from HRESULT: 0x8007007E)
    at Nefarius.ViGEm.Client.ViGEmClient.vigem_alloc()
    at Nefarius.ViGEm.Client.ViGEmClient..ctor()
    at Keyboard2XinputLib.Keyboard2Xinput..ctor(String mappingFile)
    at Keyboard2XinputGui.Keyboard2XinputGui.InitK2x()
    at Keyboard2XinputGui.Keyboard2XinputGui..ctor(String aMappingFile)
    at Keyboard2XinputGui.Program.Main(String[] args)

it probably means that you haven't installed the Visual C++ Redistributable for Visual Studio 2015 **32bit**, please install it.

### The buttons work but not the joystick
The default configuration maps the D-PAD to the joysticks. If your game expects that you use the analog left stick (Gauntlet Slayer Edition for instance), then you have to use a mapping that reflects that:

```ini
(...)
[pad1]
Up = LUP
Down = LDOWN
Left = LLEFT
Right = LRIGHT
(...)
```

### Some joysticks are not detected
If you launch kb2xi and immediately after your game, some of the configured gamepads may not have been 'plugged in' when the game starts. Some old games only detect joysticks when they start, so you have to either:
- start kb2xi, wait a number of seconds (depends on your computer speed), then launch your game
- start kb2xi when windows starts (with the gamepads disabled), and use the kb2xi hotkeys that allow you to enable/disable keys interception. That way the gamepads are always 'plugged in' but the keys are only redirected to them when you want to.


## Building
You need Microsoft Visual Studio 2017 (Community edition is ok, that's what I'm using).

## Known bugs/limitations
 * Return and Enter keys both respond to the 'Return' Virtual Key name.

## Credits
This project uses:
 * ViGEm.NET by Nefarius (https://github.com/ViGEm/ViGEm.NET)
 * ini-parser from rickyah (https://github.com/rickyah/ini-parser/tree/master)
 * Logo hacked together from two artworks from ikillyou121 (https://www.deviantart.com/ikillyou121/art/Keyboard-Vector-555191125 and https://www.deviantart.com/ikillyou121/art/Old-Xbox-360-Controller-Cutie-Mark-467878925)
 * Mistune for offline documentation rendering (https://mistune.readthedocs.io/en/v0.8.4/)
 * GitHub-like CSS for offline documentation rendering: https://gist.github.com/dashed/6714393