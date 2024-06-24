using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using biopot.Helpers;
using biopot.Services.Charts;
using biopot.Views;
using Prism.Navigation;
using Acr.UserDialogs;
using Xamarin.Forms;
using biopot.Resources.Strings;
using biopot.Models;
using biopot.Enums;
using biopot.Converters;
using biopot.Services;
using SharedCore.Services;
using SharedCore.Services.Charts;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using biopot.Controls;
using Prism.Commands;
using SharedCore.Enums;
using SharedCore.Services.TemperatureDataService;
using SharedCore.Services.RateInfoService;

namespace biopot.ViewModels
{
	public class ChartsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
		private readonly IChartManagerService _chartsManagerService;
		private readonly IUserDialogs _userDialogs;
		private readonly ISaveDataService _saveDataService;
		private readonly IAppSettingsManagerService _appSettingsManagerService;
		private readonly IBlueToothService _blueToothService;
	    private readonly ITemperatureDataService _temperatureService;
		private readonly IBiobotDataForChartsService _biobotDataForChartsService;
	    private readonly IBiopotInfoChartsService _biopotInfoChartsService;
		private readonly IChartService _chartService;
		private readonly ILogService _logService;
		private readonly ISoundService _soundService;
        private readonly ISamplesRateInfoService _samplesRateInfoService;

        private CancellationTokenSource _syncModeCompleteDialogCancellationTokenSource;
        private System.Timers.Timer _connectionTimer;
		private bool _isDeviceTapped;
		private bool _isDisconnectedBioPod;

		private static List<string> chartColros = new List<string>(){
			"#FF212121",
			"#FFFB9600",
			"#FF4680F2",
			"#FF741573"
		};
		private int _currentColor = 0;

		public ChartsViewModel(IUserDialogs userDialogs,
							   IChartService chartService,
							   IBiobotDataForChartsService biobotDataForChartsService,
							   INavigationService navigationService,
							   IChartManagerService chartsService,
							   ISaveDataService saveDataService,
							   IAppSettingsManagerService appSettingsManagerService,
                               ITemperatureDataService temperatureService,
							   IBlueToothService blueToothService,
							   ILogService logService,
                               ISoundService soundService,
                               IBiopotInfoChartsService biopotInfoChartsService,
                               ISamplesRateInfoService samplesRateInfoService)
		{
			_navigationService = navigationService;
			_chartsManagerService = chartsService;
			_userDialogs = userDialogs;
			_saveDataService = saveDataService;
			_biobotDataForChartsService = biobotDataForChartsService;
			_chartService = chartService;
			_appSettingsManagerService = appSettingsManagerService;
		    _temperatureService = temperatureService;
			_blueToothService = blueToothService;
			_logService = logService;
		    _soundService = soundService;
		    _biopotInfoChartsService = biopotInfoChartsService;
            _samplesRateInfoService = samplesRateInfoService;

            RebuildUiComponentsAsync();

			ChartSps = new ChartViewModel(_chartsManagerService, EDeviceType.SPSValue, 0, _navigationService)
			{
				ViewportHalfY = Constants.Charts.ChartYAxisDefaultValue,
			};

			Subscribe();

            
            if (Constants.ForceRecordingFile == true) {
                //IsRecording = Constants.ForceRecordingFile;
                OnStartRecord();
            }
                
            ResetChartScaleCommand = new DelegateCommand<ChartsGroupedByDevice>(ResetScaleToDefaultValues);
        }

		#region -- Overrides --

		public override void Destroy()
		{
			base.Destroy();

			Unsubscribe();
		}
        private int debugTest=0;

        /// <inheritdoc />
        public override async void OnNavigatingTo(NavigationParameters parameters)
		{
            // Handle return back.
		    var isReturnedBack = parameters.GetValue<bool>(Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN);
		    if (isReturnedBack)
		    {
		        CanOpenMenu = true;
		    }

            IDictionary<EDeviceType, int> scaleValues;
			parameters.TryGetValue(Constants.NavigationParamsKeys.SCALE_VALUES, out scaleValues);
			if (scaleValues != null)
			{
				foreach (var device in _Devices)
				{
					if (scaleValues.TryGetValue(device.DeviceType, out var scaleValue))
					{
						device.PickerValue = scaleValue;
					}
				}
			}

		    parameters.TryGetValue(Constants.NavigationParamsKeys.X_SCALE_VALUE, out int xScaleValue);
		    if (xScaleValue != 0)
		    {
		        XScaleValue = xScaleValue;
            }

            parameters.TryGetValue(Constants.NavigationParamsKeys.DEVICE_NAME, out string deviceName);
			if (deviceName != null)
			{
				DeviceName = deviceName;
			}

			parameters.TryGetValue(Constants.NavigationParamsKeys.NAV_FROM_FILE_BROWSER, out bool isFromFileBrowser);
			if (isFromFileBrowser)
			{
				if (IsRecording)
				{
					OnStopRecord();
				}
			}

            if (parameters.TryGetValue(Constants.NavigationParamsKeys.NAV_FROM_IMPEDANCE, out bool isFromImpedance)
                && isFromImpedance)
            {
                // restart the charts data instead of impedance
                await _chartsManagerService.StartAsync(CaptureMode.CaptureEegEmg);
            }
		}

        /// <inheritdoc />
        public override void OnNavigatedFrom(NavigationParameters aParameters)
        {
            base.OnNavigatedFrom(aParameters);

            // ensure any popup is closed, when we leave this page
            IsSpsChartVisible = false;
            IsMenuOpened = false;
        }

        #endregion

        #region -- Public properties --

	    private BiopotGenericInfo _deviceInfo = new BiopotGenericInfo();
	    public BiopotGenericInfo DeviceInfo
	    {
	        get => _deviceInfo;
	        set => SetProperty(ref _deviceInfo, value);
	    }

        private bool _isSpsChartVisible = false;
		public bool IsSpsChartVisible
		{
			get { return _isSpsChartVisible; }
            set
            {
                if (SetProperty(ref _isSpsChartVisible, value))
                {
                    if (value)
                    {
                        // close any other opened popups on this page
                        IsMenuOpened = false;
                    }
                }
            }
		}

		private ChartViewModel _chartSps;
		public ChartViewModel ChartSps
		{
			get { return _chartSps; }
			set { SetProperty(ref _chartSps, value); }
		}

		private bool _isRecording;
		public bool IsRecording
		{
			get { return _isRecording; }
			set { SetProperty(ref _isRecording, value); }
		}

        private bool _isEraseModeActive;
        public bool IsEraseModeActive
        {
            get => _isEraseModeActive;
            set => SetProperty(ref _isEraseModeActive, value);
        }

        private bool _isSyncModeActive;
        public bool IsSyncModeActive
        {
            get => _isSyncModeActive;
            set
            {
                if (SetProperty(ref _isSyncModeActive, value))
                {
                    if (_isSyncModeActive)
                    {
                        //FIXME customize dialog and show title + message
                        _userDialogs.ShowLoading(Strings.SyncModeActiveTitle, MaskType.Black);
                    }
                }
            }
        }

        private bool _isBluetoothOn = true;
		public bool IsBluetoothOn
		{
			get { return _isBluetoothOn; }
			set { SetProperty(ref _isBluetoothOn, value); }
		}

		private ESensorConnectionState _SensorsConnectionState;
		public ESensorConnectionState SensorsConnectionState
		{
			get { return _SensorsConnectionState; }
			set { SetProperty(ref _SensorsConnectionState, value); }
		}

		private bool _isConnected = true;
		public bool IsConnected
		{
			get { return _isConnected; }
			set { SetProperty(ref _isConnected, value); }
		}

		private IList<SensorConnectionModel> _SensorConnectionList;
		public IList<SensorConnectionModel> SensorConnectionList
		{
			get { return _SensorConnectionList; }
			set { SetProperty(ref _SensorConnectionList, value); }
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
		        SetProperty(ref _batteryLevel, value);
		        RaisePropertyChanged(nameof(BatteryImage));
		        RaisePropertyChanged(nameof(BatteryImageColor));
		    }
		}

        private bool _isMenuOpened;
        public bool IsMenuOpened
        {
            get => _isMenuOpened;
            set
            {
                if (SetProperty(ref _isMenuOpened, value))
                {
                    if (value)
                    {
                        // close any other opened popups on this page
                        IsSpsChartVisible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Flag that controls openning menu, used for fixing conflict between click and long click on gear icon.
        /// </summary>
        private bool _canOpenMenu = true;
        public bool CanOpenMenu
        {
            get => _canOpenMenu;
            private set => SetProperty(ref _canOpenMenu, value);
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

                // FIXME animate view
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

        private bool _isConnectionLost;
		public bool IsConnectionLost
		{
			get => _isConnectionLost;
			set { SetProperty(ref _isConnectionLost, value); }
		}

        private int iXScaleValue = Constants.Charts.ChartXAxisDefaultValue;
        public int XScaleValue
        {
            get => iXScaleValue;
            set
            {
                if (SetProperty(ref iXScaleValue, value))
                {
                    UpdateXScaleForAllCharts();
        }
            }
        }

        private int iXScaleMaxValue = Constants.Charts.ChartXAxisMaxValue;
        public int XScaleMaxValue
        {
            get => iXScaleMaxValue;
            set => SetProperty(ref iXScaleMaxValue, value);
        }

        private int iXScaleMinValue = Constants.Charts.ChartXAxisMinValue;
        public int XScaleMinValue
        {
            get => iXScaleMinValue;
            set => SetProperty(ref iXScaleMinValue, value);
        }

        private ICommand _UpdateChartStateCommand;
		public ICommand UpdateChartStateCommand => _UpdateChartStateCommand ?? (_UpdateChartStateCommand = SingleExecutionCommand.FromFunc(OnUpdateChartStateCommand));

		private ICommand _ConnectionStateCommand;
		public ICommand ConnectionStateCommand => _ConnectionStateCommand ?? (_ConnectionStateCommand = SingleExecutionCommand.FromFunc(OnConnectionStateCommand));

		private ICommand _BandWidthCommand;
		public ICommand BandWidthCommand => _BandWidthCommand ?? (_BandWidthCommand = SingleExecutionCommand.FromFunc(OnBandWidthCommand));

		private ICommand _OpenFolderCommand;
		public ICommand OpenFolderCommand => _OpenFolderCommand ?? (_OpenFolderCommand = SingleExecutionCommand.FromFunc(OnOpenFolderCommand));

		private ICommand _additionalSettingsCommand;
		public ICommand AdditionalSettingsCommand => _additionalSettingsCommand ?? (_additionalSettingsCommand = SingleExecutionCommand.FromFunc(OnAdditionalSettingsCommand));

		private ICommand _UserSettingsCommand;
		public ICommand UserSettingsCommand => _UserSettingsCommand ?? (_UserSettingsCommand = SingleExecutionCommand.FromFunc(OnUserSettingsCommand));

        private ICommand _technicalSettingsCommand;
        public ICommand TechnicalSettingsCommand => _technicalSettingsCommand ?? (_technicalSettingsCommand = SingleExecutionCommand.FromFunc(OnTechnicalSettingsCommand));

        private ICommand _closeSpsDetailsCommand;
        public ICommand CloseSpsDetailsCommand => _closeSpsDetailsCommand ?? (_closeSpsDetailsCommand = new Command(OnCloseSpsDetailsCommand));
        /* H.H. no working !!!!!!!
        private ICommand _BackCommand;
        public ICommand BackCommand => _BackCommand ?? (_BackCommand = SingleExecutionCommand.FromFunc(OnBackCommandAsync));
        private async Task OnBackCommandAsync()
        {
            await _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Back command from Setup View");
            debugTest++;
        }
        */
        /// <summary>
        /// Gets a command to reset chart scales, pressed for particular device type.
        /// </summary>
        public DelegateCommand<ChartsGroupedByDevice> ResetChartScaleCommand { get; }

		private ObservableCollection<ChartsGroupedByDevice> _Devices = new ObservableCollection<ChartsGroupedByDevice>();
		public ObservableCollection<ChartsGroupedByDevice> Devices
		{
			get { return _Devices; }
			set { SetProperty(ref _Devices, value); }
		}

		private ObservableCollection<CheckboxesGroupedByDevice> _CheckboxViewmodels = new ObservableCollection<CheckboxesGroupedByDevice>();
		public ObservableCollection<CheckboxesGroupedByDevice> CheckboxViewmodels
		{
			get { return _CheckboxViewmodels; }
			set { SetProperty(ref _CheckboxViewmodels, value); }
		}

	    private ICommand _ItemTappedCommand;
		public ICommand ItemTappedCommand => _ItemTappedCommand ?? (_ItemTappedCommand = SingleExecutionCommand.FromFunc(OnItemTappedCommand));

		private ICommand _startRecordCommand;
		public ICommand StartRecordCommand => _startRecordCommand ?? (_startRecordCommand = new Command(OnStartRecord));

		private ICommand _stopRecordCommand;
		public ICommand StopRecordCommand => _stopRecordCommand ?? (_stopRecordCommand = new Command(OnStopRecord));

		public ICommand DeviceNameTappedCommand => SingleExecutionCommand.FromFunc(DeviceNameTappedCommandExecute);

        #endregion

        #region -- Overrides --

        public override void OnAppearing()
		{
			base.OnAppearing();
			IsSpsChartVisible = false;
		}

		#endregion

		#region -- Private helpers --

		private void RebuildUiComponentsAsync()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigating to Multiple charts page");

		    Device.BeginInvokeOnMainThread(RebuildUiComponents);
		}

        private void RebuildUiComponents()
        {
            CheckboxViewmodels = new ObservableCollection<CheckboxesGroupedByDevice>();
            Devices = new ObservableCollection<ChartsGroupedByDevice>();

            var montageChannels = _chartsManagerService.RegisteredMontageChannels;
            var deviceTypes = _chartsManagerService.GetDeviceTypes();
            foreach (var deviceType in deviceTypes)
            {
                this._CheckboxViewmodels.Add(new CheckboxesGroupedByDevice()
                {
                    DeviceType = deviceType,
                });
                if (deviceType == EDeviceType.Accelerometer)
                {
                    this._Devices.Add(new ChartsGroupedByDevice()
                    {
                        DeviceType = deviceType,
                        MaxValue = Constants.Charts.ChartAccYAxisMaxValue,
                        MinValue = Constants.Charts.ChartAccYAxisMinValue,
                        PickerValue = Constants.Charts.ChartAccYAxisDefaultValue,
                    });
                }
                else
                {
                    this._Devices.Add(new ChartsGroupedByDevice()
                    {
                        DeviceType = deviceType,
                        MaxValue = Constants.Charts.ChartYAxisMaxValue,
                        MinValue = Constants.Charts.ChartYAxisMinValue,
                        PickerValue = Constants.Charts.ChartYAxisDefaultValue,
                    });
                }
            }

            foreach (var device in this._CheckboxViewmodels)
            {
                var ids = _chartsManagerService.GetIds(device.DeviceType);
                foreach (var id in ids)
                {
                    device.Add(new ChartCheckboxModel()
                    {
                        DeviceType = device.DeviceType,
                        Id = id,
                        IsChecked = true
                    });
                }
            }

            for (int i = 0; i < _CheckboxViewmodels.Count; i++)
            {
                var chartGroup = _Devices[i];
                chartGroup.Clear();
                var checkedChannels = _CheckboxViewmodels[i].Where(x => x.IsChecked).ToList();

                foreach (var checkedItem in checkedChannels)
                {
                    var viewModel = new ChartViewModel(_chartsManagerService, chartGroup.DeviceType,
                        checkedItem.Id, _navigationService, checkedChannels.Select(model => model.Id))
                    {
                        ChartColor = SkiaSharp.SKColor.Parse(NextColor()),
                    };

                    chartGroup.Add(viewModel);
                }

                ApplyMontageChannelIfExists(montageChannels, chartGroup);
            }

            LoadSensorsConnectionState();
            UpdateXScaleForAllCharts();
        }

        /// <summary>
        /// Resets scales for one or multiple charts to default values.
        /// </summary>
        /// <param name="aDeviceCharts">The device charts to trigger reset for.</param>
        private void ResetScaleToDefaultValues(ChartsGroupedByDevice aDeviceCharts)
        {
            // X-axis is reset for all devices at once
            XScaleValue = Constants.Charts.ChartXAxisDefaultValue;

            // Y-axis is reset for individual group
            aDeviceCharts.PickerValue = Constants.Charts.ChartYAxisDefaultValue;
        }

        /// <summary>
        /// Applies registered montage channels to view models.
        /// </summary>
        /// <param name="aMontageChannels">The registered montage channels.</param>
        /// <param name="aChartGroup">A chart group of particular type.</param>
        private void ApplyMontageChannelIfExists(IReadOnlyCollection<MontageChannelPair> aMontageChannels,
            ChartsGroupedByDevice aChartGroup)
        {
            if (aChartGroup.DeviceType != EDeviceType.EEGorEMG)
            {
                return;
            }

            foreach (var viewModel in aChartGroup)
            {
                var montageChannelId = aMontageChannels
                    .Where(pair => pair.ChannelId == viewModel.ChartId)
                    .Select(x => x.ReferenceChannelId)
                    .DefaultIfEmpty(ChartViewModel.MontageReferenceValue)
                    .First();

                viewModel.MontageChannel = montageChannelId;
            }
        }

        /// <summary>
        /// Updates X-scale of all charts, using current <see cref="XScaleValue"/>.
        /// </summary>
        private void UpdateXScaleForAllCharts()
        {
            foreach (var viewModel in Devices.SelectMany(x => x))
            {
                // actual baseline is 0, while the min value can be larger.
                viewModel.ViewportX = XScaleValue / (float) (XScaleMaxValue - 0);
            }
        }

		private void LoadSensorsConnectionState()
		{
			SensorConnectionList = new List<SensorConnectionModel>();
			var EEGEMGList = Devices.FirstOrDefault(d => d.DeviceType == EDeviceType.EEGorEMG).ToList();
			foreach (var item in EEGEMGList)
			{
				SensorConnectionList.Add(new SensorConnectionModel() { SensorId = item.ChartId, IsActive = true, SignalValue = 0 });
			}

			//TODO Data Mock
			//SensorConnectionList = new List<SensorConnectionModel>()
			//{
			//	new SensorConnectionModel(){ SensorId = 10, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 2, IsActive = true, SignalValue = 8000},
			//	new SensorConnectionModel(){ SensorId = 3, IsActive = true, SignalValue = 13000},
			//	new SensorConnectionModel(){ SensorId = 4, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 5, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 6, IsActive = true, SignalValue = 6000},
			//	new SensorConnectionModel(){ SensorId = 7, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 8, IsActive = true, SignalValue = 6000},
			//	new SensorConnectionModel(){ SensorId = 9, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 1, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 11, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 12, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 13, IsActive = true, SignalValue = 6000},
			//	new SensorConnectionModel(){ SensorId = 14, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 15, IsActive = true, SignalValue = 3000},
			//	new SensorConnectionModel(){ SensorId = 16, IsActive = true, SignalValue = 13000},
			//};

			SensorConnectionList = SensorConnectionList.OrderBy(s => s.SensorId).ToList();
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

		private void UpdateActiveSensors()
		{
			foreach (var item in SensorConnectionList)
			{
				item.IsActive = false;
			}
			var EEGEMGList = Devices.FirstOrDefault(d => d.DeviceType == EDeviceType.EEGorEMG).ToList();
			foreach (var item in EEGEMGList)
			{
				SensorConnectionList[item.ChartId - 1].IsActive = true;
			}
		}

        /// <summary>
        /// Closes Sps Details.
        /// </summary>
        private void OnCloseSpsDetailsCommand()
        {
            IsSpsChartVisible = false;
        }

        private async Task OnUserSettingsCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}:Tapped Settings command");

			_userDialogs.ShowLoading();
			NavigationParameters navigationParameters = new NavigationParameters();
			navigationParameters.Add("FromMenu", true);
			if (IsRecording)
			{
                //OnStopRecord();//H.H:Dont stop recording when entering settings 
            }
            await _navigationService.NavigateAsync(nameof(MainView), navigationParameters);
			_userDialogs.HideLoading();
		}

        private async Task OnConnectionStateCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}:Tapped Connection State command");

			_userDialogs.ShowLoading();
			await OnUpdateChartStateCommand();
			NavigationParameters navigationParameters = new NavigationParameters();
			navigationParameters.Add("SensorConnectionList", SensorConnectionList);
			await _navigationService.NavigateAsync(nameof(ImpedanceView), navigationParameters);
			_userDialogs.HideLoading();
		}

		private async Task OnBandWidthCommand()
		{
			IsSpsChartVisible = !IsSpsChartVisible;

			if (IsSpsChartVisible)
			{
				await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Open SPS chart");
			}
			else
			{
				await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Close SPS chart");
			}
		}

		private Task OnOpenFolderCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Open Folder command");

			if (IsRecording)
			{
				//OnStopRecord();//H.H:Dont stop recording when entering settings 
            }
			return _navigationService.NavigateAsync(nameof(FilesBrowserView));
		}

		private Task OnAdditionalSettingsCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigate to additional settings");

			if (IsRecording)
			{
				//OnStopRecord();//H.H:Dont stop recording when entering settings 
			}
			return _navigationService.NavigateAsync(nameof(SetupView));
		}


        private Task OnTechnicalSettingsCommand()
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigate to technical settings");

            CanOpenMenu = false;
            if (IsRecording)
            {
                //OnStopRecord();//H.H:Dont stop recording when entering settings 
            }
            return _navigationService.NavigateAsync(nameof(TechnicalSettingsView));
        }

        Stopwatch _processTime = new Stopwatch();
        

        private Task OnUpdateChartStateCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Update charts");

			_userDialogs.ShowLoading("Update charts...");
			_processTime.Reset();
			_processTime.Start();

		    var montageChannels = _chartsManagerService.RegisteredMontageChannels;
			for (int i = 0; i < this._CheckboxViewmodels.Count; i++)
			{
				var list = _CheckboxViewmodels[i].Where(x => x.IsChecked).ToList();

				for (int k = 0; k < this._Devices[i].Count; k++)
				{
					this._Devices[i][k].Unsubscribe();
				}

                var chartGroup = _Devices[i];
                chartGroup.Clear();

				foreach (var checkedItem in list)
				{

                    var chartViewModel = new ChartViewModel(_chartsManagerService, chartGroup.DeviceType, 
                        checkedItem.Id, _navigationService, list.Select(model => model.Id))
                    {
                        ChartColor = SkiaSharp.SKColor.Parse(NextColor()),
                    };
                    chartGroup.Add(chartViewModel);
				}

                ApplyMontageChannelIfExists(montageChannels, chartGroup);

				if (list.Count == 0)
				{
					_Devices[i].IsHeaderVisible = false;
					_Devices[i].HeaderHeight = 0;
				}
				else
				{
					_Devices[i].IsHeaderVisible = true;
					_Devices[i].HeaderHeight = 65;
				}

				//HACK for displaying graphs after closing Chanels menu 
				//TODO find a good solution
				var pickerVal = _Devices[i].PickerValue;
				_Devices[i].PickerValue = pickerVal;
			}
			_processTime.Stop();
			Debug.WriteLine($" FOR TIME: {_processTime.ElapsedMilliseconds}");

            UpdateActiveSensors();
            UpdateXScaleForAllCharts();
            //UpdateSensorsConnectionState();

            _userDialogs.HideLoading();

            return Task.FromResult<bool>(true);
		}

        private async Task OnItemTappedCommand(object obj)
		{
			ChartViewModel chartViewModel = (ChartViewModel)obj;
			if (chartViewModel == null)
				return;
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped On One Selected chart. ID = " + chartViewModel.ChartId);

			NavigationParameters navigationParameters = new NavigationParameters();
			navigationParameters.Add(Constants.NavigationParamsKeys.CHART_ID, chartViewModel.ChartId);

			IDictionary<EDeviceType, int> scaleValues = this._Devices
				.ToDictionary(aDevice => aDevice.DeviceType, aDevice => aDevice.PickerValue);

            navigationParameters.Add(Constants.NavigationParamsKeys.SCALE_VALUES, scaleValues);
            navigationParameters.Add(Constants.NavigationParamsKeys.X_SCALE_VALUE, XScaleValue);
			navigationParameters.Add(Constants.NavigationParamsKeys.DEVICE_TYPE, chartViewModel.DeviceType);
			var filteredIds = this.Devices.Select(charts => charts.Select(chart => chart.ChartId).ToList()).ToList();
			navigationParameters.Add(Constants.NavigationParamsKeys.FILTERED_IDS, filteredIds);
			navigationParameters.Add(Constants.NavigationParamsKeys.SENSOR_CONNECTION_LIST, SensorConnectionList);
			navigationParameters.Add(Constants.NavigationParamsKeys.DEVICE_NAME, DeviceInfo.DeviceName);
			navigationParameters.Add(Constants.NavigationParamsKeys.IS_ACCELEROMETER, DeviceInfo.IsAccelerometerPresent);
			navigationParameters.Add(Constants.NavigationParamsKeys.IS_BIO_IMPEDANCE, DeviceInfo.IsBioImpedancePresent);

			await _navigationService.NavigateAsync(nameof(DetailedChartView), navigationParameters);
		}


        private string NextColor()
		{
			if (_currentColor == chartColros.Count - 1)
			{
				_currentColor = 0;
			}
			else
			{
				_currentColor++;
			}
			return chartColros[_currentColor];
		}


        private void OnStartRecord()
		{
			if (!IsConnectionLost)
			{
				_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Start record");

				_saveDataService.OnError += OnSaveDataError;

				_saveDataService.StartRecord();
			}
            IsRecording = true;
        }

		private void OnStopRecord()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Stop record");

			_saveDataService.OnError -= OnSaveDataError;

			_saveDataService.StopRecord();

			IsRecording = false;
		}

		private async void OnSaveDataError(int aErrorId)
		{
			IsRecording = false;

		    await _userDialogs.AlertAsync(string.Format(Strings.ErrorDataSaveMessage, aErrorId), Strings.ErrorDataSaveTitle, Strings.Ok);

            await _logService.CreateLogDataAsync($"{Constants.Logs.ERRORS}: Save data error. Error ID = {aErrorId}");
		}

		private async Task DeviceNameTappedCommandExecute()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Start navigation from Multicharts View to Previous screen");

			_isDeviceTapped = true;

			NavigationParameters navigationParameters = new NavigationParameters();
			navigationParameters.Add("FromChartsView", true);
			navigationParameters.Add(Constants.NavigationParamsKeys.DISCONNECTED_DEVICE, _isDisconnectedBioPod);
			navigationParameters.Add(Constants.NavigationParamsKeys.DISCONNECTED_BLUETOOTH, IsBluetoothOn);

			if (IsRecording)
			{
				OnStopRecord();
			}
			await _navigationService.NavigateAsync('/' + nameof(NavigationPage) + '/' + nameof(MainView), navigationParameters);
		}
        //bool firstStart = true;//H.H.

        private async void Subscribe()
		{
			SetTimer();

            _blueToothService.BluetoothEnabledChanged += BluetootheStateChanged;
			var result = await _chartsManagerService.StartAsync(CaptureMode.CaptureEegEmg);
			if (result.IsSuccess)
            {
                DeviceInfo = _chartService.DeviceInfo;
                _chartService.DeviceInfoUpdated += OnDeviceInfoUpdated;

                Temperature = _temperatureService.TemperatureActualValue;
                BatteryLevel = _temperatureService.BatteryActualValue;
                IsEraseModeActive = _temperatureService.IsEraseModeActive;
                IsSyncModeActive = _temperatureService.IsSyncModeActive;
                _temperatureService.OnBatteryLevelChanged += OnBatteryLevelChanged;
                _temperatureService.OnTemperatureChanged += OnTemperatureChanged;
                _temperatureService.OnEraseModeChanged += OnEraseModeChanged;
                _temperatureService.OnSyncModeChanged += OnSyncModeChanged;
                await _temperatureService.SubscribeChangesAsync(_blueToothService.CurrentDevice);

				_chartsManagerService.ImpedanceDataLoaded += OnImpedanceDataLoaded;
				_blueToothService.OnChangedDeviceConnection += OnDeviceConnectionChanged;

                // rebuild UI since the device info has been updated
                RebuildUiComponents();
                /* disabled by H.H. no more for redefine the char5 on app load 
                if (firstStart)
                {

                    Ch5Value = "00-07-FF-FF-01-07-01-F4-09-03-05-00-00";
                    await _samplesRateInfoService.SetDataAsync(Ch5Value);
                    await Task.Delay(5);

                    Ch5Value = "00-07-FF-FF-00-07-01-F4-09-03-05-00-00";
                    await _samplesRateInfoService.SetDataAsync(Ch5Value);
                    await Task.Delay(5);
                    firstStart = false;
                }*/
                Constants.ForceReconnection = true;

            }
			else
			{
				_isDisconnectedBioPod = true;

				SetTimerState();

				if (IsBluetoothOn && !_isDeviceTapped)
				{
				    await _userDialogs.AlertAsync(string.Format(Strings.ErrorSignalRetrieveMessage, result.Message),
				        Strings.ErrorSignalRetrieveTitle, Strings.Ok);
                }

				IsConnectionLost = true;
				IsConnected = false;
			}

		}

        private string _ch5Value = "00";
        public string Ch5Value
        {
            get => _ch5Value;
            set => SetProperty(ref _ch5Value, value);
        }


        private async Task OnCh5ValueReadCommandAsync()
        {
            var res = await _samplesRateInfoService.GetInfoAsync();
            if (res.IsSuccess)
            {
                Ch5Value = res.Result;
            }
            else
            {
               // await ShowErrorDialog(res.Message);
            }

           // await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Ch5Value Read Cmmand");
        }

        /// <summary>
        /// Handles update of impedance data.
        /// </summary>
        private void OnImpedanceDataLoaded(object aSender, ImpedanceDataEventArgs aArgs)
        {
            UpdateSensorsConnectionState(aArgs.FullData);
        }

		private async void Unsubscribe()
		{
			UsubscribeTimer();

		    _chartService.DeviceInfoUpdated -= OnDeviceInfoUpdated;
			_chartsManagerService.ImpedanceDataLoaded -= OnImpedanceDataLoaded;
			_blueToothService.OnChangedDeviceConnection -= OnDeviceConnectionChanged;
            _temperatureService.OnBatteryLevelChanged -= OnBatteryLevelChanged;
		    _temperatureService.OnTemperatureChanged -= OnTemperatureChanged;
		    _temperatureService.OnEraseModeChanged -= OnEraseModeChanged;
		    _temperatureService.OnSyncModeChanged -= OnSyncModeChanged;
            _blueToothService.BluetoothEnabledChanged -= BluetootheStateChanged;

            await _temperatureService.UnsubscribeChangeAsync();
            await _chartsManagerService.StopAsync();
        }

        /// <summary>
        /// Handles the device info has been updated.
        /// </summary>
        private void OnDeviceInfoUpdated(object aSender, BiopotGenericInfo aInfo)
        {
            DeviceInfo = aInfo;

            // update the UI with new data and state
            RebuildUiComponentsAsync();
        }


        private void OnBatteryLevelChanged(object sender, int batteryLevel)
		{
		    Device.BeginInvokeOnMainThread(() =>
		    {
		        BatteryLevel = batteryLevel;
		        _logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Baterry Level Changed. level = {batteryLevel}%");
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
	            _logService.CreateLogDataAsync(
	                $"{Constants.Logs.DEVICE_INFO}: Temperature Changed. level = {aTemperature}");
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

        /// <summary>
        /// Handles changes of sync mode.
        /// </summary>
        /// <param name="aSender"> The event sender. </param>
        /// <param name="aIsSyncModeActive"> True - sync mode is active, otherwise - false. </param>
        private void OnSyncModeChanged(object aSender, bool aIsSyncModeActive)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                IsSyncModeActive = aIsSyncModeActive;
                if (!IsSyncModeActive)
                {
                    SetSyncModeFinishedSuccessfully();
                }

                _logService.CreateLogDataAsync($"{Constants.Logs.DEVICE_INFO}: Sync Mode Changed. Is active = {aIsSyncModeActive}");
            });
        }

        private void OnDeviceConnectionChanged(object sender, bool isConnected)
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Device Connection was changed. Is connected = {isConnected}");

			IsConnected = isConnected;

			if (isConnected)
			{
				_isDisconnectedBioPod = false;

                // restore modes after reconnect
			    IsSyncModeActive = _temperatureService.IsSyncModeActive;
			    IsEraseModeActive = _temperatureService.IsEraseModeActive;

                _temperatureService.SubscribeChangesAsync(_blueToothService.CurrentDevice);

				SetTimerState();
			}
			else
			{
				_isDisconnectedBioPod = true;

				SetTimerState();

				if (IsBluetoothOn && !_isDeviceTapped)
				{
				    if (IsSyncModeActive)
				    {
				        SetSyncModeInterrupted();
				    }

                    // remove 'Erase mode' notification when connection is lost
				    if (IsEraseModeActive)
				    {
				        IsEraseModeActive = false;
				    }

                    //disabled by H.H. removed message in disconnectin
                    //_userDialogs.AlertAsync(string.Format(Strings.ErrorConnectionLostMessage, (int) ConnectionLostErrors.ConnectionLost), title: Strings.ErrorConnectionLostTitle, okText: Strings.Ok);
                    _userDialogs.ShowLoading("Reconnecting...", MaskType.Black);
                    Constants.ForceReconnection = true;
                    Constants.ForceRecordingFile = IsRecording;

                }

                _temperatureService.UnsubscribeChangeAsync();
			}

			IsConnectionLost = !isConnected;
		}

        /// <summary>
        /// Sets that sync mode is finished successfully.
        /// </summary>
        private void SetSyncModeFinishedSuccessfully()
        {
            IsSyncModeActive = false;

            _userDialogs.HideLoading();
            _soundService.Play3Beeps();

            // dismiss previously shown dialog if any
            _syncModeCompleteDialogCancellationTokenSource?.Cancel();
            _syncModeCompleteDialogCancellationTokenSource = new CancellationTokenSource();

            // FIXME customize the dialog.
            _userDialogs.AlertAsync(Strings.SyncModeFinishedMessage, Strings.SyncModeFinishedTitle, Strings.Ok, _syncModeCompleteDialogCancellationTokenSource.Token);
        }

        /// <summary>
        /// Sets that sync mode is interrupted, e.g. connection lost.
        /// </summary>
        private void SetSyncModeInterrupted()
        {
            IsSyncModeActive = false;

            _userDialogs.HideLoading();
        }

        private void SetTimer()
		{
			_connectionTimer = new System.Timers.Timer(Constants.DISCONNECTION_TIMER_TIME);

			_connectionTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
			_connectionTimer.Interval = Constants.DISCONNECTION_TIMER_TIME;
		}

		private void UsubscribeTimer()
		{
			_connectionTimer.Elapsed -= new ElapsedEventHandler(OnTimedEvent);
		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			if (IsConnectionLost == true || !IsBluetoothOn)
			{
				Device.BeginInvokeOnMainThread(NavigateFromPageAction);
			}
		}

		private async void BluetootheStateChanged(object sender, bool aBluetoothIsOn)
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Bluetooth Connection was changed. Is BT on? = {aBluetoothIsOn}");

			if (!aBluetoothIsOn)
			{
				SetTimerState();

				if (!IsBluetoothOn)
				{
					await DeviceNameTappedCommandExecute();
				}

				IsBluetoothOn = false;
				IsConnected = false;
			}
			else
			{
				IsBluetoothOn = true;

				SetTimerState();
			}
		}

		private async void NavigateFromPageAction()
		{
			SetTimerState();

			await DeviceNameTappedCommandExecute();
		}

		private void SetTimerState()
		{
			if (_connectionTimer.Enabled)
			{
				_connectionTimer.Stop();
			}
			else
			{
				_connectionTimer.Start();
			}
		}

		#endregion
	}
}
