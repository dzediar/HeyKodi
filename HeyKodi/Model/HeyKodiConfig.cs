using HeyKodi.Properties;
using KodiRPC.RPC.Specifications;
using KodiRPC.Services;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace HeyKodi.Model
{
    public abstract class HeyKodiConfigElementBase : INotifyPropertyChanged
    {
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class HeyKodiConfig : HeyKodiConfigElementBase, IRpcConnectorConfig
    {
        public HeyKodiConfig()
        {
            Language = HeyKodiConfigExtensions.GetDefaultLanguage();
        }

        private static List<CultureInfo> supportedLanguages = new List<CultureInfo>()
        {
            CultureInfo.GetCultureInfo("en-US"),
            CultureInfo.GetCultureInfo("fr-FR")
        };

        public static List<CultureInfo> SupportedLanguages
        {
            get
            {
                return supportedLanguages;
            }
        }

        private string language;

        public string Language
        {
            get
            {
                return language;
            }
            set
            {
                language = value;

                CultureInfo culture = null;

                if (string.IsNullOrWhiteSpace(language) || (culture = CultureInfo.GetCultureInfo(value)) == null)
                {
                    culture = Thread.CurrentThread.CurrentCulture.Name.StartsWith("fr") ?
                        CultureInfo.GetCultureInfo("fr-FR") : CultureInfo.GetCultureInfo("en-US");
                }

                Resources.Culture = culture;

                NotifyPropertyChanged(nameof(Language));

                if (KodiCommands != null)
                {
                    foreach (var c in KodiCommands)
                    {
                        c.NotifyPropertyChanged("");
                    }
                }
            }
        }
        
        private string kodiApiHost = "localhost";

        public string KodiApiHost 
        { 
            get
            {
                return kodiApiHost;
            }
            set
            {
                kodiApiHost = value;
                NotifyPropertyChanged(nameof(KodiApiHost));
            }
        } 

        private int kodiApiPort = 5156;

        public int KodiApiPort
        { 
            get
            {
                return kodiApiPort;
            }
            set
            {
                kodiApiPort = value;
                NotifyPropertyChanged(nameof(KodiApiPort));
            }
        }

        private string kodiApiUserName = "";

        public string KodiApiUserName
        {
            get
            {
                return kodiApiUserName;
            }
            set
            {
                kodiApiUserName = value;
                NotifyPropertyChanged(nameof(KodiApiUserName));
            }
        }

        private string kodiApiPassword = "";

        public string KodiApiPassword
        {
            get
            {
                return kodiApiPassword;
            }
            set
            {
                kodiApiPassword = value;
                NotifyPropertyChanged(nameof(KodiApiPassword));
            }
        }

        private bool needHeyKodiWakeup = true;

        public bool NeedHeyKodiWakeup
        {
            get
            {
                return needHeyKodiWakeup;
            }
            set
            {
                needHeyKodiWakeup = value;
                NotifyPropertyChanged(nameof(NeedHeyKodiWakeup));
            }
        }

        private string kodiWakeupSpeech = "kodi";

        public string KodiWakeupSpeech
        {
            get
            {
                return kodiWakeupSpeech;
            }
            set
            {
                kodiWakeupSpeech = value;
                NotifyPropertyChanged(nameof(KodiWakeupSpeech));
            }
        }

        private bool debugMode = false;

        public bool DebugMode
        {
            get
            {
                return debugMode;
            }
            set
            {
                debugMode = value;
                NotifyPropertyChanged(nameof(DebugMode));
            }
        }

        private double volume = 0.2;

        public double Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                NotifyPropertyChanged(nameof(Volume));
            }
        }

        private bool runAtWindowsStart = true;

        public bool RunAtWindowsStart
        {
            get
            {
                return runAtWindowsStart;
            }
            set
            {
                runAtWindowsStart = value;
                NotifyPropertyChanged(nameof(RunAtWindowsStart));
            }
        }

        private bool minimizeWhenPending = false;

        public bool MinimizeWhenPending
        {
            get
            {
                return minimizeWhenPending;
            }
            set
            {
                minimizeWhenPending = value;
                NotifyPropertyChanged(nameof(MinimizeWhenPending));
            }
        }

        private bool useSpeechSynthesizer = true;

        public bool UseSpeechSynthesizer
        {
            get
            {
                return useSpeechSynthesizer;
            }
            set
            {
                useSpeechSynthesizer = value;
                NotifyPropertyChanged(nameof(UseSpeechSynthesizer));
            }
        }

        private ObservableCollection<HeyKodiKodyCommandConfig> kodiCommands;

        public ObservableCollection<HeyKodiKodyCommandConfig> KodiCommands
        {
            get
            {
                return kodiCommands;
            }
            set
            {
                kodiCommands = value;
                NotifyPropertyChanged(nameof(KodiCommands));
            }
        }

        private ObservableCollection<HeyKodiShellCommandConfig> shellCommands;

        public ObservableCollection<HeyKodiShellCommandConfig> ShellCommands
        {
            get
            {
                return shellCommands;
            }
            set
            {
                shellCommands = value;
                NotifyPropertyChanged(nameof(ShellCommands));
            }
        }
    }

    public class HeyKodiKodyCommandConfig : HeyKodiConfigElementBase
    {
        private HeyKodiCommandEnum kodiCommand;

        [JsonConverter(typeof(StringEnumConverter))]
        public HeyKodiCommandEnum KodiCommand
        {
            get
            {
                return kodiCommand;
            }
            set
            {
                kodiCommand = value;
                NotifyPropertyChanged(nameof(KodiCommand));
            }
        }

        private string commandSpeech;

        public string CommandSpeech
        {
            get
            {
                return commandSpeech;
            }
            set
            {
                commandSpeech = value;
                NotifyPropertyChanged(nameof(CommandSpeech));
            }
        }
    }

    public class HeyKodiShellCommandConfig : HeyKodiConfigElementBase
    {
        private string commandLine;

        public string CommandLine
        {
            get
            {
                return commandLine;
            }
            set
            {
                commandLine = value;
                NotifyPropertyChanged(nameof(CommandLine));
            }
        }

        private string commandArguments;

        public string CommandArguments
        {
            get
            {
                return commandArguments;
            }
            set
            {
                commandArguments = value;
                NotifyPropertyChanged(nameof(CommandArguments));
            }
        }

        private string commandSpeech;

        public string CommandSpeech
        {
            get
            {
                return commandSpeech;
            }
            set
            {
                commandSpeech = value;
                NotifyPropertyChanged(nameof(CommandSpeech));
            }
        }
    }


    public enum HeyKodiCommandEnum
    {
        ShowHeyKodiConfig,
        CancelHeyKodi,
        Search,
        Youtube,
        Home,
        Back,
        Select,
        Right,
        Left,
        Up,
        Down,
        Quit,
        Stop,
        Play,
        Pause,
        Previous,
        Next,
        MuteUnmute,
        SetVolume,
        ShowVideos, 
        ShowTV, 
        ShowGames, 
        ShowMusic, 
        ShowWeather, 
        ShowFavourites,
        ShowRadio,
        EjectOpticalDrive,
        SystemShutdown,
        SystemReboot
    }

    public static class HeyKodiConfigExtensions
    {
        private static JsonSerializerSettings serializerConfig = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        private static string configFilePath;

        public const string SHELL_COMMAND_PARAMETER = "%%param%%";

        public static string GetConfigFilePath()
        {
            return configFilePath ?? (configFilePath = 
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"HeyKodi\HeyKodiConfig.json"));
        }

        public static HeyKodiConfig Load()
        {
            HeyKodiConfig result = null;

            var confPath = GetConfigFilePath();

            if (System.IO.File.Exists(confPath))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<HeyKodiConfig>(System.IO.File.ReadAllText(confPath, Encoding.UTF8), serializerConfig);
                    result.Consolidate();
                }
                catch
                {
                    result = new HeyKodiConfig().Init();
                }
            }
            else
            {
                result = new HeyKodiConfig().Init();
            }

            return result;
        }

        public static HeyKodiConfig Save(this HeyKodiConfig config)
        {
            var confPath = GetConfigFilePath();
            var confDir = System.IO.Path.GetDirectoryName(confPath);

            try
            {
                config.Consolidate();

                if (!System.IO.Directory.Exists(confDir))
                {
                    System.IO.Directory.CreateDirectory(confDir);
                }

                System.IO.File.WriteAllText(confPath, JsonConvert.SerializeObject(config, serializerConfig));
            }
            catch
            {
                config.Init();
            }

            try
            {
                string registryAppName = "HeyKodi";

                using (var rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (config.RunAtWindowsStart)
                    {
                        if (rkApp.GetValue(registryAppName) == null)
                        {
                            rkApp.SetValue(registryAppName, Assembly.GetEntryAssembly().Location);
                        }
                    }
                    else
                    {
                        if (rkApp.GetValue(registryAppName) != null)
                        {
                            rkApp.DeleteValue(registryAppName, false);
                        }
                    }
                }
            }
            catch
            {
                config.RunAtWindowsStart = false;
            }

            return config;
        }


        public static string GetDefaultLanguage()
        {
            var currentCultureName = Thread.CurrentThread.CurrentCulture.Name;

            return (HeyKodiConfig.SupportedLanguages.FirstOrDefault(c => c.Name == currentCultureName) ??
                HeyKodiConfig.SupportedLanguages.FirstOrDefault(c => c.Name.StartsWith(currentCultureName.Substring(0, 2))) ??
                HeyKodiConfig.SupportedLanguages.First()).Name;
        }

        public static HeyKodiConfig Init(this HeyKodiConfig config)
        {
            config.Language = GetDefaultLanguage();
            config.KodiApiHost = "localhost";
            config.KodiApiPort = 5156;
            config.KodiApiUserName = "";
            config.KodiApiPassword = "";
            config.NeedHeyKodiWakeup = false;
            config.KodiWakeupSpeech = "codi";
            config.DebugMode = false;
            config.Volume = 1;
            config.MinimizeWhenPending = false;
            config.UseSpeechSynthesizer = true;
            config.Consolidate();

            return config;
        }

        public static HeyKodiConfig Consolidate(this HeyKodiConfig config)
        {
            if (!HeyKodiConfig.SupportedLanguages.Any(sl => sl.Name == config.Language))
            {
                config.Language = GetDefaultLanguage();
            }

            config.KodiApiHost = config.KodiApiHost?.Trim();
            config.KodiApiPort = config.KodiApiPort < 1 || config.KodiApiPort > 65535 ? 5156 : config.KodiApiPort;
            config.KodiWakeupSpeech = ConsolidateSpeech(config.KodiWakeupSpeech);

            var kodiCommands = new List<HeyKodiKodyCommandConfig>();

            if (config.KodiCommands != null)
            {
                kodiCommands.AddRange(config.KodiCommands);
            }

            var unknownCommands = kodiCommands.Where(c => !CommandRepository.Keys.Contains(c.KodiCommand)).ToList();

            foreach (var c in unknownCommands)
            {
                kodiCommands.Remove(c);
            }

            foreach (var c in CommandRepository)
            {
                if (!kodiCommands.Any(c2 => c2.KodiCommand == c.Key))
                {
                    kodiCommands.Add(new HeyKodiKodyCommandConfig()
                    {
                        KodiCommand = c.Key,
                        CommandSpeech = c.Value.DefaultSpeech
                    });
                }
            }

            foreach (var cmd in kodiCommands)
            {
                cmd.CommandSpeech = ConsolidateSpeech(cmd.CommandSpeech);
            }

            config.KodiCommands = new ObservableCollection<HeyKodiKodyCommandConfig>(kodiCommands);

            if (config.ShellCommands == null)
            {
                config.ShellCommands = new ObservableCollection<HeyKodiShellCommandConfig>();
            }
            else
            {
                config.ShellCommands = new ObservableCollection<HeyKodiShellCommandConfig>(
                    config.ShellCommands.Where(c => !(string.IsNullOrWhiteSpace(c.CommandSpeech) || string.IsNullOrWhiteSpace(c.CommandLine))));
            }

            return config;
        }

        public static string ConsolidateSpeech(string speech)
        {
            if (string.IsNullOrWhiteSpace(speech))
            {
                return string.Empty;
            }

            var purgedSpeech = speech; // new string(speech.ToArray().Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == ' ').ToArray());
            var speechWords = purgedSpeech.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).Select(w => w.Trim()).ToList();
            return string.Join(" ", speechWords);
        }

        public static readonly Dictionary<HeyKodiCommandEnum, CommandInfos> CommandRepository = new Dictionary<HeyKodiCommandEnum, CommandInfos>()
        {
            {
                HeyKodiCommandEnum.ShowHeyKodiConfig,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOW_HEYKODI_CONFIG,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_CONFIGURATION,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.CancelHeyKodi,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_CANCEL_HEYKODI_ACTIVATION,
                    DefaultSpeech = "annuler",
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Search,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_KODI_SEARCH,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SEARCH,
                    ParameterRequired = true,
                    GetParameterQuestion = () => Resources.KODY_COMMAND_QUESTION_SEARCH
                }
            },
            //{
            //    HeyKodiCommandEnum.Youtube,
            //    new CommandInfos()
            //    {
            //        GetDescription = () => Resources.KODI_COMMAND_DESC_YOUTUBE_SEARCH,
            //        DefaultSpeech = Resources.KODI_COMMAND_SPEECH_YOUTUBE,
            //        ParameterRequired = true,
            //        GetParameterQuestion = () => Resources.KODY_COMMAND_QUESTION_SEARCH
            //    }
            //},            
            {
                HeyKodiCommandEnum.Home,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_HOME,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_HOME,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Back,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_BACK,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_BACK,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Select,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SELECT,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SELECT,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Right,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_RIGHT,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_RIGHT,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Left,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_LEFT,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_LEFT,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Up,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_UP,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_UP,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Down,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_DOWN,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_DOWN,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Quit,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_QUIT,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_QUIT,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Stop,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_STOP,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_STOP,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Play,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_PLAY,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_PLAY,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Pause,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_PAUSE,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_PAUSE,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Previous,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_PREVIOUS,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_PREVIOUS,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Next,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_NEXT,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_NEXT,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.MuteUnmute,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_MUTE,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_MUTE,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.SetVolume,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_VOLUME,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_VOLUME,
                    ParameterRequired = true,
                    GetParameterQuestion = () => Resources.KODY_COMMAND_QUESTION_VOLUME
                }
            },
            {
                HeyKodiCommandEnum.ShowVideos,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOWVIDEOS,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SHOWVIDEOS,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowTV,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOWTV,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SHOWTV,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowGames,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOWGAMES,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SHOWGAMES,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowMusic,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOWMUSIC,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SHOWMUSIC,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowWeather,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOWWEATHER,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SHOWWEATHER,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowFavourites,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOWFAVOURITES,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SHOWFAVOURITES,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowRadio,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SHOWRADIOS,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SHOWRADIOS,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.EjectOpticalDrive,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_EJECT,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_EJECT,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.SystemShutdown,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SYSTEM_SHUTDOWN,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SYSTEM_SHUTDOWN,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.SystemReboot,
                new CommandInfos()
                {
                    GetDescription = () => Resources.KODI_COMMAND_DESC_SYSTEM_RESTART,
                    DefaultSpeech = Resources.KODI_COMMAND_SPEECH_SYSTEM_RESTART,
                    ParameterRequired = false,
                }
            }
        };

        public class CommandInfos : HeyKodiConfigElementBase
        {
            public Func<string> GetDescription { get; set; }

            public string Description { get => GetDescription == null ? null : GetDescription(); }

            public string DefaultSpeech { get; set; }

            public bool ParameterRequired { get; set; }

            public Func<string> GetParameterQuestion { get; set; }

            public string ParameterQuestion { get => GetParameterQuestion == null ? null : GetParameterQuestion(); }

            public override string ToString()
            {
                return Description;
            }
        }
    }
}
