# Keyboard2Xinput
Creates virtual Xbox 360 gamepads backed by keyboard inputs.

This project was created to fill a very specific need: using an I-PAC (or keyboard) to play recent Windows games that only accept XInput as input and/or do not allow 2 players on the keyboard.

Has been successfully tested with Astebreed, BlazBlue: Calamity Trigger, BlazBlue: Chronophantasma Extend, BlazBlue: Continuum Shift Extend, Broforce, Darius Burst Chronicle Saviours, DoDonPachi Resurrection, Mortal Kombat X, PAC-MAN Championship Edition DX+, Raiden IV: OverKill, Street Fighter V, The Bug Butcher, Ultimate Marvel vs. Capcom 3.

## Requirements
The awesome ViGEm Bus Driver (https://github.com/ViGEm/ViGEmBus). Follow installation instructions carefully (https://docs.vigem.org/#!vigem-bus-driver-installation.md).

## Download & Installation
Download the latest zip from https://gitlab.com/SchwingSK/Keyboard2Xinput/tags

Make sure that ViGEm Bus Driver is installed (see Requirements).

Unzip & launch Keyboard2XinputGui.exe

## Configuration
Example mapping.ini file:
```ini
[config]
Subtract = enableToggle
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
The [config] section defines the key (here the minus key from the keypad) that toggles the interception of keys. Can be handy if you have a real keyboard connected and want to disable momentarily the interception, without quitting your game. This does NOT disconnect the gamepads.

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
LT    | LeftTrigger
RT    | RightTrigger
LX    | LeftThumbX
LY    | LeftThumbY
RX    | RightThumbX
RY    | RightThumbY

## Usage
Run Keyboard2XinputGui.exe. An icon should appear in the Windows System tray; There is no main window. By right-clicking the icon, you can toggle key interception, see the about box and exit the program.

By default, the program looks for a file named *mapping.ini* in the same folder. This behavior can be chnaged by giving the path of your mapping file to the programe as a parameter.

## Troubleshooting
A file named k2x.log should be created each the program runs. It contains detailed information on the keys pressed (even if they're not mapped).

## Building
You need Microsoft Visual Studio 2017 (Community edition is ok, that's what I'm using).

## Known bugs/limitations
 * Return and Enter keys both respond to the 'Return' Virtual Key name.

This project uses:
 * ViGEm.NET by Nefarius (https://github.com/ViGEm/ViGEm.NET)
 * ini-parser from rickyah (https://github.com/rickyah/ini-parser/tree/master)
 * Logo hacked together from two artworks from ikillyou121 (https://www.deviantart.com/ikillyou121/art/Keyboard-Vector-555191125 and https://www.deviantart.com/ikillyou121/art/Old-Xbox-360-Controller-Cutie-Mark-467878925)
