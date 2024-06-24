using biopot.Helpers;
using biopot.Models;
using biopot.Services.MediaService;
using biopot.Views;
using Plugin.SimpleAudioPlayer;
using Prism.Commands;
using Prism.Navigation;
using SharedCore.Services;
using SharedCore.Services.MediaService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Xamarin.Forms;

namespace biopot.ViewModels
{
    public class AudioRecognitionViewModel : BaseViewModel
    {
        private readonly ILogService _logService;
        private readonly INavigationService _navigationService;
        private readonly IMediaService _mediaService;
        private Timer _timer;
        private ISimpleAudioPlayer _player;

        public AudioRecognitionViewModel(ILogService logService, INavigationService navigationService, IMediaService mediaService)
        {
            _logService = logService;
            _navigationService = navigationService;
            _mediaService = mediaService;
            _player = CrossSimpleAudioPlayer.Current;

            _timer = new Timer();
            _timer.Interval = 1000; // 1 second interval
            _timer.Elapsed += TimerElapsed;
        }

        #region -- Public properties --
        private int _time = 360;
        public int Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        public Timer Timer { get => _timer; }

        private ICommand _PreviousCommand;
        public ICommand PreviousCommand => _PreviousCommand ?? (_PreviousCommand = SingleExecutionCommand.FromFunc(OnPreviousCommand));

        private ICommand _EndTaskCommand;
        public ICommand EndTaskCommand => _EndTaskCommand ?? (_EndTaskCommand = SingleExecutionCommand.FromFunc(OnEndTaskCommand));

        private ICommand _HighPitchCommand;
        public ICommand HighPitchCommand => _HighPitchCommand ?? (_HighPitchCommand = SingleExecutionCommand.FromFunc(OnHighPitchCommand));

        private ICommand _LowPitchCommand;
        public ICommand LowPitchCommand => _LowPitchCommand ?? (_LowPitchCommand = SingleExecutionCommand.FromFunc(OnLowPitchCommand));

        public Dictionary<int, bool> PitchRecordDict { get; set; } = new Dictionary<int, bool>();
        #endregion

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Time--;
            if (Time == 0)
            {
                _timer.Stop();
            }
        }

        private void resetTimer()
        {
            _time = 360;
            Timer.Stop();
        }

        private Task OnPreviousCommand()
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Previous button Navigate from Audio recognition page");
            _mediaService.Stop();
            resetTimer();
            return _navigationService.GoBackAsync();
        }

        private Task OnEndTaskCommand()
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: End Task button Navigate from Audio recognition page");
            _mediaService.Stop();
            resetTimer();

            NavigationParameters navigationParameters = new NavigationParameters();
            navigationParameters.Add("EndTask", true);
            return _navigationService.NavigateAsync('/' + nameof(NavigationPage) + '/' + nameof(MainView), navigationParameters);
        }

        private Task OnHighPitchCommand()
        {
            PitchRecordDict[Time] = true;
            return Task.FromResult(0);
        }

        private Task OnLowPitchCommand()
        {
            PitchRecordDict[Time] = false;
            return Task.FromResult(0);
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            parameters.TryGetValue("AudioName", out string audioName);
            _mediaService.LoadFileStream($"biopot.Resources.Audios.{audioName}");
            _mediaService.Play();
            _timer.Start();
        }
    }
}
