#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

Run, "E:\bartop\software\keyboard2Xinput\keyboard2XinputGui.exe"
Run, "C:\Program Files (x86)\Steam\Steam.exe" -applaunch %1%

isRunning := "0"
While (isRunning = "0") ; Wait until the game is launched
    RegRead, isRunning, HKCU\Software\Valve\Steam\Apps\%1%, Running
	Sleep, 500

Sleep, 5000
MouseMove, 1920, 1200
WinGetTitle, Title

While (isRunning = "1") ; Wait until the game is closed
    RegRead, isRunning, HKCU\Software\Valve\Steam\Apps\%1%, Running
	Sleep, 500

; Game stopped, stop keyboard2Xinput by sending numpad multiply key (see k2x mapping.ini)
SendInput {NumpadMult}

; change focus back to Attract-Mode
WinActivate Attract-Mode
