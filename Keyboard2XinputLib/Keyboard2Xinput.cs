using System;
using System.Collections.Generic;

using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Keyboard2XinputLib
{
    public class Keyboard2Xinput
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        private Dictionary<string, Xbox360Buttons> buttonsDict = new Dictionary<string, Xbox360Buttons>();
        private Dictionary<string, Xbox360Axes> axesDict = new Dictionary<string, Xbox360Axes>();


        private ViGEmClient client;
        private List<Xbox360Controller> controllers;
        private List<Xbox360Report> reports;
        private Config config;
        private Boolean enabled = true;

        public Keyboard2Xinput(String mappingFile)
        {
            config = new Config(mappingFile);

            InitializeAxesDict();
            InitializeButtonsDict();
            log.Debug("initialize dicts done.");


            // create pads
            client = new ViGEmClient();
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

            }

        }


        public Boolean keyEvent(int eventType, Keys vkCode)
        {
            Boolean handled = false;

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
                            handled = true;
                        }
                        else if (axesDict.ContainsKey(mappedButton))
                        {
                            if ((eventType == WM_KEYDOWN) || (eventType == WM_SYSKEYDOWN))
                            {
                                reports[i - 1].SetAxis(axesDict[mappedButton], 0xFF);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{i} {mappedButton} down");
                                }
                            }
                            else
                            {
                                reports[i - 1].SetAxis(axesDict[mappedButton], 0x0);
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug($"pad{i} {mappedButton} up");
                                }
                            }
                            controllers[i - 1].SendReport(reports[i - 1]);
                            handled = true;
                        }
                        else
                        {
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
                handled = true;
            }
            if (!handled && enabled && log.IsDebugEnabled)
            {
                log.Debug($"unmapped button {vkCode.ToString()}");
            }

            return handled;
        }
        private void InitializeAxesDict()
        {
            axesDict.Add("LT", Xbox360Axes.LeftTrigger);
            axesDict.Add("RT", Xbox360Axes.RightTrigger);
            axesDict.Add("LX", Xbox360Axes.LeftThumbX);
            axesDict.Add("LY", Xbox360Axes.LeftThumbY);
            axesDict.Add("RX", Xbox360Axes.RightThumbX);
            axesDict.Add("RY", Xbox360Axes.RightThumbY);

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
        }
        public void Disable()
        {
            enabled = false;
        }
        public void ToggleEnabled()
        {
            enabled = !enabled;
        }
        public Boolean IsEnabled()
        {
            return enabled;
        }
    }
}
