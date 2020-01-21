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
            String startEnabledStr = config.mapping["startup"]["enabled"];
            // only start disabled if explicitly configured as such
            if ((startEnabledStr != null) && ("false".Equals(startEnabledStr.ToLower()))) {
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
            controllers = new List<Xbox360Controller>(config.padCount);
            reports = new List<Xbox360Report>(config.padCount);
            for (int i = 1; i <= config.padCount; i++)
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
        /// andles key events
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="vkCode"></param>
        /// <returns>1 if the event has been handled, 0 if the key was mapped, and -1 if the exit key has been pressed</returns>
        public int keyEvent(int eventType, Keys vkCode)
        {
            int handled = 0;

            if (enabled)
            {

                for (int i = 1; i <= config.padCount; i++)
                {
                    string sectionName = "pad" + (i);
                    // is the key pressed mapped to a button?
                    string mappedButton = config.mapping[sectionName][vkCode.ToString()];
                    if (mappedButton != null)
                    {
                        if (buttonsDict.ContainsKey(mappedButton))
                        {
                            if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                            {
                                reports[i - 1].SetButtonState(buttonsDict[mappedButton], true);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{i} {mappedButton} down");
                                }
                            }
                            else
                            {
                                reports[i - 1].SetButtonState(buttonsDict[mappedButton], false);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{i} {mappedButton} up");
                                }
                            }
                            controllers[i - 1].SendReport(reports[i - 1]);
                            handled = 1;
                        }
                        else if (axesDict.ContainsKey(mappedButton))
                        {
                            KeyValuePair<Xbox360Axes, short> axisValuePair = axesDict[mappedButton];
                            if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                            {
                                reports[i - 1].SetAxis(axisValuePair.Key, axisValuePair.Value);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{i} {mappedButton} down");
                                }
                            }
                            else
                            {
                                reports[i - 1].SetAxis(axisValuePair.Key, 0x0);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{i} {mappedButton} up");
                                }
                            }
                            controllers[i - 1].SendReport(reports[i - 1]);
                            handled = 1;
                        }

                    }
                }
            }
            // handle the enable toggle key even if disabled (otherwise there's not much point to it...)
            string enableButton = config.mapping["config"][vkCode.ToString()];
            if ("enableToggle".Equals(enableButton))
            {
                if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                {
                    ToggleEnabled();
                    if (log.IsDebugEnabled)
                    {
                        log.Debug($"enableToggle down; enabled={enabled}");
                    }
                }
                handled = 1;
            } else  if ("enable".Equals(enableButton))
            {
                if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                {
                    Enable();
                    if (log.IsDebugEnabled)
                    {
                        log.Debug($"enable down; enabled={enabled}");
                    }
                }
                handled = 1;
            }
            else if ("disable".Equals(enableButton))
            {
                if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                {
                    Disable();
                    if (log.IsDebugEnabled)
                    {
                        log.Debug($"disable down; enabled={enabled}");
                    }
                }
                handled = 1;
            }
            // key that exits the software
            string exitButton = config.mapping["config"][vkCode.ToString()];
            if ("exit".Equals(exitButton))
            {

                handled = -1;
            }
            if (handled == 0 && enabled && log.IsDebugEnabled)
            {
                log.Debug($"unmapped button {vkCode.ToString()}");
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
            log.Debug("Closing");
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
