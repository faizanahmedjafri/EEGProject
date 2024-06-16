using System;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using Xamarin.Forms;

namespace biopot.Controls
{
    public partial class RecordControl : ContentView
    {
        private const string IMAGE_SOURCE_REC = "btn_rec";
        private const string IMAGE_SOURCE_REC_STOP = "btn_stop";
        private const string IMAGE_SOURCE_REC_DISABLED = "btn_rec_inacive";

        private const int TIMER_WAIT = 1000;
        private Timer _timer;

        public RecordControl()
        {
            InitializeComponent();
            ImageSource = IMAGE_SOURCE_REC;
        }

        #region --Public properties--

        private ICommand _clickedCommand;
        public ICommand ClickedCommand => _clickedCommand ?? (_clickedCommand = new Command(OnClickedCommand));

        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(RecordControl), default(ImageSource));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly BindableProperty IsRecordingProperty =
            BindableProperty.Create(nameof(IsRecording), typeof(bool), typeof(RecordControl), default(bool));

        public bool IsRecording
        {
            get { return (bool)GetValue(IsRecordingProperty); }
            set { SetValue(IsRecordingProperty, value); }
        }

        public static readonly BindableProperty IsConnectedProperty =
            BindableProperty.Create(nameof(IsConnected), typeof(bool), typeof(RecordControl), default(bool));

        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        public static readonly BindableProperty TimeProperty =
            BindableProperty.Create(nameof(Time), typeof(DateTime), typeof(RecordControl), default(DateTime));

        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly BindableProperty StartRecordCommandProperty =
            BindableProperty.Create(nameof(StartRecordCommand), typeof(ICommand), typeof(RecordControl), default(ICommand));

        public ICommand StartRecordCommand
        {
            get { return (ICommand)GetValue(StartRecordCommandProperty); }
            set { SetValue(StartRecordCommandProperty, value); }
        }

        public static readonly BindableProperty StopRecordCommandProperty =
            BindableProperty.Create(nameof(StopRecordCommand), typeof(ICommand), typeof(RecordControl), default(ICommand));

        public ICommand StopRecordCommand
        {
            get { return (ICommand)GetValue(StopRecordCommandProperty); }
            set { SetValue(StopRecordCommandProperty, value); }
        }

        #endregion

        #region --Overrides--

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsRecording))
            {
                ChangeRecord();
            }
            else if (propertyName == nameof(IsConnected))
            {
                SetImageSource();
            }
        }

        #endregion

        #region --Private helpers--

        private void OnClickedCommand()
        {
            if (IsConnected)
                IsRecording = !IsRecording;
        }

        private void ChangeRecord()
        {
            if (IsRecording)
            {
                if (StartRecordCommand != null)
                    StartRecordCommand?.Execute(null);
            }
            else
            {
                if (StopRecordCommand != null)
                    StopRecordCommand?.Execute(null);
            }

            SetImageSource();
            StartStopTimer();
        }

        private void SetImageSource()
        {
            if (!IsConnected)
            {
                ImageSource = IMAGE_SOURCE_REC_DISABLED;
            }
            else if (IsRecording)
            {
                ImageSource = IMAGE_SOURCE_REC_STOP;
            }
            else
            {
                ImageSource = IMAGE_SOURCE_REC;
            }
        }

        private void StartStopTimer()
        {
            if (IsRecording)
            {
                Time = DateTime.MinValue;
                _timer = new Timer();
                _timer.Enabled = true;
                _timer.Interval = TIMER_WAIT;
                _timer.Elapsed += Timer_Elapsed;
                _timer.Start();
            }
            else
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Time = Time.AddSeconds(1);
        }

        #endregion

    }
}