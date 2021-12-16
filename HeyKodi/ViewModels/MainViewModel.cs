using GalaSoft.MvvmLight.CommandWpf;
using HeyKodi.Messages;
using HeyKodi.Model;
using HeyKodi.Tools;
using KodiRPC.Responses.VideoLibrary;
using KodiRPC.RPC.RequestResponse.Params.VideoLibrary;
using KodiRPC.RPC.Specifications;
using KodiRPC.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
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

        private MainViewModel()
        {
        }

        public void Init()
        {
            try
            {
                ShowConfigurationCommand = new RelayCommand(() => ShowConfiguration());
                ShowDocumentationCommand = new RelayCommand(() => ShowDocumentation());
                MinimizeCommand = new RelayCommand(() => Minimize());

                KodiService = new KodiService();

                KodiService.Config = HeyKodiConfig = HeyKodiConfigExtensions.Load();

                KodiSpeechRecognizer = new KodiSpeechRecognizer(HeyKodiConfig, KodiService);

                KodiSpeechRecognizer.PropertyChanged += KodiSpeechRecognizer_PropertyChanged;

                StartRecognition();

                SpeechSynthesizer = new SpeechSynthesizer();

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
                }
            }
            catch (Exception ex)
            {
                throw new CriticalApplicationException("Une erreur critique s'est produite lors de l'initialisation de Hey Kodi, l'application va se fermer.", ex);
            }
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

        public bool RunKodiCommand(HeyKodiCommandEnum command, string parameter = null, bool mute = false)
        {
            var result = false;

            try
            {
                if (command == HeyKodiCommandEnum.ShowHeyKodiConfig)
                {
                    ShowConfiguration();
                    result = true;
                }
                else if (command == HeyKodiCommandEnum.CancelHeyKodi)
                {
                    KodiSpeechRecognizer.Cancel(false);
                    result = true;
                }
                else
                {
                    try
                    {

                        List<ActivePlayer> players = null;

                        if (command == HeyKodiCommandEnum.Search)
                        {
                            if (!(RunKodiCommand(HeyKodiCommandEnum.Back, null, true) &&
                                RunKodiCommand(HeyKodiCommandEnum.Home, null, true)))
                            {
                                return false;
                            }
                        }
                        else if (command == HeyKodiCommandEnum.Search ||
                            command == HeyKodiCommandEnum.Home ||
                            command == HeyKodiCommandEnum.ShowGames ||
                            command == HeyKodiCommandEnum.ShowMusic ||
                            command == HeyKodiCommandEnum.ShowTV ||
                            command == HeyKodiCommandEnum.ShowVideos ||
                            command == HeyKodiCommandEnum.ShowWeather)
                        {
                            if (!RunKodiCommand(HeyKodiCommandEnum.Back, null, true))
                            {
                                return false;
                            }
                        }
                        else if (command == HeyKodiCommandEnum.Stop ||
                            command == HeyKodiCommandEnum.Play ||
                            command == HeyKodiCommandEnum.Pause)
                        {
                            players = KodiService.GetActivePlayers(new GetActivePlayersParams()).Result
                                .Where(p => p.PlayerType == "internal").ToList();
                        }

                        //var kodiApiMethod = HeyKodiConfigExtensions.CommandRepository[command].KodiApiMethod;

                        switch (command)
                        {
                            case HeyKodiCommandEnum.Search:
                                KodiService.ExecuteAddon(new ExecuteAddonParams()
                                {
                                    AddonId = "script.globalsearch",
                                    Wait = false,
                                    Params = new string[] { "searchstring=" + parameter }
                                });
                                break;
                            case HeyKodiCommandEnum.Home:
                                KodiService.InputHome();
                                break;
                            case HeyKodiCommandEnum.Back:
                                KodiService.InputBack();
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
                            case HeyKodiCommandEnum.MuteUnmute:
                                KodiService.SetMute(new SetMuteParams());
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
                            case HeyKodiCommandEnum.EjectOpticalDrive:
                                KodiService.EjectOpticalDrive(new EjectOpticalDriveParams());
                                break;
                            default:
                                throw new Exception($"Unknown command : {command}");
                        }

                        result = true;
                    }
                    finally
                    {
                        //KodiSpeechRecognizer.RecognizeAsync();
                    }
                }

                if (!mute)
                {
                    PlaySound(result ? KodiSpeechRecognizerSound.RunCommandSound : KodiSpeechRecognizerSound.CancelSound);
                }
            }
            catch (Exception ex)
            {
                var exPlus = new Exception("L'exécution de la commande Kodi a échoué.", ex);
                ShowMessage(ShowMessageType.Error, ex);
            }

            return result;
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
            if (mediaGrammar == null)
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
                        //.Select(t => new string(t.ToArray()
                        //    .Select(c => accents.ContainsKey(c) ? accents[c] : c)
                        //    //.Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == ' ')
                        //    .ToArray()))
                        //.Select(t => string.Join(" ", t.Split(' ').Where(w => !string.IsNullOrWhiteSpace(w)).Select(w => w.Trim())))
                        .Distinct()
                        .ToList();

                    var movieTitlesParts = moviesTitles
                        .Select(t => t.Split(ponctuations))
                        .Where(mt => mt.Length > 1)
                        .SelectMany(mt => mt)
                        .Select(tp => tp.Trim())
                        .Where(tp => !string.IsNullOrWhiteSpace(tp))
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
                        .Where(tp => !string.IsNullOrWhiteSpace(tp))
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
                        .Where(tp => !string.IsNullOrWhiteSpace(tp))
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
                        "Impossible de récupérer les titres des films / musiques présents dans Kodi : " + ex.ToMessageDetailed());
                }
            }

            grammar = HeyKodiConfig.Commands.Select(c => c.CommandSpeech).ToList();
            
            if (HeyKodiConfig.NeedHeyKodiWakeup && !string.IsNullOrWhiteSpace(HeyKodiConfig.KodiWakeupSpeech))
            {
                grammar.Add(HeyKodiConfig.KodiWakeupSpeech);
            }

            if (mediaGrammar != null)
            {
                grammar.AddRange(mediaGrammar);
            }

            grammar = grammar.Distinct().Where(g => g.Length > 3).Take(800).ToList();
        }

        public void StartRecognition()
        {
            BuildGrammar();
            KodiSpeechRecognizer.Start(grammar);
        }

        public void StopRecognition()
        {
            KodiSpeechRecognizer.Stop();
        }

        private void ShowDocumentation()
        {
            var docPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "HeyKodi.pdf");
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
