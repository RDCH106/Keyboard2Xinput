# Keyboard2Xinput
Creates virtual Xbox 360 gamepads backed by keyboard inputs.

This project was created to fill a very specific need: using an I-PAC (or keyboard) to play recent Windows games that only accept XInput as input and/or do not allow 2 players on the keyboard.
Has been successfully tested with Astebreed, BlazBlue: Calamity Trigger, BlazBlue: Chronophantasma Extend, BlazBlue: Continuum Shift Extend, Broforce, Darius Burst Chronicle Saviours, DoDonPachi Resurrection, Mortal Kombat X, PAC-MAN Championship Edition DX+, Raiden IV: OverKill, Street Fighter V, The Bug Butcher, Ultimate Marvel vs. Capcom 3. 

## Requirements
The awesome ViGEm Bus Driver (https://github.com/ViGEm/ViGEmBus). Follow installation instructions carefully.

## Installation
Just unzip & launch Keyboard2XinputGui.exe

## Configuration
Example mapping.ini file:
```ini
[pad1]
Up = UP
Down = DOWN
Left = LEFT
Right = RIGHT
LControlKey = B
LMenu = A
LShiftKey = Y
W = X
Space = RB
X = LB
C = RT
V = LT

[pad2]
R = UP
F = DOWN
D = LEFT
G = RIGHT
Q = B
S = A
Z = Y
I = X
A = RB
K = LB
J = RT
L = LT
```

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

## Known bugs/limitations
 * Return and Enter keys both respond to the 'Return' Virtual Key name.
 * Thumb axes do not work 

This project uses:
 * ViGEm.NET by Nefarius (https://github.com/ViGEm/ViGEm.NET)
 * ini-parser from rickyah (https://github.com/rickyah/ini-parser/tree/master)
 * Logo hacked together from two artworks from ikillyou121 (https://www.deviantart.com/ikillyou121/art/Keyboard-Vector-555191125 and https://www.deviantart.com/ikillyou121/art/Old-Xbox-360-Controller-Cutie-Mark-467878925)
