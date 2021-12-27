using GalaSoft.MvvmLight.CommandWpf;
using HeyKodi.Messages;
using HeyKodi.Model;
using HeyKodi.Properties;
using HeyKodi.Tools;
using KodiRPC.Responses.VideoLibrary;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;
using KodiRPC.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using zComp.Core.Helpers;

namespace HeyKodi.ViewModels
{
    public class MainViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private static MainViewModel instance;

        public static MainViewModel Instance { get { return instance ?? (instance = new MainViewModel()); } }

        public Dictionary<KodiSpeechRecognizerSound, string> SoundsResources = new Dictionary<KodiSpeechRecognizerSound, string>()
        {
            { KodiSpeechRecognizerSound.CancelSound, "GS46.mp3" },
            { KodiSpeechRecognizerSound.WakeupSound, "GS155.mp3" },
            { KodiSpeechRecognizerSound.NeedMoreParamSound, "GS213.mp3" },
            { KodiSpeechRecognizerSound.RunCommandSound, "GS254.mp3" }
        };

        private bool stayActivated;

        private int minimizationDelay = 0;

        private List<string> grammar;

        private List<string> mediaGrammar;

        private Dictionary<char, char> accents = new Dictionary<char, char>()
        {
            { 'à', 'a' },
            { 'â', 'a' },
            { 'ä', 'a' },
            { 'ô', 'o' },
            { 'ö', 'o' },
            { 'ù', 'u' },
            { 'û', 'u' },
            { 'ü', 'u' },
            { 'é', 'e' },
            { 'è', 'e' },
            { 'ê', 'e' },
            { 'ë', 'e' },
            { 'î', 'i' },
            { 'ï', 'i' },
        };

        private MainViewModel()
        {
            ShowConfigurationCommand = new RelayCommand(() => ShowConfiguration());
            ShowDocumentationCommand = new RelayCommand(() => ShowDocumentation());
            MinimizeCommand = new RelayCommand(() => Minimize());
            AddShellCommandCommand = new RelayCommand(() => AddShellCommand());
            RemoveShellCommandCommand = new RelayCommand(() => RemoveShellCommand());
        }

        public void Init()
        {
            try
            {
                KodiService = new KodiService();

                KodiService.Config = HeyKodiConfig = HeyKodiConfigExtensions.Load();

                KodiSpeechRecognizer = new KodiSpeechRecognizer(HeyKodiConfig, KodiService);

                KodiSpeechRecognizer.PropertyChanged += KodiSpeechRecognizer_PropertyChanged;

                StartRecognition();
            }
            catch (Exception ex)
            { 
                throw new CriticalApplicationException(Resources.MISC_CRITICAL_ERROR, ex);
            }
        }

        private void SpeechSynthesizer_SpeakCompleted(object sender, System.Speech.Synthesis.SpeakCompletedEventArgs e)
        {
            KodiSpeechRecognizer.RecognizeAsync();
        }

        private void KodiSpeechRecognizer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(KodiSpeechRecognizer.State))
            {
                if (KodiSpeechRecognizer.State == KodiSpeechRecognizerState.Pending)
                {
                    if (HeyKodiConfig.MinimizeWhenPending)
                    {
                        if (stayActivated)
                        {
                            stayActivated = false;
                        }
                        else
                        {
                            Task.Run(() => { System.Threading.Thread.Sleep(minimizationDelay); Minimize(); });
                        }
                    }
                }
                else
                {
                    Activate();
                }

                minimizationDelay = 1500;
            }
        }

        public bool RunShellCommand(HeyKodiShellCommandConfig command, string parameter = null, bool mute = false)
        {
            Exception exception = null;

            try
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = command.CommandLine,
                    Arguments = command.CommandArguments?.Replace(HeyKodiConfigExtensions.SHELL_COMMAND_PARAMETER, parameter ?? string.Empty),
                    CreateNoWindow = true,
                    ErrorDialog = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                exception = new Exception(Resources.MAINVIEWMODEL_SHELL_COMMAND_EXE_FAILED, ex);
            }

            if (exception != null)
            {
                if (!mute)
                {
                    PlaySound(KodiSpeechRecognizerSound.CancelSound);
                }

                ShowMessage(ShowMessageType.Error, exception);

                return false;
            }
            else
            {
                if (!mute)
                {
                    PlaySound(KodiSpeechRecognizerSound.RunCommandSound);
                }

                return true;
            }
        }



        public bool RunKodiCommand(HeyKodiKodyCommandConfig command, string parameter = null, bool mute = false)
        {
            Exception exception = null;

            try
            {
                if (command.KodiCommand == HeyKodiCommandEnum.ShowHeyKodiConfig)
                {
                    ShowConfiguration();
                }
                else if (command.KodiCommand == HeyKodiCommandEnum.CancelHeyKodi)
                {
                    KodiSpeechRecognizer.Cancel(false);
                }
                else
                {
                    List<ActivePlayer> players = null;

                    if (command.KodiCommand == HeyKodiCommandEnum.Search ||
                        command.KodiCommand == HeyKodiCommandEnum.Youtube ||
                        command.KodiCommand == HeyKodiCommandEnum.Home ||
                        command.KodiCommand == HeyKodiCommandEnum.ShowGames ||
                        command.KodiCommand == HeyKodiCommandEnum.ShowMusic ||
                        command.KodiCommand == HeyKodiCommandEnum.ShowTV ||
                        command.KodiCommand == HeyKodiCommandEnum.ShowVideos ||
                        command.KodiCommand == HeyKodiCommandEnum.ShowWeather)
                    {
                        KodiService.InputBack();
                        //KodiService.InputHome();
                    }
                    else if (command.KodiCommand == HeyKodiCommandEnum.Stop ||
                        command.KodiCommand == HeyKodiCommandEnum.Play ||
                        command.KodiCommand == HeyKodiCommandEnum.Pause ||
                        command.KodiCommand == HeyKodiCommandEnum.Previous ||
                        command.KodiCommand == HeyKodiCommandEnum.Next)
                    {
                        players = KodiService.GetActivePlayers(new GetActivePlayersParams()).Result
                            .Where(p => p.PlayerType == "internal").ToList();
                    }

                    switch (command.KodiCommand)
                    {
                        case HeyKodiCommandEnum.Search:
                            KodiService.ExecuteAddon(new ExecuteAddonParams()
                            {
                                AddonId = "script.globalsearch",
                                Wait = false,
                                Params = new string[] { "searchstring=" + parameter }
                            });
                            break;
                        //case HeyKodiCommandEnum.Youtube:
                        //    KodiService.ExecuteAddon(new ExecuteAddonParams()
                        //    {
                        //        AddonId = "plugin.video.youtube",
                        //        Wait = false,
                        //        Params = new string[] { "action=search_video", "q=" + parameter }
                        //    });
                        //    break;
                        case HeyKodiCommandEnum.Home:
                            KodiService.InputHome();
                            break;
                        case HeyKodiCommandEnum.Back:
                            KodiService.InputBack();
                            break;
                        case HeyKodiCommandEnum.Select:
                            KodiService.InputSelect();
                            break;
                        case HeyKodiCommandEnum.Right:
                            KodiService.InputRight();
                            break;
                        case HeyKodiCommandEnum.Left:
                            KodiService.InputLeft();
                            break;
                        case HeyKodiCommandEnum.Up:
                            KodiService.InputUp();
                            break;
                        case HeyKodiCommandEnum.Down:
                            KodiService.InputDown();
                            break;
                        case HeyKodiCommandEnum.Quit:
                            KodiService.QuitApplication();
                            break;
                        case HeyKodiCommandEnum.Stop:
                            if (players != null && players.Count > 0)
                            {
                                foreach (var p in players)
                                {
                                    KodiService.StopPlayer(new StopPlayerParams() { PlayerId = p.PlayerId });
                                }
                            }
                            break;
                        case HeyKodiCommandEnum.Play:
                            if (players != null && players.Count > 0)
                            {
                                foreach (var p in players)
                                {
                                    var speed = KodiService.TogglePlayerPlayPause(new TogglePlayPauseParams() { PlayerId = p.PlayerId }).Result.Speed;
                                    if (speed == 0)
                                    {
                                        KodiService.TogglePlayerPlayPause(new TogglePlayPauseParams() { PlayerId = p.PlayerId });
                                    }
                                }
                            }
                            break;
                        case HeyKodiCommandEnum.Pause:
                            if (players != null && players.Count > 0)
                            {
                                foreach (var p in players)
                                {
                                    var speed = KodiService.TogglePlayerPlayPause(new TogglePlayPauseParams() { PlayerId = p.PlayerId }).Result.Speed;
                                    if (speed > 0)
                                    {
                                        KodiService.TogglePlayerPlayPause(new TogglePlayPauseParams() { PlayerId = p.PlayerId });
                                    }
                                }
                            }
                            break;
                        case HeyKodiCommandEnum.Previous:
                            if (players != null && players.Count > 0)
                            {
                                foreach (var p in players)
                                {
                                    KodiService.PlayerGoto(new PlayerGotoParams() { PlayerId = p.PlayerId, To = "previous" });
                                }
                            }
                            break;
                        case HeyKodiCommandEnum.Next:
                            if (players != null && players.Count > 0)
                            {
                                foreach (var p in players)
                                {
                                    KodiService.PlayerGoto(new PlayerGotoParams() { PlayerId = p.PlayerId, To = "next" });
                                }
                            }
                            break;
                        case HeyKodiCommandEnum.MuteUnmute:
                            KodiService.SetMute(new SetMuteParams());
                            break;
                        case HeyKodiCommandEnum.SetVolume:
                            parameter = parameter.Trim(' ', '%');
                            if (int.TryParse(parameter, out var volume))
                            {
                                if (volume < 10)
                                {
                                    volume = 10;
                                }
                                else
                                {
                                    if (volume > 100)
                                    {
                                        volume = 100;
                                    }
                                }

                                KodiService.SetVolume(new SetVolumeParams() { Volume = (int)volume });
                            }
                            else
                            {
                                throw new Exception(string.Format(Resources.MAINVIEWMODEL_BAD_VOLUME_VALUE, parameter));
                            }
                            break;
                        case HeyKodiCommandEnum.ShowVideos:
                            KodiService.ActivateWindow("videos");
                            break;
                        case HeyKodiCommandEnum.ShowTV:
                            KodiService.ActivateWindow("tvsearch");
                            break;
                        case HeyKodiCommandEnum.ShowGames:
                            KodiService.ActivateWindow("games");
                            break;
                        case HeyKodiCommandEnum.ShowMusic:
                            KodiService.ActivateWindow("music");
                            break;
                        case HeyKodiCommandEnum.ShowWeather:
                            KodiService.ActivateWindow("weather");
                            break;
                        case HeyKodiCommandEnum.ShowFavourites:
                            KodiService.ActivateWindow("favourites");
                            break;
                        case HeyKodiCommandEnum.ShowRadio:
                            KodiService.ActivateWindow("radiochannels");
                            break;
                        case HeyKodiCommandEnum.EjectOpticalDrive:
                            KodiService.EjectOpticalDrive(new EjectOpticalDriveParams());
                            break;
                        case HeyKodiCommandEnum.SystemShutdown:
                            KodiService.SystemShutdown();
                            break;
                        case HeyKodiCommandEnum.SystemReboot:
                            KodiService.SystemReboot();
                            break;
                        default:
                            throw new Exception(string.Format(Resources.ERROR_UNKNOWN_COMMAND, command));
                    }
                }
            }
            catch (Exception ex)
            {
                exception = new Exception(Resources.MAINVIEWMODEL_KODI_COMMAND_EXE_FAILED, ex);
            }

            if (exception != null)
            {
                if (!mute)
                {
                    PlaySound(KodiSpeechRecognizerSound.CancelSound);
                }

                ShowMessage(ShowMessageType.Error, exception);

                return false;
            }
            else
            {
                if (!mute)
                {
                    PlaySound(KodiSpeechRecognizerSound.RunCommandSound);
                }

                return true;
            }
        }

        public void PlaySound(KodiSpeechRecognizerSound sound, string speech = null)
        {
            var soundsDir = UnpackSounds();
            var soundFileName = System.IO.Path.Combine(soundsDir, SoundsResources[sound]);

            var msg = new PlaySoundMsg(this, soundFileName, speech);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        private string UnpackSounds()
        {
            var soundsDir = Path.Combine(System.IO.Path.GetDirectoryName(HeyKodiConfigExtensions.GetConfigFilePath()), "Sounds");

            if (!System.IO.Directory.Exists(soundsDir))
            {
                System.IO.Directory.CreateDirectory(soundsDir);
                foreach (var s in SoundsResources)
                {
                    var sr = "pack://application:,,,/HeyKodi;component/Sounds/" + s.Value;
                    var srp = Path.Combine(soundsDir, s.Value);
                    var sri = Application.GetResourceStream(new Uri(sr));

                    using (var srs = sri.Stream)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            srs.CopyTo(ms);
                            System.IO.File.WriteAllBytes(srp, ms.ToArray());
                        }
                    }
                }
            }

            return soundsDir;
        }

        public void ShowMessage(ShowMessageType messageType, object message)
        {
            KodiSpeechRecognizer.Cancel(false);

            if (message is Exception ex && !HeyKodiConfig.DebugMode)
            {
                message = ex.ToMessageRecursive();
            }

            var msg = new ShowMessageMsg(this, messageType, this.Title, message);

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        private void ShowConfiguration()
        {
            stayActivated = true;
            StopRecognition();
            var msg = new ShowConfigurationMsg(this);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
            HeyKodiConfig.Save();
            StartRecognition();
        }

        private void BuildGrammar()
        {
            if (mediaGrammar == null && !string.IsNullOrWhiteSpace(heyKodiConfig.KodiApiHost))
            {
                try
                {
                    var ponctuations = new char[] { ';', ',', ':', '.', '!', '?', '/', '-', '_', '=' };

                    var movies = kodiService.GetMovies(new GetMoviesParams() { Properties = new string[] { "title", "writer", "genre" } });
                    var tvShows = kodiService.GetTvShows(new GetTvShowsParams() { Properties = new string[] { "title", "genre" } });
                    var albums = kodiService.GetAlbums(new GetAlbumsParams() { Properties = new string[] { "title", "artist", "songgenres" } });

                    var moviesTitles = movies.Result.Movies
                        .Select(m => m.Title)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    var movieTitlesParts = moviesTitles
                        .Select(t => t.Split(ponctuations))
                        .Where(mt => mt.Length > 1)
                        .SelectMany(mt => mt)
                        .Select(tp => tp.Trim())
                        .Where(tp => tp.Length > 3)
                        .ToList();

                    var moviesGenres = movies.Result.Movies
                        .Where(m => m.Genre != null && m.Genre.Length > 0)
                        .SelectMany(m => m.Genre)
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .Select(g => g.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    var moviesWriters = movies.Result.Movies
                        .Where(m => m.Writer != null && m.Writer.Length > 0)
                        .SelectMany(m => m.Writer)
                        .Where(w => !string.IsNullOrWhiteSpace(w))
                        .Select(g => g.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    var tvShowsTitles = tvShows.Result.TvShows
                        .Select(m => m.Title)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    var tvShowsTitlesParts = tvShowsTitles
                        .Select(t => t.Split(ponctuations))
                        .Where(mt => mt.Length > 1)
                        .SelectMany(mt => mt)
                        .Select(tp => tp.Trim())
                        .Where(tp => tp.Length > 3)
                        .ToList();

                    var tvShowsGenres = tvShows.Result.TvShows
                        .Where(m => m.Genre != null && m.Genre.Length > 0)
                        .SelectMany(m => m.Genre)
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .Select(g => g.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    var albumsTitles = albums.Result.Albums
                        .Select(m => m.Title)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    var albumsTitlesParts = albumsTitles
                        .Select(t => t.Split(ponctuations))
                        .Where(mt => mt.Length > 1)
                        .SelectMany(mt => mt)
                        .Select(tp => tp.Trim())
                        .Where(tp => tp.Length > 3)
                        .ToList();

                    var albumsGenres = albums.Result.Albums
                        .Where(a => a.SongGenres != null && a.SongGenres.Length > 0)
                        .SelectMany(a => a.SongGenres)
                        .Select(sg => sg.Title)
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .Select(g => g.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    var albumsArtists = albums.Result.Albums
                        .Where(a => a.Artist != null && a.Artist.Length > 0)
                        .SelectMany(a => a.Artist)
                        .Where(a => !string.IsNullOrWhiteSpace(a))
                        .Select(a => a.Trim().ToLower())
                        .Distinct()
                        .ToList();

                    mediaGrammar = new List<string>();

                    mediaGrammar.AddRange(moviesTitles);
                    mediaGrammar.AddRange(movieTitlesParts);
                    mediaGrammar.AddRange(tvShowsTitles);
                    mediaGrammar.AddRange(tvShowsTitlesParts);
                    mediaGrammar.AddRange(albumsArtists);

                    mediaGrammar.AddRange(albumsTitles);
                    mediaGrammar.AddRange(albumsTitlesParts);
                    mediaGrammar.AddRange(moviesGenres);
                    mediaGrammar.AddRange(moviesWriters);
                    mediaGrammar.AddRange(tvShowsGenres);
                    mediaGrammar.AddRange(albumsGenres);
                }
                catch (Exception ex)
                {
                    MainViewModel.Instance.ShowMessage(Messages.ShowMessageType.Warning,
                        Resources.MAINVIEWMODEL_GET_MEDIA_GRAMAR_FAILED + ex.ToMessageDetailed());
                }
            }

            grammar = HeyKodiConfig.KodiCommands.Select(c => c.CommandSpeech)
                .Union(HeyKodiConfig.ShellCommands.Select(c => c.CommandSpeech))
                .Distinct().ToList();

            if (HeyKodiConfig.NeedHeyKodiWakeup && !string.IsNullOrWhiteSpace(HeyKodiConfig.KodiWakeupSpeech))
            {
                grammar.Add(HeyKodiConfig.KodiWakeupSpeech);
            }

            for (int i = 10; i <= 100; i += 10)
            {
                grammar.Add($"{i} %");
            }

            if (mediaGrammar != null)
            {
                var mediaDoublons = mediaGrammar
                    .Select(g => new KeyValuePair<string, string>(GetPurgedGrammar(g), g))
                    .ToLookup(g => g.Key);

                var filtredMediaDoublons = mediaDoublons
                    .Select(d => d.OrderBy(dv => dv.Value.Length).First().Value)
                    .Where(d => d.Length > 3)
                    .ToList();

                grammar.AddRange(filtredMediaDoublons);
            }

            grammar = grammar.Distinct().Where(g => g.Length > 3).Take(1000).ToList();
        }

        private string GetPurgedGrammar(string grammar)
        {
            return string.Join("", new string(grammar.ToArray()
                    .Select(c => accents.ContainsKey(c) ? accents[c] : c)
                    .Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == ' ')
                    .ToArray())
                .Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).Select(w => w.Trim()));
        }

        public void StartRecognition()
        {
            BuildGrammar();
            KodiSpeechRecognizer.Start(grammar);

            SpeechSynthesizer = new SpeechSynthesizer();

            if (HeyKodiConfig.UseSpeechSynthesizer)
            {
                var installedVoices = SpeechSynthesizer.GetInstalledVoices();

                if (installedVoices.Count == 0)
                {
                    SpeechSynthesizer.Dispose();
                    SpeechSynthesizer = null;
                }
                else
                {
                    var voiceToUse = installedVoices.FirstOrDefault(v => v.VoiceInfo.Culture.Name == System.Threading.Thread.CurrentThread.CurrentCulture.Name) ?? installedVoices[0];
                    SpeechSynthesizer.SelectVoice(voiceToUse.VoiceInfo.Name);
                    SpeechSynthesizer.SpeakCompleted += SpeechSynthesizer_SpeakCompleted;
                }
            }
        }

        public void StopRecognition()
        {
            if (SpeechSynthesizer != null)
            {
                SpeechSynthesizer.Dispose();
                SpeechSynthesizer = null;
            }

            KodiSpeechRecognizer.Stop();
        }

        private void ShowDocumentation()
        {
            var culture = string.IsNullOrWhiteSpace(HeyKodiConfig.Language) ? 
                Thread.CurrentThread.CurrentCulture.Name.Substring(0, 2) : HeyKodiConfig.Language.Substring(0, 2);
            
            var docPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"HeyKodi.{culture}.pdf");

            if (!File.Exists(docPath))
            {
                docPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"HeyKodi.EN.pdf");
            }

            if (File.Exists(docPath))
            {
                Process.Start(docPath);
            }
        }        

        private void Minimize()
        {
            var msg = new MinimizeHeyKodiMsg(this);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        private void AddShellCommand()
        {
            HeyKodiConfig.ShellCommands.Add(new HeyKodiShellCommandConfig()
            {
                CommandLine = string.Empty,
                CommandArguments = string.Empty,
                CommandSpeech = string.Empty
            });
        }

        private void RemoveShellCommand()
        {
            if (SelectedApplicationCommmand != null)
            {
                HeyKodiConfig.ShellCommands.Remove(SelectedApplicationCommmand);
            }
        }

        private void Activate()
        {
            var msg = new ActivateHeyKodiMsg(this);
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(msg);
        }

        public override void Cleanup()
        {
            KodiSpeechRecognizer.Dispose();
            SpeechSynthesizer?.Dispose();

            HeyKodiConfig.Save();

            base.Cleanup();
        }

        private KodiService kodiService;

        public KodiService KodiService
        {
            get 
            {
                return kodiService;
            }
            private set
            {
                kodiService = value;
                RaisePropertyChanged(nameof(KodiService));
            }
        }

        private KodiSpeechRecognizer kodiSpeechRecognizer;

        public KodiSpeechRecognizer KodiSpeechRecognizer
        {
            get
            {
                return kodiSpeechRecognizer;
            }
            private set
            {
                kodiSpeechRecognizer = value;
                RaisePropertyChanged(nameof(KodiSpeechRecognizer));
            }
        }

        private SpeechSynthesizer speechSynthesizer;

        public SpeechSynthesizer SpeechSynthesizer
        {
            get
            {
                return speechSynthesizer;
            }
            private set
            {
                speechSynthesizer = value;
                RaisePropertyChanged(nameof(SpeechSynthesizer));
            }
        }        

        private HeyKodiConfig heyKodiConfig;

        public HeyKodiConfig HeyKodiConfig
        {
            get
            {
                return heyKodiConfig;
            }
            private set
            {
                heyKodiConfig = value;
                RaisePropertyChanged(nameof(HeyKodiConfig));
            }
        }

        private HeyKodiShellCommandConfig selectedApplicationCommmand;

        public HeyKodiShellCommandConfig SelectedApplicationCommmand
        {
            get
            {
                return selectedApplicationCommmand;
            }
            set
            {
                selectedApplicationCommmand = value;
                RaisePropertyChanged(nameof(SelectedApplicationCommmand));
            }
        }


        private RelayCommand showConfigurationCommand;

        public RelayCommand ShowConfigurationCommand
        {
            get
            {
                return showConfigurationCommand;
            }
            private set
            {
                showConfigurationCommand = value;
                RaisePropertyChanged(nameof(ShowConfigurationCommand));
            }
        }

        private RelayCommand showDocumentationCommand;

        public RelayCommand ShowDocumentationCommand
        {
            get
            {
                return showDocumentationCommand;
            }
            private set
            {
                showDocumentationCommand = value;
                RaisePropertyChanged(nameof(ShowDocumentationCommand));
            }
        }

        private RelayCommand addShellCommandCommand;

        public RelayCommand AddShellCommandCommand
        {
            get
            {
                return addShellCommandCommand;
            }
            private set
            {
                addShellCommandCommand = value;
                RaisePropertyChanged(nameof(AddShellCommandCommand));
            }
        }

        private RelayCommand removeShellCommandCommand;

        public RelayCommand RemoveShellCommandCommand
        {
            get
            {
                return removeShellCommandCommand;
            }
            private set
            {
                removeShellCommandCommand = value;
                RaisePropertyChanged(nameof(RemoveShellCommandCommand));
            }
        }

        private RelayCommand minimizeCommand;

        public RelayCommand MinimizeCommand
        {
            get
            {
                return minimizeCommand;
            }
            private set
            {
                minimizeCommand = value;
                RaisePropertyChanged(nameof(MinimizeCommand));
            }
        }

        public string Title { get; } = "Hey Kodi !";
    }
}
