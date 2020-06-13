using System;
using System.Threading;
using System.Collections.Generic;

using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Exceptions;
using Keyboard2XinputLib.Exceptions;

namespace Keyboard2XinputLib
{
    public class Keyboard2Xinput
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        private Dictionary<string, Xbox360Buttons> buttonsDict = new Dictionary<string, Xbox360Buttons>();
        private Dictionary<string, KeyValuePair<Xbox360Axes, short>> axesDict = new Dictionary<string, KeyValuePair<Xbox360Axes, short>>();


        private ViGEmClient client;
        private List<Xbox360Controller> controllers;
        private List<Xbox360Report> reports;
        private List<ISet<Xbox360Buttons>> pressedButtons;
        private Config config;
        private Boolean enabled = true;
        private List<StateListener> listeners;

        public Keyboard2Xinput(String mappingFile)
        {
            config = new Config(mappingFile);

            InitializeAxesDict();
            InitializeButtonsDict();
            log.Debug("initialize dicts done.");

            // start enabled?
            String startEnabledStr = config.getCurrentMapping()["startup"]["enabled"];
            // only start disabled if explicitly configured as such
            if ((startEnabledStr != null) && ("false".Equals(startEnabledStr.ToLower())))
            {
                enabled = false;
            }

            // try to init ViGEm
            try
            {
                client = new ViGEmClient();
            }
            catch (VigemBusNotFoundException e)
            {
                throw new ViGEmBusNotFoundException("ViGEm bus not found, please make sure ViGEm is correctly installed.", e);
            }
            // create pads
            controllers = new List<Xbox360Controller>(config.PadCount);
            reports = new List<Xbox360Report>(config.PadCount);
            for (int i = 1; i <= config.PadCount; i++)
            {
                Xbox360Controller controller = new Xbox360Controller(client);
                controllers.Add(controller);
                controller.FeedbackReceived +=
                    (sender, eventArgs) => Console.WriteLine(
                        $"LM: {eventArgs.LargeMotor}, " +
                        $"SM: {eventArgs.SmallMotor}, " +
                        $"LED: {eventArgs.LedNumber}");

                controller.Connect();
                reports.Add(new Xbox360Report());
                Thread.Sleep(1000);
            }
            // the pressed buttons (to avoid sending reports if the pressed buttons haven't changed)
            pressedButtons = new List<ISet<Xbox360Buttons>>(config.PadCount);
            for (int i = 0; i < config.PadCount; i++)
            {
                pressedButtons.Add(new HashSet<Xbox360Buttons>());
            }
            listeners = new List<StateListener>();
        }

        public void AddListener(StateListener listener)
        {
            listeners.Add(listener);
        }

        private void NotifyListeners(Boolean enabled)
        {
            listeners.ForEach(delegate (StateListener listener)
            {
                listener.NotifyEnabled(enabled);
            });
        }


        /// <summary>
        /// handles key events
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="vkCode"></param>
        /// <returns>1 if the event has been handled, 0 if the key was not mapped, and -1 if the exit key has been pressed</returns>
        public int keyEvent(int eventType, Keys vkCode)
        {
            int handled = 0;

            if (enabled)
            {

                for (int i = 0; i < config.PadCount; i++)
                {
                    int padNumberForDisplay = i + 1;
                    string sectionName = "pad" + (padNumberForDisplay);
                    // is the key pressed mapped to a button?
                    string mappedButton = config.getCurrentMapping()[sectionName][vkCode.ToString()];
                    if (mappedButton != null)
                    {
                        if (buttonsDict.ContainsKey(mappedButton))
                        {
                            if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                            {
                                // if we already notified the virtual pad, don't do it again
                                Xbox360Buttons pressedButton = buttonsDict[mappedButton];
                                if (pressedButtons[i].Contains(pressedButton))
                                {
                                    handled = 1;
                                    break;
                                }
                                // store the state of the button
                                pressedButtons[i].Add(pressedButton);
                                reports[i].SetButtonState(pressedButton, true);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{padNumberForDisplay} {mappedButton} down");
                                }
                            }
                            else
                            {
                                Xbox360Buttons pressedButton = buttonsDict[mappedButton];
                                reports[i].SetButtonState(pressedButton, false);
                                // remove the button state from our own set
                                pressedButtons[i].Remove(pressedButton);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{padNumberForDisplay} {mappedButton} up");
                                }
                            }
                            controllers[i].SendReport(reports[i]);
                            handled = 1;
                            break;
                        }
                        else if (axesDict.ContainsKey(mappedButton))
                        {
                            KeyValuePair<Xbox360Axes, short> axisValuePair = axesDict[mappedButton];
                            if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                            {
                                reports[i].SetAxis(axisValuePair.Key, axisValuePair.Value);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{padNumberForDisplay} {mappedButton} down");
                                }
                            }
                            else
                            {
                                reports[i].SetAxis(axisValuePair.Key, 0x0);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{padNumberForDisplay} {mappedButton} up");
                                }
                            }
                            controllers[i].SendReport(reports[i]);
                            handled = 1;
                            break;
                        }

                    }
                }
            }
            if (handled == 0)
            {
                // handle the enable toggle key even if disabled (otherwise there's not much point to it...)
                string enableButton = config.getCurrentMapping()["config"][vkCode.ToString()];
                if ("enableToggle".Equals(enableButton))
                {
                    if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                    {
                        ToggleEnabled();
                        if (log.IsInfoEnabled)
                        {
                            log.Info($"enableToggle down; enabled={enabled}");
                        }
                    }
                    handled = 1;
                }
                else if ("enable".Equals(enableButton))
                {
                    if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                    {
                        Enable();
                        if (log.IsInfoEnabled)
                        {
                            log.Info($"enable down; enabled={enabled}");
                        }
                    }
                    handled = 1;
                }
                else if ("disable".Equals(enableButton))
                {
                    if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                    {
                        Disable();
                        if (log.IsInfoEnabled)
                        {
                            log.Info($"disable down; enabled={enabled}");
                        }
                    }
                    handled = 1;
                }
                // key that exits the software
                string configButton = config.getCurrentMapping()["config"][vkCode.ToString()];
                if ("exit".Equals(configButton))
                {
                    handled = -1;
                }
                else if ((configButton != null) && configButton.StartsWith("config"))
                {
                    if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                    {
                        int index = Int32.Parse(configButton.Substring(configButton.Length - 1));
                        if (log.IsInfoEnabled)
                        {
                            log.Info($"Switching to mapping {index}");
                        }
                        config.CurrentMappingIndex = index;
                    }
                    handled = 1;
                }
            }

            if (handled == 0 && enabled && log.IsWarnEnabled)
            {
                log.Warn($"unmapped button {vkCode.ToString()}");
            }

            return handled;
        }
        private void InitializeAxesDict()
        {
            // a bit weird: left& right thumb axes max values are 0x7530 (max short value), but left & right triggers max value are 0xFF
            short triggerValue = 0xFF;
            short posAxisValue = 0x7530;
            short negAxisValue = -0x7530;
            axesDict.Add("LT", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.LeftTrigger, triggerValue));
            axesDict.Add("RT", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.RightTrigger, triggerValue));
            axesDict.Add("LLEFT", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.LeftThumbX, negAxisValue));
            axesDict.Add("LRIGHT", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.LeftThumbX, posAxisValue));
            axesDict.Add("LUP", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.LeftThumbY, posAxisValue));
            axesDict.Add("LDOWN", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.LeftThumbY, negAxisValue));
            axesDict.Add("RLEFT", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.RightThumbX, negAxisValue));
            axesDict.Add("RRIGHT", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.RightThumbX, posAxisValue));
            axesDict.Add("RUP", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.RightThumbY, posAxisValue));
            axesDict.Add("RDOWN", new KeyValuePair<Xbox360Axes, short>(Xbox360Axes.RightThumbY, negAxisValue));

        }

        private void InitializeButtonsDict()
        {
            buttonsDict.Add("UP", Xbox360Buttons.Up);
            buttonsDict.Add("DOWN", Xbox360Buttons.Down);
            buttonsDict.Add("LEFT", Xbox360Buttons.Left);
            buttonsDict.Add("RIGHT", Xbox360Buttons.Right);
            buttonsDict.Add("A", Xbox360Buttons.A);
            buttonsDict.Add("B", Xbox360Buttons.B);
            buttonsDict.Add("X", Xbox360Buttons.X);
            buttonsDict.Add("Y", Xbox360Buttons.Y);
            buttonsDict.Add("START", Xbox360Buttons.Start);
            buttonsDict.Add("BACK", Xbox360Buttons.Back);
            buttonsDict.Add("GUIDE", Xbox360Buttons.Guide);
            buttonsDict.Add("LB", Xbox360Buttons.LeftShoulder);
            buttonsDict.Add("LTB", Xbox360Buttons.LeftThumb);
            buttonsDict.Add("RB", Xbox360Buttons.RightShoulder);
            buttonsDict.Add("RTB", Xbox360Buttons.RightThumb);

        }
        public void Close()
        {
            log.Info("Closing");
            foreach (Xbox360Controller controller in controllers)
            {
                log.Debug($"Disconnecting {controller.ToString()}");
                controller.Disconnect();
            }
            log.Debug("Disposing of ViGEm client");
            client.Dispose();
        }
        public void Enable()
        {
            enabled = true;
            NotifyListeners(enabled);
        }
        public void Disable()
        {
            enabled = false;
            NotifyListeners(enabled);
        }
        public void ToggleEnabled()
        {
            enabled = !enabled;
            NotifyListeners(enabled);
        }
        public Boolean IsEnabled()
        {
            return enabled;
        }
    }
}
