using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using biopot.Helpers;
using biopot.Views;
using Prism.Navigation;
using SharedCore.Services;
using Xamarin.Forms;
using SharedCore.Services.Charts;
using System;
using System.Linq;
using biopot.Services;
using biopot.Services.Charts;
using SharedCore.Services.RateInfoService;
using SharedCore.Services.TemperatureDataService;

namespace biopot.ViewModels
{
	public class SetupViewModel : BaseViewModel
	{
	    private const string DefaultSwLowPassFilterValue = "Cutoff 20hz";

        private const string NotchFilterNoFilter = "No filter";
		private const string NotchFilter50Hz = "50Hz";
		private const string NotchFilter60Hz = "60Hz";
		private const string SamplingRate250Sps = "250";
		private const string SamplingRate500Sps = "500";
		private const string SamplingRate1000Sps = "1000";
		private const string SamplingRate2000Sps = "2000";

		private static readonly IReadOnlyList<string> DefaultSamplingRates = new[]
			{SamplingRate250Sps, SamplingRate500Sps, SamplingRate1000Sps, SamplingRate2000Sps};

		private readonly INavigationService _navigationService;
		private readonly IUserDialogs _userDialogs;
		private readonly IBlueToothService _blueToothService;
		private readonly ILogService _logService;
		private readonly IBiopotInfoChartsService _biopotInfoService;
		private readonly IBiobotDataForChartsService _biopotDataForChartsService;
		private readonly ITemperatureDataService _batteryDataService;
		private readonly ISamplesRateInfoService _samplesRateInfoService;
		private readonly IChartManagerService _chartManagerService;
		private readonly IAppSettingsManagerService _settingsService;

		public SetupViewModel(IUserDialogs userDialogs,
							  INavigationService navigationService,
							  IBlueToothService blueToothService,
							  ILogService logService,
							  IBiopotInfoChartsService biopotInfoService,
							  IBiobotDataForChartsService biopotDataForChartsService,
                              ITemperatureDataService batteryDataService,
							  ISamplesRateInfoService samplesRateInfoService,
							  IChartManagerService chartManagerService,
							  IAppSettingsManagerService settingsService)
		{
			_navigationService = navigationService;
			_userDialogs = userDialogs;
			_blueToothService = blueToothService;
			_logService = logService;
			_biopotInfoService = biopotInfoService;
			_biopotDataForChartsService = biopotDataForChartsService;
			_batteryDataService = batteryDataService;
			_samplesRateInfoService = samplesRateInfoService;
			_chartManagerService = chartManagerService;
			_settingsService = settingsService;

		    
			Subscribe();
		}

		#region -- Public properties --

		private bool _isBluetoothOn = true;
		public bool IsBluetoothOn
		{
			get { return _isBluetoothOn; }
			set { SetProperty(ref _isBluetoothOn, value); }
		}

		private int _StripTabIndex;
		public int StripTabIndex
		{
			get { return _StripTabIndex; }
			set { SetProperty(ref _StripTabIndex, value); }
		}

		private bool _isConnectionLost;
		public bool IsConnectionLost
		{
			get => _isConnectionLost;
			set { SetProperty(ref _isConnectionLost, value); }
		}

	    private bool _isEraseModeActive;
	    public bool IsEraseModeActive
	    {
	        get => _isEraseModeActive;
	        set => SetProperty(ref _isEraseModeActive, value);
	    }

        private IList<string> _LPFTargetsList = new List<string>()
        {
            "100 Hz", "150 Hz", "200 Hz", "250 Hz",
            "300 Hz", "500 Hz", "750 Hz", "1.0 kHz",
            "1.5 kHz", "2.0 kHz", "2.5 kHz", "3.0 kHz",
            "5.0 kHz", "7.5 kHz", "10 kHz", "15 kHz",
            "20 kHz"
        };
		public IList<string> LPFTargetsList
		{
			get { return _LPFTargetsList; }
			private set { SetProperty(ref _LPFTargetsList, value); }
		}

		private IList<string> _IPITargetsList = new List<string>() { "Data_1", "Data_2", "Data_3", "Data_4" };
		public IList<string> IPITargetsList
		{
			get { return _IPITargetsList; }
			private set { SetProperty(ref _IPITargetsList, value); }
		}


		private IList<string> _RateTargetsList = DefaultSamplingRates.ToList();
		public IList<string> RateTargetsList
		{
			get { return _RateTargetsList; }
			private set { SetProperty(ref _RateTargetsList, value); }
		}

		private IList<string> _NotchTargetsList = new List<string>() { NotchFilterNoFilter, NotchFilter50Hz, NotchFilter60Hz };
		public IList<string> NotchTargetsList
		{
			get { return _NotchTargetsList; }
			private set { SetProperty(ref _NotchTargetsList, value); }
		}

		private IList<string> _HPFTargetsList = new List<string>()
	    {
	        "0.10 Hz", "0.25 Hz", "0.30 Hz", "0.50 Hz",
	        "0.75 Hz", "1.0 Hz", "1.5 Hz", "2.0 Hz",
	        "2.5 Hz", "3.0 Hz", "5.0 Hz", "7.5 Hz",
	        "10 Hz", "15 Hz", "20 Hz", "25 Hz",
	        "30 Hz", "50 Hz", "75 Hz", "100 Hz",
	        "150 Hz", "200 Hz", "250 Hz", "300 Hz",
	        "500 Hz"
        };
		public IList<string> HPFTargetsList
		{
			get { return _HPFTargetsList; }
			private set { SetProperty(ref _HPFTargetsList, value); }
		}

		private IList<string> _SLPFTargetsList = new[] {"No LPF"}
			.Concat(Enumerable.Range(5, 100 - 5 + 1)
				.Where(x => x % 5 == 0)
				.Select(x => $"Cutoff {x}hz"))
			.ToList();
		public IList<string> SLPFTargetsList
		{
			get { return _SLPFTargetsList; }
		    private set
		    {
		        if(SetProperty(ref _SLPFTargetsList, value))
		        {
		            AreSettingsChanged = true;
		        }
		    }
		}

        private string _LPFTarget = "100 Hz";
		public string LPFTarget
		{
			get { return _LPFTarget; }
		    set
		    {
		        if (SetProperty(ref _LPFTarget, value))
		        {
		            AreSettingsChanged = true;
		        }
		    }
		}

	    private string _IPITarget = "Data_1";
        public string IPITarget
		{
			get { return _IPITarget; }
			set { SetProperty(ref _IPITarget, value); }
		}

		private string _RateTarget = SamplingRate500Sps;
		public string RateTarget
		{
			get { return _RateTarget; }
			set
			{
				if (SetProperty(ref _RateTarget, value))
				{
					AreSettingsChanged = true;
				}
			}
		}

		private string _NotchTarget = NotchFilterNoFilter;
		public string NotchTarget
		{
			get { return _NotchTarget; }
			set
			{
				if (SetProperty(ref _NotchTarget, value))
				{
					AreSettingsChanged = true;
				}
			}
		}

		private string _HPFTarget = "0.10 Hz";
		public string HPFTarget
		{
			get { return _HPFTarget; }
			set {
				if (SetProperty(ref _HPFTarget, value))
				{
					AreSettingsChanged = true;
				}
			}
		}

		private string _SLPFTarget = DefaultSwLowPassFilterValue;
		public string SLPFTarget
		{
			get { return _SLPFTarget; }
			set
			{
				if (SetProperty(ref _SLPFTarget, value))
				{
					AreSettingsChanged = true;
				}
			}
		}

		private string _batteryStatusValue = "Value";
		public string BatteryStatusValue
		{
			get => _batteryStatusValue;
		    set
			{
			    if (_batteryStatusValue != value)
			    {
			        SetProperty(ref _batteryStatusValue, value);
                }
			}
		}

		private string _temperatureValue = "0°C";
		public string TemperatureValue
		{
			get => _temperatureValue;
		    set
			{
			    if (_temperatureValue != value)
			    {
			        SetProperty(ref _temperatureValue, value);
                }
			}
		}

	    private string _synchRatio;
	    public string SynchRatio
	    {
	        get => _synchRatio;
	        set
	        {
	            if (_synchRatio != value)
	            {
	                SetProperty(ref _synchRatio, value);
                }
	        }
	    }

        private bool _AreSettingsChanged = false;
		public bool AreSettingsChanged
		{
			get => _AreSettingsChanged;
			set => SetProperty(ref _AreSettingsChanged, value);
		}

	    private bool _isAdditionalSettingsActiveTab = true;
	    public bool IsAdditionalSettingsActiveTab
        {
	        get => _isAdditionalSettingsActiveTab;
	        set => SetProperty(ref _isAdditionalSettingsActiveTab, value);
	    }

        private ICommand _BackCommand;
		public ICommand BackCommand => _BackCommand ?? (_BackCommand = SingleExecutionCommand.FromFunc(OnBackCommandAsync));

		private ICommand _ApplyCommand;
		public ICommand ApplyCommand => _ApplyCommand ?? (_ApplyCommand = SingleExecutionCommand.FromFunc(OnApplyCommandAsync));

	    private ICommand _refreshCommand;
	    public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = SingleExecutionCommand.FromFunc(OnRefreshCommandAsync));

		#endregion

		#region -- Overrides --

		/// <inheritdoc />
		public override void OnAppearing()
		{
			base.OnAppearing();

			Device.BeginInvokeOnMainThread(async () =>
			{
                ReadDeviceRedoutValues();
			});
		}

		/// <inheritdoc />
		public override void Destroy()
		{
			base.Destroy();
			Unsubscribe();
		}

		#endregion

		#region -- Private helpers --

        /// <summary>
        /// Reads the temperature, battery status and sycn ration values.
        /// </summary>
	    private void ReadDeviceRedoutValues()
	    {
	        ReadBatteryStatusValue();
	        ReadTemperatureValue();
	        ReadSynchRatio();
        }

		private void ReadBatteryStatusValue()
		{
			BatteryStatusValue = $"{_batteryDataService.BatteryActualValue}%";
			_logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Battery Status Value = {BatteryStatusValue}");
		}

		private void ReadTemperatureValue()
		{
            TemperatureValue = $"{_batteryDataService.TemperatureActualValue}°C";
			_logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Temperature Value = {TemperatureValue }");
		}

        /// <summary>
        /// Reads 
        /// </summary>
        /// <returns></returns>
	    private void ReadSynchRatio()
        {
            SynchRatio = $"{_batteryDataService.SynchRatioActualValue:F8}";//H.H. sync ratio looks like 1.0000XXX
            _logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Synch Ratio = {SynchRatio }");
	    }

        private async Task OnBackCommandAsync()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Back command from Setup View");

		    NavigationParameters keys = new NavigationParameters();
		    keys.Add(Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN, true);

            if (AreSettingsChanged)
			{
				var confirmedDiscard = await _userDialogs.ConfirmAsync(
					"Settings have changed. Do you want to discard them?",
					okText: "Discard",
					cancelText: "Cancel");
				if (confirmedDiscard)
				{
					await _navigationService.GoBackAsync(keys);
				}
			}
			else
			{
				await _navigationService.GoBackAsync(keys);
			}
		}

		private async Task OnApplyCommandAsync()
		{
            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Apply Command. On Setup View");

			await ApplySettingsAsync();

		    NavigationParameters keys = new NavigationParameters();
		    keys.Add(Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN, true);

            await _navigationService.GoBackAsync(keys);
		}

        /// <summary>
        /// Refreshes 'Device Readout' values.
        /// </summary>
        /// <returns></returns>
	    private async Task OnRefreshCommandAsync()
	    {
	        await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Refresh Command. On Setup View");

            ReadDeviceRedoutValues();
	    }

        private async Task OnReadSettingsAsync()
		{
			switch (_chartManagerService.CurrentSignalFilterType)
			{
				case SignalFilterType.NotchFilter50Hz:
					NotchTarget = NotchFilter50Hz;
					break;
				case SignalFilterType.NotchFilter60Hz:
					NotchTarget = NotchFilter60Hz;
					break;
				case SignalFilterType.NoFilter:
				default:
					NotchTarget = NotchFilterNoFilter;
					break;
			}

			var rateResult = await _samplesRateInfoService.GetSamplingRateAsync();
			if (rateResult.IsSuccess && rateResult.Result > 0)
			{
				RateTarget = rateResult.Result.ToString();
			}
			else
			{
				RateTarget = SamplingRate500Sps;
			}

			var swLowPassResult = await _samplesRateInfoService.GetSoftwareLowPassFilterAsync();
			if (swLowPassResult.IsSuccess && swLowPassResult.Result >= 0)
			{
				SLPFTarget = SLPFTargetsList.ElementAtOrDefault(swLowPassResult.Result) ?? DefaultSwLowPassFilterValue;
			}
			else
			{
				SLPFTarget = DefaultSwLowPassFilterValue;
			}

		    var hwLowPassResult = await _samplesRateInfoService.GetHardwareLowPassFilterAsync();
		    if (hwLowPassResult.IsSuccess && hwLowPassResult.Result >= 0)
		    {
		        LPFTarget = LPFTargetsList.ElementAtOrDefault(hwLowPassResult.Result) ?? "No LPF";
		    }
		    else
		    {
		        LPFTarget = "No LPF";
		    }

		    var hwHighPassResult = await _samplesRateInfoService.GetHardwareHighPassFilterAsync();
		    if (hwHighPassResult.IsSuccess && hwHighPassResult.Result >= 0)
		    {
		        HPFTarget = HPFTargetsList.ElementAtOrDefault(hwHighPassResult.Result) ?? "No HPF";
		    }
		    else
		    {
		        HPFTarget = "No HPF";
		    }

            AreSettingsChanged = false;
		}

		private async Task ApplySettingsAsync()
		{
			ApplyNotchFilter();

            /* Software Low Pass Filter */
			var swLowPassFilterIndex = SLPFTargetsList.IndexOf(SLPFTarget);
			if (swLowPassFilterIndex < 0)
			{
				swLowPassFilterIndex = 0;
			}

			await _samplesRateInfoService.SetSoftwareLowPassFilterAsync(swLowPassFilterIndex);

		    /* Hardware Low Pass Filter */
            var hwLowPassFilterIndex = LPFTargetsList.IndexOf(LPFTarget);
		    if (hwLowPassFilterIndex < 0)
		    {
		        hwLowPassFilterIndex = 0;
		    }

		    await _samplesRateInfoService.SetHardwareLowPassFilterAsync(hwLowPassFilterIndex);

		    /* Hardware High Pass Filter */
            var hwHighPassFilterIndex = HPFTargetsList.IndexOf(HPFTarget);
		    if (hwHighPassFilterIndex < 0)
		    {
		        hwHighPassFilterIndex = 0;
		    }

		    await _samplesRateInfoService.SetHardwareHighPassFilterAsync(hwHighPassFilterIndex);

		    int samplingRate = int.Parse(RateTarget);
			await _samplesRateInfoService.SetSamplingRateAsync(samplingRate);
		}

		/// <summary>
		/// Saves notch filter parameters.
		/// </summary>
		private void ApplyNotchFilter()
		{
			switch (NotchTarget)
			{
				case NotchFilter50Hz:
					_chartManagerService.SetSignalFilter(SignalFilterType.NotchFilter50Hz, int.Parse(RateTarget));
					break;
				case NotchFilter60Hz:
					_chartManagerService.SetSignalFilter(SignalFilterType.NotchFilter60Hz, int.Parse(RateTarget));
					break;
				default:
					_chartManagerService.SetSignalFilter(SignalFilterType.NoFilter, 0);
					break;
			}
		}

		private async void Subscribe()
		{
		    IsEraseModeActive = _batteryDataService.IsEraseModeActive;

            _blueToothService.OnChangedDeviceConnection += OnDeviceConnectionChanged;
		    _batteryDataService.OnEraseModeChanged += OnEraseModeChanged;

			await Task.Run(async () =>
			{
				await OnReadSettingsAsync();
			});
		}

		private void Unsubscribe()
		{
			_blueToothService.OnChangedDeviceConnection -= OnDeviceConnectionChanged;
		    _batteryDataService.OnEraseModeChanged -= OnEraseModeChanged;
		}

	    /// <summary>
	    /// Handles changes of erase mode.
	    /// </summary>
	    /// <param name="aSender"> The event sender. </param>
	    /// <param name="aIsEraseModeActive"> True - erase mode is active, otherwise - false. </param>
	    private void OnEraseModeChanged(object aSender, bool aIsEraseModeActive)
	    {
	        Device.BeginInvokeOnMainThread(() =>
	        {
	            IsEraseModeActive = aIsEraseModeActive;
	            _logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Erase Mode Changed. Is active = {aIsEraseModeActive}");
	        });
	    }

        private void OnDeviceConnectionChanged(object sender, bool isConnected)
		{
			if (!isConnected)
			{
				_logService.WriteInFileAsync();
			}
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Device Connection State isConnected = {isConnected}");
		}

		private async Task DisconnectionNavigation()
		{
			NavigationParameters navigationParameters = new NavigationParameters();
			navigationParameters.Add("FromChartsView", true);

			await _navigationService.NavigateAsync('/' + nameof(NavigationPage) + '/' + nameof(MainView), navigationParameters);
		}

		#endregion
	}
}
