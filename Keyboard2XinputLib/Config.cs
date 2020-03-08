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

        public List<IniData> Mappings { get; }
        private int currentMappingIndex;
        public int CurrentMappingIndex {
            get { return currentMappingIndex; }
            set {
                if (value + 1 > Mappings.Count)
                {
                    log.Warn($"Mapping{value} doesn't exist; active mapping not changed");
                } else
                {
                    currentMappingIndex = value;
                }

            }
        }
        public int PadCount { get; }

        public Config(string configFilePath)
        {
            Mappings = new List<IniData>(0);
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

            // read config(s)
            var parser = new FileIniDataParser();
            log.Info($"Loading config file: {configFilePath}");
            Mappings.Add(parser.ReadFile(configFilePath));
            // how many pads?
            PadCount = Math.Max(PadCount, countPads(Mappings[0]));

            // additional mappings
            string baseFolder = System.IO.Path.GetDirectoryName(configFilePath);
            int i = 1;
            Boolean exists;
            do {
                configFilePath = $"{baseFolder}\\mapping{i}.ini";
                exists = System.IO.File.Exists(configFilePath);
                log.Info($"{configFilePath} exists: {exists}");
                if (exists)
                {
                    log.Info($"Loading additional mapping file: {configFilePath}");
                    Mappings.Add(parser.ReadFile(configFilePath));
                    if (Mappings[i]["config"].Count > 0)
                    {
                        throw new Exception($"Additional mapping file {configFilePath} must NOT contain a 'config' section");
                    }
                    // update pad count
                    PadCount = Math.Max(PadCount, countPads(Mappings[i]));
                    // copy the config from mapping 0
                    Mappings[i]["config"].Merge(Mappings[0]["config"]);
                }
                i++;
            } while (exists);

            log.Info($"found {PadCount} pads");
            log.Info($"found {Mappings.Count} mappings");
        }

        public IniData getCurrentMapping()
        {
            return Mappings[currentMappingIndex];
        }

        private int countPads(IniData mapping)
        {
            int result = 0;
            foreach (SectionData section in mapping.Sections)
            {
                String intStr = section.SectionName.Substring(section.SectionName.Length - 1);
                int padNumber = 0;
                if (int.TryParse(intStr, out padNumber))
                {
                    log.Debug($"found config for pad {padNumber}");
                    result = Math.Max(PadCount, padNumber);

                }
                else if (("config".Equals(section.SectionName)) || ("startup".Equals(section.SectionName)))
                {
                    // nothing special?
                }
                else
                {
                    log.Error($"Ignored section [{section.SectionName}]");
                }
            }
            return result;
        }
    }
}
