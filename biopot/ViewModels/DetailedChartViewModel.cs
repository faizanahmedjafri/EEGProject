using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using biopot.Converters;
using biopot.Enums;
using biopot.Helpers;
using biopot.Models;
using biopot.Resources.Strings;
using biopot.Services;
using biopot.Services.Charts;
using biopot.Views;
using Prism.Commands;
using Prism.Navigation;
using SharedCore.Models;
using SharedCore.Services;
using SharedCore.Services.TemperatureDataService;
using Xamarin.Forms;

namespace biopot.ViewModels
{
	public class DetailedChartViewModel : BaseViewModel
	{
        private readonly INavigationService _navigationService;
		private readonly IChartManagerService _chartsService;
		private readonly IAppSettingsManagerService _appSettingsManagerService;
	    private readonly ITemperatureDataService _temperatureService;
		private readonly IBlueToothService _blueToothService;
		private readonly IUserDialogs _userDialogs;
		private readonly ILogService _logService;

		private GriddedChartViewModel _EEMorEMG;
		private GriddedChartViewModel _BioImpedance;
		private GriddedChartViewModel _Accelerometer;

		private int _StripTabIndex;
		private bool _isFromFileBrowser;

		public DetailedChartViewModel(INavigationService navigationService,
									  IChartManagerService chartsService,
									  IAppSettingsManagerService appSettingsManagerService,
                                      ITemperatureDataService temperatureService,
									  IBlueToothService blueToothService,
									  IUserDialogs userDialogs,
									  ILogService logService)
		{
			_navigationService = navigationService;
			_chartsService = chartsService;
			_appSettingsManagerService = appSettingsManagerService;
		    _temperatureService = temperatureService;
			_blueToothService = blueToothService;
			_userDialogs = userDialogs;
			_logService = logService;

			LoadData();

			ChartSps = new ChartViewModel(_chartsService, EDeviceType.SPSValue, 0, _navigationService)
			{
				ViewportHalfY = Constants.Charts.ChartYAxisDefaultValue,
            };

			Subscribe();
		}

		public override void Destroy()
		{
            Unsubscribe();
            base.Destroy();
		}

		private void LoadData()
		{
			Task.Run(async () =>
			{
				var _DeviceModel = await _appSettingsManagerService.GetObjectAsync<DeviceModel>(Constants.StorageKeys.CONNECTED_DEVICE);
				if (_DeviceModel != null)
					this.DeviceName = _DeviceModel.DeviceName;
				_EEMorEMG = new GriddedChartViewModel(_chartsService, _navigationService)
				{
					DeviceType = EDeviceType.EEGorEMG
				};
				_BioImpedance = new GriddedChartViewModel(_chartsService, _navigationService)
				{
					DeviceType = EDeviceType.BioImpedance
				};
				_Accelerometer = new GriddedChartViewModel(_chartsService, _navigationService)
				{
					DeviceType = EDeviceType.Accelerometer
				};

				_EEMorEMG.ChartViewModel.ChartColor = SkiaSharp.SKColor.Parse("#FF4680F2");
				_BioImpedance.ChartViewModel.ChartColor = SkiaSharp.SKColor.Parse("#FF4680F2");
				_Accelerometer.ChartViewModel.ChartColor = SkiaSharp.SKColor.Parse("#FF4680F2");

			}).ConfigureAwait(false);
		}

		public Task OnBackCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Back Command from Chart Details pressed");

			NavigationParameters keys = new NavigationParameters();

            // get the max scale
		    var maxXScaleValue = EEMorEMG.XScaleValue;
            if (BioImpedance.HasChannels && Accelerometer.HasChannels)
            {
                maxXScaleValue = Math.Max(EEMorEMG.XScaleValue,
                    Math.Max(BioImpedance.XScaleValue, Accelerometer.XScaleValue));

            } else if (BioImpedance.HasChannels)
            {
                maxXScaleValue = Math.Max(EEMorEMG.XScaleValue, BioImpedance.XScaleValue);
            } else if (Accelerometer.HasChannels)
		    {
		        maxXScaleValue = Math.Max(EEMorEMG.XScaleValue, Accelerometer.XScaleValue);
            }

            IDictionary<EDeviceType, int> scaleValues = new Dictionary<EDeviceType, int>
			{
				{EDeviceType.EEGorEMG, EEMorEMG.PickerValue},
				{EDeviceType.BioImpedance, BioImpedance.PickerValue},
				{EDeviceType.Accelerometer, Accelerometer.PickerValue},
			};
			keys.Add(Constants.NavigationParamsKeys.SCALE_VALUES, scaleValues);
			keys.Add(Constants.NavigationParamsKeys.X_SCALE_VALUE, maxXScaleValue);
			keys.Add(Constants.NavigationParamsKeys.NAV_FROM_FILE_BROWSER, _isFromFileBrowser);
		    keys.Add(Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN, true);

            return _navigationService.GoBackAsync(keys);
		}

		#region -- Public properties --

		private bool _isSpsChartVisible = false;
		public bool IsSpsChartVisible
		{
			get { return _isSpsChartVisible; }
			set { SetProperty(ref _isSpsChartVisible, value); }
		}

		private ChartViewModel _chartSps;
		public ChartViewModel ChartSps
		{
			get { return _chartSps; }
			set { SetProperty(ref _chartSps, value); }
		}

		public GriddedChartViewModel EEMorEMG
		{
			get { return _EEMorEMG; }
			set { SetProperty(ref _EEMorEMG, value); }
		}
		public GriddedChartViewModel BioImpedance
		{
			get { return _BioImpedance; }
			set { SetProperty(ref _BioImpedance, value); }
		}
		public GriddedChartViewModel Accelerometer
		{
			get { return _Accelerometer; }
			set { SetProperty(ref _Accelerometer, value); }
		}
		public int StripTabIndex
		{
			get { return _StripTabIndex; }
			set { SetProperty(ref _StripTabIndex, value); }
		}

		private IList<SensorConnectionModel> _SensorConnectionList;
		public IList<SensorConnectionModel> SensorConnectionList
		{
			get { return _SensorConnectionList; }
			set { SetProperty(ref _SensorConnectionList, value); }
		}

		private ESensorConnectionState _SensorsConnectionState;
		public ESensorConnectionState SensorsConnectionState
		{
			get { return _SensorsConnectionState; }
			set { SetProperty(ref _SensorsConnectionState, value); }
		}

		private string _DeviceName;
		public string DeviceName
		{
			get => _DeviceName;
			set { SetProperty(ref _DeviceName, value); }
		}

		private int _batteryLevel;
		public int BatteryLevel
		{
			get => _batteryLevel;
		    set
		    {
                if (SetProperty(ref _batteryLevel, value))
                {
                    RaisePropertyChanged(nameof(BatteryImage));
                    RaisePropertyChanged(nameof(BatteryImageColor));
                }
		    }
		}

	    public ImageSource BatteryImage
	    {
	        get
	        {
	            if (BatteryLevel >= 60)
	            {
	                return "battery_full";
	            }
	            if (BatteryLevel >= 40)
	            {
	                return "battery_half_full";
	            }

                // the image is animated on the view level
	            return "battery_empty";
	        }
	    }

        public Color BatteryImageColor
	    {
	        get
	        {
	            if (BatteryLevel >= 40)
	            {
	                return Color.DarkGray;
	            }

	            return Color.Red;
            }
	    }

        private int _temperature;
	    public int Temperature
	    {
	        get => _temperature;
	        set => SetProperty(ref _temperature, value);
	    }

	    private bool _isEraseModeActive;
	    public bool IsEraseModeActive
	    {
	        get => _isEraseModeActive;
	        set => SetProperty(ref _isEraseModeActive, value);
	    }

        private bool _isConnectionLost;
		public bool IsConnectionLost
		{
			get => _isConnectionLost;
			set { SetProperty(ref _isConnectionLost, value); }
		}

		private bool _isBluetoothOn = true;
		public bool IsBluetoothOn
		{
			get { return _isBluetoothOn; }
			set { SetProperty(ref _isBluetoothOn, value); }
		}

	    public bool CanOpenAnotherScreen { get; private set; } = true;

	    private ICommand _BackCommand;
		public ICommand BackCommand => _BackCommand ?? (_BackCommand = SingleExecutionCommand.FromFunc(OnBackCommand));

		private ICommand _ConnectionStateCommand;
		public ICommand ConnectionStateCommand => _ConnectionStateCommand ?? (_ConnectionStateCommand = SingleExecutionCommand.FromFunc(OnConnectionStateCommand));

		private ICommand _BandWidthCommand;
		public ICommand BandWidthCommand => _BandWidthCommand ?? (_BandWidthCommand = SingleExecutionCommand.FromFunc(OnBandWidthCommand));

		private ICommand _OpenFolderCommand;
		public ICommand OpenFolderCommand => _OpenFolderCommand ?? (_OpenFolderCommand = SingleExecutionCommand.FromFunc(OnOpenFolderCommand));

	    private ICommand _additionalSettingsCommand;
	    public ICommand AdditionalSettingsCommand => _additionalSettingsCommand ?? (_additionalSettingsCommand = new DelegateCommand(OnAdditionalSettingsCommand)
            .ObservesCanExecute(() => CanOpenAnotherScreen));

	    private ICommand _technicalSettingsCommand;
	    public ICommand TechnicalSettingsCommand => _technicalSettingsCommand ?? (_technicalSettingsCommand = new DelegateCommand(OnTechnicalSettingsCommand)
	                                                    .ObservesCanExecute(() => CanOpenAnotherScreen));

        private ICommand _closeSpsDetailsCommand;
	    public ICommand CloseSpsDetailsCommand => _closeSpsDetailsCommand ?? (_closeSpsDetailsCommand = new Command(OnCloseSpsDetailsCommand));
        #endregion

        #region -- Overrides --

        public override async void OnNavigatingTo(NavigationParameters parameters)
        {
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigating to detail chart view");

		    parameters.TryGetValue(Constants.NavigationParamsKeys.NAV_FROM_FILE_BROWSER, out bool isFromFileBrowser);
		    _isFromFileBrowser = isFromFileBrowser;

		    if (parameters.TryGetValue(Constants.NavigationParamsKeys.NAV_FROM_IMPEDANCE, out bool isFromImpedance)
		        && isFromImpedance)
		    {
		        // restart the charts data instead of impedance
		        await _chartsService.StartAsync(CaptureMode.CaptureEegEmg);
		    }

		    if (_blueToothService.CurrentDevice != null && _blueToothService.CurrentDevice.State != Plugin.BLE.Abstractions.DeviceState.Connected)
		    {
		        IsConnectionLost = true;
		        _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Lost Device Connection");
		    }

            // not read navigation params when user returns back to this screen from another one.
            var isReturnedBack = parameters.GetValue<bool>(Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN);
		    if (isReturnedBack)
		    {
		        CanOpenAnotherScreen = true;
		        return;
		    }

			var chartId = parameters.GetValue<int>(Constants.NavigationParamsKeys.CHART_ID);
			var deviceType = parameters.GetValue<EDeviceType>(Constants.NavigationParamsKeys.DEVICE_TYPE);
			var deviceName = parameters.GetValue<string>(Constants.NavigationParamsKeys.DEVICE_NAME);
			var isAccelerometerPresent = parameters.GetValue<bool>(Constants.NavigationParamsKeys.IS_ACCELEROMETER);
            var accelerometerMode = parameters.GetValue<uint>(Constants.NavigationParamsKeys.ACC_MODE);
            var isBioImpedancePresent = parameters.GetValue<bool>(Constants.NavigationParamsKeys.IS_BIO_IMPEDANCE);
		    var xScaleValue = parameters.GetValue<int>(Constants.NavigationParamsKeys.X_SCALE_VALUE);

		    EEMorEMG.XScaleValue = xScaleValue;
		    BioImpedance.XScaleValue = xScaleValue;
		    Accelerometer.XScaleValue = xScaleValue;

			if (deviceName != null)
			{
				DeviceName = deviceName;
				_logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Device name:  = { deviceName}");
			}

			IDictionary<EDeviceType, int> scaleValues;
			parameters.TryGetValue(Constants.NavigationParamsKeys.SCALE_VALUES, out scaleValues);
			if (scaleValues != null && EEMorEMG != null && BioImpedance != null && Accelerometer != null)
			{
				EEMorEMG.PickerValue = scaleValues[EDeviceType.EEGorEMG];

				if (isAccelerometerPresent && isBioImpedancePresent)
				{
					BioImpedance.PickerValue = scaleValues[EDeviceType.BioImpedance];
					Accelerometer.PickerValue = scaleValues[EDeviceType.Accelerometer];
				}
				else if (isBioImpedancePresent)
				{
					BioImpedance.PickerValue = scaleValues[EDeviceType.BioImpedance];
					Accelerometer.PickerValue = Constants.Charts.ChartAccYAxisDefaultValue;
				}
				else if (isAccelerometerPresent)
				{
					BioImpedance.PickerValue = Constants.Charts.ChartYAxisDefaultValue;
					Accelerometer.PickerValue = scaleValues[EDeviceType.Accelerometer];
				}

                _logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: EEMorEMG.PickerValue:  = { EEMorEMG.PickerValue }");
				_logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: BioImpedance.PickerValue:  = { BioImpedance.PickerValue }");
				_logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Accelerometer.PickerValue: = { Accelerometer.PickerValue } ");
			}
			List<List<int>> filteredIds;
			parameters.TryGetValue(Constants.NavigationParamsKeys.FILTERED_IDS, out filteredIds);
			if (filteredIds != null && EEMorEMG != null && BioImpedance != null && Accelerometer != null)
			{
				EEMorEMG.FilteredList = filteredIds[0];
			    if (isAccelerometerPresent && isBioImpedancePresent)
			    {
			        BioImpedance.FilteredList = filteredIds[1];
			        Accelerometer.FilteredList = filteredIds[2];
                }
			    else if (isBioImpedancePresent)
			    {
			        BioImpedance.FilteredList = filteredIds[1];
                }
			    else if (isAccelerometerPresent) 
			    {
			        Accelerometer.FilteredList = filteredIds[1];
                }
			}

			IList<SensorConnectionModel> sensorConnectionList = null;
			parameters.TryGetValue(Constants.NavigationParamsKeys.SENSOR_CONNECTION_LIST, out sensorConnectionList);
			if (sensorConnectionList != null)
			{
				SensorConnectionList = sensorConnectionList;
			}
			//UpdateSensorsConnectionState();

			switch (deviceType)
			{
				case EDeviceType.EEGorEMG:
					StripTabIndex = 0;
					EEMorEMG.CurrentChart = chartId;
					break;
				case EDeviceType.BioImpedance:
					StripTabIndex = 1;
					BioImpedance.CurrentChart = chartId;
					break;
				case EDeviceType.Accelerometer:
					StripTabIndex = 2;
					Accelerometer.CurrentChart = chartId;
					break;
			}
        }

        /// <inheritdoc />
        public override void OnNavigatedFrom(NavigationParameters aParameters)
        {
            base.OnNavigatedFrom(aParameters);

            // ensure any popup is closed, when we leave this page
            IsSpsChartVisible = false;
        }

        #endregion

		#region -- Private helpers --

		//private void UpdateSensorsConnectionState()
		//{
		//	var actualState = ESensorConnectionState.None;
		//	var converter = new SignalValueToEConnectionStateConverter();
		//	foreach (var sensor in SensorConnectionList)
		//	{
		//		if (sensor.IsActive)
		//		{
		//			var currentState = (ESensorConnectionState)converter.Convert(sensor.SignalValue, typeof(ESensorConnectionState), null, null);
		//			actualState = actualState >= currentState ? actualState : currentState;
		//		}
		//		else
		//		{
		//			continue;
		//		}
		//	}

		//	SensorsConnectionState = actualState;
		//}

		private Task OnConnectionStateCommand()
		{
			NavigationParameters navigationParameters = new NavigationParameters();
			navigationParameters.Add("SensorConnectionList", SensorConnectionList);
			return _navigationService.NavigateAsync(nameof(ImpedanceView), navigationParameters);
		}

		private async Task OnBandWidthCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: tap on SPS.  SPS chart visible = {IsSpsChartVisible}");

			IsSpsChartVisible = !IsSpsChartVisible;
		}

		private Task OnOpenFolderCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped on open folder");

			return _navigationService.NavigateAsync(nameof(FilesBrowserView));
		}

	    private async void OnAdditionalSettingsCommand()
	    {
	        _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigate to advanced setup");

	        CanOpenAnotherScreen = false;
	        await _navigationService.NavigateAsync(nameof(SetupView));
        }

        /// <summary>
        /// Opens Technical Settings screen.
        /// </summary>
	    private async void OnTechnicalSettingsCommand()
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigate to advanced setup");

            CanOpenAnotherScreen = false;
	        await _navigationService.NavigateAsync(nameof(TechnicalSettingsView));
	    }

        /// <summary>
        /// Closes Sps Details.
        /// </summary>
        private void OnCloseSpsDetailsCommand()
	    {
	        IsSpsChartVisible = false;
	    }

        private void UpdateSensorsConnectionState(IReadOnlyDictionary<int, double> aImpedanceData)
        {
            var converter = new SignalValueToEConnectionStateConverter();

            SensorsConnectionState = SensorConnectionList
                .Where(aModel => aModel.IsActive)
                .Where(aModel => aImpedanceData.ContainsKey(aModel.SensorId))
                .Select(aModel =>
                {
                    var impedanceValue = aImpedanceData[aModel.SensorId];

                    var result = converter.Convert(impedanceValue,
                        typeof(ESensorConnectionState), null, null);
                    return (ESensorConnectionState) result;
                })
                .DefaultIfEmpty(ESensorConnectionState.None)
                .Max();
        }

		private void Subscribe()
		{
		    _temperatureService.SubscribeChangesAsync(_blueToothService.CurrentDevice);

            Temperature = _temperatureService.TemperatureActualValue;
            BatteryLevel = _temperatureService.BatteryActualValue;
		    IsEraseModeActive = _temperatureService.IsEraseModeActive;

            _blueToothService.OnChangedDeviceConnection += OnDeviceConnectionChanged;
            _temperatureService.OnBatteryLevelChanged += OnBatteryLevelChanged;
		    _temperatureService.OnTemperatureChanged += OnTemperatureChanged;
		    _temperatureService.OnEraseModeChanged += OnEraseModeChanged;
			_chartsService.ImpedanceDataLoaded += OnImpedanceDataLoaded;
		}

		private void Unsubscribe()
		{
			_blueToothService.OnChangedDeviceConnection -= OnDeviceConnectionChanged;
            _temperatureService.OnBatteryLevelChanged -= OnBatteryLevelChanged;
		    _temperatureService.OnTemperatureChanged -= OnTemperatureChanged;
		    _temperatureService.OnEraseModeChanged -= OnEraseModeChanged;
			_chartsService.ImpedanceDataLoaded -= OnImpedanceDataLoaded;
		}

        /// <summary>
        /// Handles update of impedance data.
        /// </summary>
        private void OnImpedanceDataLoaded(object aSender, ImpedanceDataEventArgs aArgs)
        {
            UpdateSensorsConnectionState(aArgs.FullData);
        }

        private void OnBatteryLevelChanged(object sender, int batteryLevel)
		{
            Device.BeginInvokeOnMainThread(() =>
            {
                BatteryLevel = batteryLevel;
                _logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Device Buttery level = {batteryLevel}%");
            });
		}

	    /// <summary>
	    /// Handles temperature changes.
	    /// </summary>
	    /// <param name="aSender"> The event sender. </param>
	    /// <param name="aTemperature"> The temperature value. </param>
	    private void OnTemperatureChanged(object aSender, int aTemperature)
	    {
            Device.BeginInvokeOnMainThread(() =>
            {
                Temperature = aTemperature;
                _logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Temperature Changed. level = {aTemperature}");
            });
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
			if (isConnected)
			{
                _temperatureService.SubscribeChangesAsync(_blueToothService.CurrentDevice);
			}
			else
			{
                _temperatureService.UnsubscribeChangeAsync();
			}

			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Device Connection State isConnected = {isConnected}");

			IsConnectionLost = !isConnected;
		}

		#endregion
	}
}
