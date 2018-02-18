#Keyboard2Xinput
Creates virtual Xbox 360 gamepads backed by keyboard inputs.

This project was created to fill a very specific need: using an I-PAC (or keyboard) to play recent Windows games that only accept XInput as input and/or do not allow 2 players on the keyboard.

##Requirements
The awesome ViGEm Bus Driver (https://github.com/ViGEm/ViGEmBus). Follow installation instructions carefully.

##Installation
Just unzip & launch Keyboard2XinputGui.exe

##Configuration
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

This project uses:
 * ViGEm.NET by Nefarius (https://github.com/ViGEm/ViGEm.NET)
 * ini-parser from rickyah (https://github.com/rickyah/ini-parser/tree/master)
