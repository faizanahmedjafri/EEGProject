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
using System.Linq;
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
        List<string> audioList = new List<string> { "oddball_sequence_6_minutes_10_percent.wav", "oddball_sequence_6_minutes_20%.wav" };
        private TimeSpan _remainingTime;
        private double _timerInterval = 10;

        public AudioRecognitionViewModel(ILogService logService, INavigationService navigationService, IMediaService mediaService)
        {
            _logService = logService;
            _navigationService = navigationService;
            _mediaService = mediaService;
            _player = CrossSimpleAudioPlayer.Current;

            _remainingTime = TimeSpan.FromMinutes(6);
            _timer = new Timer(_timerInterval);
            _timer.Elapsed += TimerElapsed;
            _time = _remainingTime.TotalMilliseconds;
        }

        #region -- Public properties --
        private double _time;
        public double Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private string _audioName;
        public string AudioName
        {
            get => _audioName;
            set => SetProperty(ref _audioName, value);
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

        public Dictionary<double, bool> PitchRecordDict { get; set; } = new Dictionary<double, bool>();
        #endregion

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_remainingTime.TotalMilliseconds <= 0)
            {
                resetTimer();
                Device.BeginInvokeOnMainThread(() => {
                    OnEndTaskCommand();
                });
                return;
            }

            _remainingTime = _remainingTime.Subtract(TimeSpan.FromMilliseconds(_timerInterval));
            Time = _remainingTime.TotalMilliseconds;
        }

        private void resetTimer()
        {
            _time = 0;
            Timer.Stop();
        }

        private Task OnPreviousCommand()
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Previous button Navigate from Audio recognition page");
            return _navigationService.GoBackAsync();
        }

        private Task OnEndTaskCommand()
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: End Task button Navigate from Audio recognition page");

            NavigationParameters navigationParameters = new NavigationParameters();
            navigationParameters.Add("EndTask", true);

            string result = string.Join("\r\n", PitchRecordDict.Select(kvp => $"{kvp.Key} : {kvp.Value}"));
            navigationParameters.Add("AudioData", result);
            navigationParameters.Add("AudioName", _audioName);

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

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            resetTimer();
            _mediaService.Stop();
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            Random random = new Random();
            _audioName = audioList[random.Next(audioList.Count)];

            _mediaService.LoadFileStream($"biopot.Resources.Audios.{_audioName}");
            _mediaService.Play();
            _timer.Start();
        }
    }
}
