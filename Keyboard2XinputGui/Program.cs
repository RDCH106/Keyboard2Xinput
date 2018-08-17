using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keyboard2XinputGui
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Keyboard2XinputGui gui;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            // prevent multiple instances: code from https://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c/229567
            // get application GUID as defined in AssemblyInfo.cs
            string appGuid =
                ((GuidAttribute)Assembly.GetExecutingAssembly().
                    GetCustomAttributes(typeof(GuidAttribute), false).
                        GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);


            // Need a place to store a return value in Mutex() constructor call
            bool createdNew;

            // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
            // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule =
                new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid
                                                           , null)
                                   , MutexRights.FullControl
                                   , AccessControlType.Allow
                                   );
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            // edited by MasonGZhwiti to prevent race condition on security settings via VanNguyen
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                // edited by acidzombie24
                var hasHandle = false;
                try
                {
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        // edited by acidzombie24
                        hasHandle = mutex.WaitOne(5000, false);
                        if (hasHandle == false)
                            throw new TimeoutException("Timeout waiting for exclusive access");
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process,
                        // it will still get acquired
                        hasHandle = true;
                    }

                    // Perform your work here.
                    log4net.Config.XmlConfigurator.Configure();
                    // log version number 
                    Assembly asm = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                    log.Info($"Keyboard2Xinput v{fvi.ProductVersion}");

                    // parse args
                    if (args.Length > 1)
                    {
                        MessageBox.Show("Too many arguments. Usage: Keyboard2XinputGui [mappingfile]", "Keyboard2Xinput", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        String mappingFile = null;
                        if (args.Length > 0)
                        {
                            mappingFile = args[0];
                        }
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        AppDomain currentDomain = AppDomain.CurrentDomain;
                        currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
                        Application.ApplicationExit += new EventHandler(OnApplicationExit);
                        gui = new Keyboard2XinputGui(mappingFile);
                        // Run() without parameter to not show the form at launch
                        Application.Run();
                        //Application.Run(gui);
                    }

                }
                finally
                {
                    // edited by acidzombie24, added if statement
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }
        
        private static void OnApplicationExit(object sender, EventArgs e)
        {
            gui.CloseK2x();
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs e)
        {
            log.Fatal(e.ExceptionObject);
        }
    }
}
