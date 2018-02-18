using IniParser;
using IniParser.Model;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboard2XinputLib
{
    class Config
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly String DEFAULT_NAME = "mapping.ini";

        public IniData mapping { get; }
        public int padCount { get; }


        public Config(string configFilePath)
        {
            if (configFilePath == null)
            {
                configFilePath = DEFAULT_NAME;
                log.Debug($"Using default config file: {configFilePath}");
            }
            if (!System.IO.Path.IsPathRooted(configFilePath))
            {
                // get the directory where the program resides
                string codebase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                string baseDir = new Uri(System.IO.Path.GetDirectoryName(codebase)).LocalPath;
                configFilePath = baseDir + "\\" + configFilePath;
            }

            if (!System.IO.File.Exists(configFilePath))
            {
                //log.Error($"Config file does not exist: {configFilePath}");
                throw new FileNotFoundException($"Config file does not exist: {configFilePath}");
            }

            log.Info($"Loading config file: {configFilePath}");

            // read config
            var parser = new FileIniDataParser();

            // TODO check file existence; create a default one if it doesn't exist
            mapping = parser.ReadFile(configFilePath);

            // how many pads?
            foreach (SectionData section in mapping.Sections)
            {
                String intStr = section.SectionName.Substring(section.SectionName.Length - 1);
                int padNumber = 0;
                if (int.TryParse(intStr, out padNumber))
                {
                    log.Debug($"found config for pad {padNumber}");
                    padCount = Math.Max(padCount, padNumber);

                }
                else
                {
                    log.Error($"Ignored section [{section.SectionName}]");
                }
                log.Info($"found {padCount} pads");
            }
        }
    }
}
