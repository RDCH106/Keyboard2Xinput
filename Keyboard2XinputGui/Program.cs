using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keyboard2XinputGui
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            // log version number 
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            log.Info($"Keyboard2Xinput v{fvi.ProductVersion}");

            // parse args
            if (args.Length > 1)
            {
                MessageBox.Show("Too many arguments. Usage: Keyboard2XinputGui [mappingfile]", "Keyboard2Xinput",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }else
            {
                String mappingFile = null;
                if (args.Length>0)
                {
                    mappingFile = args[0];
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
                Application.Run(new Keyboard2XinputGui(mappingFile));
            }


        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs e)
        {
            log.Fatal(e.ExceptionObject);
        }
    }
}
