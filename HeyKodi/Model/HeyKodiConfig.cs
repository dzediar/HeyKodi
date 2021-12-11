using KodiRPC.RPC.Specifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyKodi.Model
{
    public abstract class HeyKodiConfigElementBase : INotifyPropertyChanged
    {
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class HeyKodiConfig : HeyKodiConfigElementBase
    {
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

        private string kodiWakeupSpeech = "codi";

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

        private double volume = 0.4;

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

        private List<HeyKodiCommandConfig> commands;

        public List<HeyKodiCommandConfig> Commands
        {
            get
            {
                return commands;
            }
            set
            {
                commands = value;
                NotifyPropertyChanged(nameof(Commands));
            }
        }
    }

    public class HeyKodiCommandConfig : HeyKodiConfigElementBase
    {
        private HeyKodiCommandEnum kodiCommand;

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

    public enum HeyKodiCommandEnum
    {
        ShowHeyKodiConfig,
        CancelHeyKodi,
        Search,
        Home,
        Back,
        Quit,
        Stop,
        Play,
        Pause,
        MuteUnmute,

        ShowVideos, // videos
        ShowTV, // tvsearch
        ShowGames, // games
        ShowMusic, // music
        ShowWeather, // weather
        ShowFavourites, // weather
        EjectOpticalDrive

        //favori (ajout)
	    //go (sélection)
	    //ping (mettre un timeout faible)
	    //sous-titres => avec param
	    //ejecter (lecteur optique)
	    //dormir
	    //éteindre
	    //redémarrer
    }

    public static class HeyKodiConfigExtensions
    {

        private static string configFilePath;

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
                    result = JsonConvert.DeserializeObject<HeyKodiConfig>(System.IO.File.ReadAllText(confPath, Encoding.UTF8));
                    result.Consolidate();
                }
                catch (Exception ex)
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

                System.IO.File.WriteAllText(confPath, JsonConvert.SerializeObject(config));
            }
            catch (Exception ex)
            {
                config.Init();
            }

            return config;
        }

        public static HeyKodiConfig Init(this HeyKodiConfig config)
        {
            config.KodiApiHost = "localhost";
            config.KodiApiPort = 5156;
            config.KodiApiUserName = "";
            config.KodiApiPassword = "";
            config.NeedHeyKodiWakeup = true;
            config.KodiWakeupSpeech = "codi";
            config.DebugMode = false;
            config.Volume = 0.4;
            config.MinimizeWhenPending = false;
            config.Consolidate();

            return config;
        }

        public static HeyKodiConfig Consolidate(this HeyKodiConfig config)
        {
            config.KodiApiHost = config.KodiApiHost?.Trim();
            config.KodiApiPort = config.KodiApiPort < 1 || config.KodiApiPort > 65535 ? 5156 : config.KodiApiPort;
            config.KodiWakeupSpeech = ConsolidateSpeech(config.KodiWakeupSpeech);

            if (config.Commands == null)
            {
                config.Commands = new List<HeyKodiCommandConfig>();
            }

            var unknownCommands = config.Commands.Where(c => !CommandRepository.Keys.Contains(c.KodiCommand)).ToList();

            foreach (var c in unknownCommands)
            {
                config.Commands.Remove(c);
            }

            foreach (var c in CommandRepository)
            {
                if (!config.Commands.Any(c2 => c2.KodiCommand == c.Key))
                {
                    config.Commands.Add(new HeyKodiCommandConfig()
                    {
                        KodiCommand = c.Key,
                        CommandSpeech = c.Value.DefaultSpeech
                    });
                }
            }

            foreach (var cmd in config.Commands)
            {
                cmd.CommandSpeech = ConsolidateSpeech(cmd.CommandSpeech);
            }

            return config;
        }

        public static string ConsolidateSpeech(string speech)
        {
            if (string.IsNullOrWhiteSpace(speech))
            {
                return string.Empty;
            }

            var purgedSpeech = new string(speech.ToArray().Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == ' ').ToArray());
            var speechWords = purgedSpeech.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).ToList();
            return string.Join(" ", speechWords);
        }

        public static readonly Dictionary<HeyKodiCommandEnum, CommandInfos> CommandRepository = new Dictionary<HeyKodiCommandEnum, CommandInfos>()
        {
            {
                HeyKodiCommandEnum.ShowHeyKodiConfig,
                new CommandInfos()
                {
                    Description = "Affichage de la configuration de Hey Kodi",
                    DefaultSpeech = "configuration",
                    KodiApiMethod = null,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.CancelHeyKodi,
                new CommandInfos()
                {
                    Description = "Annuler l'activation de Hey Kodi",
                    DefaultSpeech = "non rien",
                    KodiApiMethod = null,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Search,
                new CommandInfos()
                {
                    Description = "Recherche Kodi (extension globalsearch)",
                    DefaultSpeech = "recherche",
                    KodiApiMethod = KodiMethods.ExecuteAddon,
                    ParameterRequired = true,
                }
            },
            {
                HeyKodiCommandEnum.Home,
                new CommandInfos()
                {
                    Description = "Retour à l'accueil",
                    DefaultSpeech = "accueil",
                    KodiApiMethod = KodiMethods.InputHome,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Back,
                new CommandInfos()
                {
                    Description = "Retour en arrière",
                    DefaultSpeech = "retour",
                    KodiApiMethod = KodiMethods.InputBack,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Quit,
                new CommandInfos()
                {
                    Description = "Quitter Kodi",
                    DefaultSpeech = "quitter",
                    KodiApiMethod = KodiMethods.QuitApplication,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Stop,
                new CommandInfos()
                {
                    Description = "Arrêt de la lecture",
                    DefaultSpeech = "stop",
                    KodiApiMethod = KodiMethods.StopPlayer,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Play,
                new CommandInfos()
                {
                    Description = "Lancer la lecture",
                    DefaultSpeech = "play",
                    KodiApiMethod = KodiMethods.PlayPause,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.Pause,
                new CommandInfos()
                {
                    Description = "Mettre la lecture en pause",
                    DefaultSpeech = "pause",
                    KodiApiMethod = KodiMethods.PlayPause,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.MuteUnmute,
                new CommandInfos()
                {
                    Description = "Mute",
                    DefaultSpeech = "mute",
                    KodiApiMethod = KodiMethods.SetMute,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowVideos,
                new CommandInfos()
                {
                    Description = "Afficher les vidéos",
                    DefaultSpeech = "video",
                    KodiApiMethod = KodiMethods.ActivateWindow,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowTV,
                new CommandInfos()
                {
                    Description = "Afficher les chaînes de télévision",
                    DefaultSpeech = "tv",
                    KodiApiMethod = KodiMethods.ActivateWindow,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowGames,
                new CommandInfos()
                {
                    Description = "Afficher les jeux",
                    DefaultSpeech = "jeu",
                    KodiApiMethod = KodiMethods.ActivateWindow,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowMusic,
                new CommandInfos()
                {
                    Description = "Afficher la musique",
                    DefaultSpeech = "musique",
                    KodiApiMethod = KodiMethods.ActivateWindow,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowWeather,
                new CommandInfos()
                {
                    Description = "Afficher la météo",
                    DefaultSpeech = "meteo",
                    KodiApiMethod = KodiMethods.ActivateWindow,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.ShowFavourites,
                new CommandInfos()
                {
                    Description = "Afficher les favoris",
                    DefaultSpeech = "favori",
                    KodiApiMethod = KodiMethods.ActivateWindow,
                    ParameterRequired = false,
                }
            },
            {
                HeyKodiCommandEnum.EjectOpticalDrive,
                new CommandInfos()
                {
                    Description = "Ejecter le disque",
                    DefaultSpeech = "ejecter",
                    KodiApiMethod = KodiMethods.EjectOpticalDrive,
                    ParameterRequired = false,
                }
            },
        };

        public class CommandInfos
        {
            public string Description { get; set; }

            public string DefaultSpeech { get; set; }

            public string KodiApiMethod { get; set; }

            public bool ParameterRequired { get; set; }

            public override string ToString()
            {
                return Description;
            }
        }
    }
}
