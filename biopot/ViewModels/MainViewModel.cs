using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Navigation;
using Xamarin.Forms;
using biopot.Enums;
using biopot.Views;
using biopot.Services;
using System.Collections.Generic;
using Acr.UserDialogs;
using biopot.Helpers;
using System.Threading.Tasks;
using biopot.Resources.Strings;
using SharedCore.Services;
using SharedCore.Models;
using System.Linq;
using biopot.Models;
using Prism.Commands;
using Prism.Events;

namespace biopot.ViewModels
{
	public class MainViewModel : BaseViewModel
	{
        private readonly IEventAggregator _eventAggregator;
        private readonly IPermissionsRequester _permissionsRequester;
        private readonly INavigationService _navigationService;
		private readonly IBlueToothService _blueToothService;
		private readonly IUserDialogs _userDialogs;
		private readonly IAppSettingsManagerService _appSettings;
		private readonly ILogService _logService;
        private readonly ISaveDataService _saveDataService;

        private DeviceModel _currentDevice;
		private string _lastConnectedDeviceId;
		private bool _IsFromChartsView = false;
		private bool _wasDisconnectedDeviceOnChartsView;

        public MainViewModel(IEventAggregator eventAggregator,
            IPermissionsRequester permissionsRequester,
            ILogService logService,
            IAppSettingsManagerService appSettings,
            IUserDialogs userDialogs,
            IBlueToothService blueToothService,
            INavigationService navigationService,
            ISaveDataService saveDataService)
		{
            _eventAggregator = eventAggregator;
            _permissionsRequester = permissionsRequester;
            _navigationService = navigationService;
			_blueToothService = blueToothService;
			_userDialogs = userDialogs;
			_appSettings = appSettings;
			_logService = logService;
            _saveDataService = saveDataService;

            DiscoveredDeviceTappedCommand = new DelegateCommand<DeviceConnectionViewModel>(OnDiscoveredDeviceTappedCommand);
            StartCommand = new DelegateCommand(async () => await OnStartCommand())
                .ObservesProperty(() => IsStartAllowed);

            LoadDataAndSubscribe();
		}

	    /// <inheritdoc/>
	    public override void OnAppearing()
	    {
	        base.OnAppearing();
	        PatientDetailsViewModel.OnAppearing();
	    }

	    /// <inheritdoc/>
	    public override void OnDisappearing()
	    {
	        base.OnDisappearing();
	        PatientDetailsViewModel.OnDisappearing();
	    }

        #region -- Nested ViewModels --

        private IList<string> _targetsList = new List<string>() { Strings.InternalMemory, Strings.InternalSDCard, Strings.USBCable, Strings.WirelessStick };
		public IList<string> TargetsList
		{
			get { return _targetsList; }
			set { SetProperty(ref _targetsList, value); }
		}

		private PatientDetailsViewModel _PatientDetailsViewModel;
		public PatientDetailsViewModel PatientDetailsViewModel
		{
			get { return _PatientDetailsViewModel; }
			set { _PatientDetailsViewModel = value; }
		}

		private SessionViewModel _SessionViewModel;
		public SessionViewModel SessionViewModel
		{
			get { return _SessionViewModel; }
			set { _SessionViewModel = value; }
		}

		private UserDetailsViewModel _UserDetailsViewModel;
		public UserDetailsViewModel UserDetailsViewModel
		{
			get { return _UserDetailsViewModel; }
			set { _UserDetailsViewModel = value; }
		}

		private OtherDetailsViewModel _OtherDetailsViewModel;
		public OtherDetailsViewModel OtherDetailsViewModel
		{
			get { return _OtherDetailsViewModel; }
			set { _OtherDetailsViewModel = value; }
		}

		#endregion

		#region -- Public properties --

		private bool _isShowLoader;
		public bool IsShowLoader
		{
            get => _isShowLoader;
            private set
            {
                if (SetProperty(ref _isShowLoader, value))
                {
                    UpdateCommandsAvailability();
		}
            }
        }

		private bool _isShowWorningAlert;
		public bool IsShowWorningAlert
		{
			get { return _isShowWorningAlert; }
			set { SetProperty(ref _isShowWorningAlert, value); }
		}

		private bool _IsFromMenu;
		public bool IsFromMenu
		{
			get { return _IsFromMenu; }
		    set
		    {
		        SetProperty(ref _IsFromMenu, value);
		        if (PatientDetailsViewModel != null)
		        {
		            PatientDetailsViewModel.IsBarcodeScanned = IsFromMenu;
                }
		    }
		}

		private bool iIsStartAllowed;
        /// <summary>
        /// Gets a flag indicating if the 'start' command is allowed or not.
        /// </summary>
		public bool IsStartAllowed
		{
			get => iIsStartAllowed;
            private set => SetProperty(ref iIsStartAllowed, value);
		}

        // FIXME use this flag to enable/disable 'Next' button for other steps: Session and User details
	    private bool _isNextAllowed;
	    public bool IsNextAllowed
	    {
	        get => _isNextAllowed;
	        set => SetProperty(ref _isNextAllowed, value);
	    }

        private EWelcomeSteps _CurrentStep = EWelcomeSteps.First;
		public EWelcomeSteps CurrentStep
		{
			get { return _CurrentStep; }
			set { SetProperty(ref _CurrentStep, value); }
		}

		private IList<DeviceConnectionViewModel> _Devices = new ObservableCollection<DeviceConnectionViewModel>();
		public IList<DeviceConnectionViewModel> Devices
		{
			get { return _Devices; }
			set
			{
				if (_wasDisconnectedDeviceOnChartsView && value.Count > 0)
				{
					IsShowWorningAlert = true;
					_wasDisconnectedDeviceOnChartsView = false;
				}

				SetProperty(ref _Devices, value);
            }
        }

        private ICommand _NextCommand;
		public ICommand NextCommand => _NextCommand ?? (_NextCommand = SingleExecutionCommand.FromFunc(OnNextCommandExecute));

		private ICommand _BackCommand;
		public ICommand BackCommand => _BackCommand ?? (_BackCommand = SingleExecutionCommand.FromFunc(OnBackCommandExecute));

		private ICommand _RetryConnectionCommand;
		public ICommand RetryConnectionCommand => _RetryConnectionCommand ?? (_RetryConnectionCommand = SingleExecutionCommand.FromFunc(OnRetryConnectionCommand));

        /// <summary>
        /// Gets the command to execute, when a list item is tapped.
        /// </summary>
        public DelegateCommand<DeviceConnectionViewModel> DiscoveredDeviceTappedCommand { get; }

        /// <summary>
        /// Gets the command to execute, when the 'start' button is pressed.
        /// </summary>
        public ICommand StartCommand { get; }

		private ICommand _SkipAllCommand;
		public ICommand SkipAllCommand => _SkipAllCommand ?? (_SkipAllCommand = SingleExecutionCommand.FromFunc(OnSkipAllCommand));

		private ICommand _BackToMenuCommand;
		public ICommand BackToMenuCommand => _BackToMenuCommand ?? (_BackToMenuCommand = SingleExecutionCommand.FromFunc(OnBackToMenuCommand));

	    private ICommand _stepSelectedCommand;
	    public ICommand StepSelectedCommand => _stepSelectedCommand ?? (_stepSelectedCommand = new Command(OnStepSelectedCommand));

        /// <summary>
        /// Gets/sets current device instance, or <c>null</c>.
        /// </summary>
        public DeviceModel CurrentDevice
        {
            get => _currentDevice;
            private set
            {
                if (SetProperty(ref _currentDevice, value))
                {
                    UpdateCommandsAvailability();
                }
            }
        }

        #endregion

        #region -- Overrides --

        public override void Destroy()
		{
			UnSubscribe();
			base.Destroy();
		}

		public override async void OnNavigatingTo(NavigationParameters parameters)
		{
			await _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigating to patient detail page");

			if (parameters.TryGetValue("FromMenu", out bool isFromMenu))
			{
				IsFromMenu = isFromMenu;
				_wasDisconnectedDeviceOnChartsView = false;
			}

            //if we returned from Charts View (was disconnect device)
            if (parameters.TryGetValue(Constants.NavigationParamsKeys.DISCONNECTED_DEVICE, out _wasDisconnectedDeviceOnChartsView))
            {
                await DisconnectCurrentDeviceAsync();
            }
            //if we returned from Charts View (was turned off bluetooth)
            if (parameters.TryGetValue(Constants.NavigationParamsKeys.DISCONNECTED_BLUETOOTH, out bool wasConnectedBluetoothOnChartsView))
            {
                if (!wasConnectedBluetoothOnChartsView)
                {
                    await DisconnectCurrentDeviceAsync();
                }
            }

            if (parameters.TryGetValue("FromChartsView", out bool isFromChartsView))
			{
				if (isFromChartsView)
				{
					_IsFromChartsView = true;

					CurrentStep = EWelcomeSteps.Fourth;

					await DisconnectCurrentDeviceAsync();
                    TryConnectLastDeviceOrDiscoverDevices();
				}
			}

            if (parameters.TryGetValue("EndTask", out bool isEndTask))
            {
                if (isEndTask)
                {
                    CurrentStep = EWelcomeSteps.First;
                    _saveDataService.StopRecord();
                    if (parameters.TryGetValue("AudioName", out string audioName))
                        _saveDataService.SaveAudioDate(audioName);

                    if (parameters.TryGetValue("AudioData", out string audioData))
                        _saveDataService.SaveAudioDate(audioData);

                    PatientDetailsViewModel.PatientsInformation = new PatientsInformation();
                }
            }
        }

		#endregion

		#region -- Private helpers --

		private void Subscribe()
		{
		    PatientDetailsViewModel.IsValidationPassed += OnPatientDetailsValidationPassed;
		}

	    private void UnSubscribe()
	    {
	        PatientDetailsViewModel.IsValidationPassed -= OnPatientDetailsValidationPassed;
        }

		private Task OnBackToMenuCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Back to menu");

			if (string.IsNullOrWhiteSpace(SessionViewModel.FolderName))
				SessionViewModel.FolderName = Strings.DefaultFolderName;
			return Task.WhenAll(SaveVMDataAsync(), _navigationService.GoBackAsync());
		}

        /// <summary>
        /// Handles steps selection.
        /// </summary>
        /// <param name="aStep">The selected step. </param>
        private void OnStepSelectedCommand(object aStep)
        {
            if (CurrentStep <= EWelcomeSteps.Third)
            {
                CurrentStep = (EWelcomeSteps) aStep;
            }
        }

        /// <summary>
        /// Handles validation event if current steps if 1st.
        /// </summary>
        /// <param name="aSender"> The event sender. </param>
        /// <param name="aIsValidationPassed"> True - validation is passed, false - otherwise. </param>
	    private void OnPatientDetailsValidationPassed(object aSender, bool aIsValidationPassed)
	    {
	        if (CurrentStep == EWelcomeSteps.First)
	        {
	            IsNextAllowed = aIsValidationPassed;
	        }
	    }

        private async Task OnSkipAllCommand()
        {
            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Skip all command");

            CurrentStep = EWelcomeSteps.Fourth;

            TryConnectLastDeviceOrDiscoverDevices();
        }

        private async Task OnStartCommand()
        {
            if (CurrentDevice == null)
            {
                return;
            }

            await _logService.CreateLogDataAsync(
                $"{Constants.Logs.EVENT}: Start work with selected device '{CurrentDevice.DeviceName}'");

            if (await CheckBluetoothEnabledOrAlert())
            {
                IsShowLoader = true;

                try
                {
                    var isConnected = await ConnectToRemoteDeviceAsync(CurrentDevice);
                    if (isConnected)
                    {
                        await SaveVMDataAsync();
                        await RememberCurrentConnectedDeviceAndNavigateToCharts();
                    }
                    else
                    {
                        SetDisconnectedDevicesStatus();
                        UpdateSelectedDevices(Devices.FirstOrDefault());
                    }
                }
                finally
                {
                    IsShowLoader = false;
                }
            }
        }

        /// <summary>
        /// Loads data and then subscribes to the events.
        /// </summary>
	    private async void LoadDataAndSubscribe()
	    {
	        await LoadData();
            Subscribe();
	    }

		private async Task LoadData()
		{
			PatientDetailsViewModel = await _appSettings.GetObjectAsync<PatientDetailsViewModel>(Constants.StorageKeys.PATIENT_DETAIL) ?? new PatientDetailsViewModel();
			SessionViewModel = await _appSettings.GetObjectAsync<SessionViewModel>(Constants.StorageKeys.SESSION_DETAIL) ?? new SessionViewModel();
			UserDetailsViewModel = await _appSettings.GetObjectAsync<UserDetailsViewModel>(Constants.StorageKeys.USER_DETAIL) ?? new UserDetailsViewModel();
			OtherDetailsViewModel = await _appSettings.GetObjectAsync<OtherDetailsViewModel>(Constants.StorageKeys.OTHER_DETAIL) ?? new OtherDetailsViewModel();

            if (Constants.FirstTimeLoad == true)
            {
                _lastConnectedDeviceId = null;
                Constants.FirstTimeLoad = false; 
            }
            else
                _lastConnectedDeviceId = await _appSettings.GetObjectAsync<string>(Constants.StorageKeys.CONNECTED_DEVICE);
          
            PatientDetailsViewModel.InitDependencies(_permissionsRequester, _eventAggregator);
            PatientDetailsViewModel.PatientsInformation = new PatientsInformation();
        }

		private Task OnNextCommandExecute()
		{
            //_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: NEXT tapped");

            //if (CurrentStep == EWelcomeSteps.Third && IsFromMenu)
            //{
            //	OnBackToMenuCommand();
            //	return Task.CompletedTask;
            //}

            //if (CurrentStep <= EWelcomeSteps.Fourth)
            //{
            //	bool IsAllFieldDataValid = ValidationAllFieldData(CurrentStep);

            //	if (IsAllFieldDataValid)
            //	{
            //		int num = (int)CurrentStep + 1;
            //		CurrentStep = (EWelcomeSteps)(num);
            //	}
            //}

            //if (CurrentStep == EWelcomeSteps.Fourth)
            //{
            //             TryConnectLastDeviceOrDiscoverDevices();
            //}

            OnSkipAllCommand();
			return SaveVMDataAsync();
		}

		private bool ValidationAllFieldData(EWelcomeSteps currentStep)
		{
			bool isValid = true;
			switch (currentStep)
			{
				case EWelcomeSteps.First:
					_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Patient Details Page");
					if (string.IsNullOrWhiteSpace(PatientDetailsViewModel.PatientsInformation?.Name))
					{
						//isValid = false; TODO check nessarity for skip all
						//PatientDetailsViewModel.Message = Strings.PleaseEnterAllFields;
					}
					else
					{
						PatientDetailsViewModel.InfoMessage = "";
					}
					_logService.CreateLogDataAsync($"{Constants.Logs.DATA_ENTERED}: Patient id: " + PatientDetailsViewModel.PatientsInformation.Name);
					isValid = true; // TODO hack for skip validation
					break;
				case EWelcomeSteps.Second:
					_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Folder Settings Page");
					if (string.IsNullOrWhiteSpace(SessionViewModel.FolderName))
					{
						SessionViewModel.FolderName = Strings.DefaultFolderName;
					}
					else
					{
						SessionViewModel.ErrorMessage = "";
					}
					_logService.CreateLogDataAsync($"{Constants.Logs.DATA_ENTERED}: Folder name " + SessionViewModel.FolderName);
					break;
				case EWelcomeSteps.Third:
					_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: User Details Page");

					if (string.IsNullOrWhiteSpace(UserDetailsViewModel.Name) ||
						string.IsNullOrWhiteSpace(UserDetailsViewModel.EmpId) ||
						string.IsNullOrWhiteSpace(UserDetailsViewModel.Role) ||
						string.IsNullOrWhiteSpace(UserDetailsViewModel.Email))
					{
						//isValid = false; TODO check nessarity for skip all
						//UserDetailsViewModel.Message = Strings.PleaseEnterAllFields;
					}
					else
					{
						UserDetailsViewModel.ErrorMessage = "";
					}
					_logService.CreateLogDataAsync($"{Constants.Logs.DATA_ENTERED}: User name: " + UserDetailsViewModel.Name +
												   " User id: " + UserDetailsViewModel.EmpId +
												   " User role: " + UserDetailsViewModel.Role +
												   " User Email: " + UserDetailsViewModel.Email);
					isValid = true; // TODO hack for skip validation
					break;
				case EWelcomeSteps.Fourth:

					break;
				case EWelcomeSteps.Fifth:

					break;
				default:
					break;
			}
			return isValid;
		}

        private async void NavigateToCharts()
		{
			await _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigate to Multichart View");

			var navParams = new NavigationParameters();
			navParams.Add(Constants.NavigationParamsKeys.DEVICE_NAME, CurrentDevice.DeviceName);
			await _navigationService.NavigateAsync('/' + nameof(NavigationPage) + '/' + nameof(ChartsView), navParams);
        }

        private async Task OnBackCommandExecute()
		{
            try
            {
                await _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Back command execute. Current step:  = {CurrentStep}");

                if (_IsFromChartsView)
                {
                    await _navigationService.GoBackAsync();
                }

                if (CurrentStep > EWelcomeSteps.First)
                {
                    if (CurrentStep == EWelcomeSteps.Fourth)
                    {
                        await StopDiscoverDevicesAsync();
                        await DisconnectedAndResetCurrentAndLastConnectedDevicesAsync();
                        Devices = new ObservableCollection<DeviceConnectionViewModel>();
                        UpdateSelectedDevices(Devices.FirstOrDefault());
                    }

                    ValidationAllFieldData(CurrentStep);
                    CurrentStep = CurrentStep - 1;
                }
            }
            catch (System.Exception ex)
            {

            }
            finally
            {
                //H.H.
                Constants.ForceReconnection = false;
                Constants.ForceRecordingFile = false;
                _lastConnectedDeviceId = null;
            }
           
		}

		private async Task OnRetryConnectionCommand()
		{
			await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Retry connection");

			await DisconnectCurrentDeviceAsync();

            TryConnectLastDeviceOrDiscoverDevices();

        }

        private void OnDiscoveredDeviceTappedCommand(DeviceConnectionViewModel aModel)
		{
            var tappedDevice = aModel;
            if (tappedDevice != null)
			{
                UpdateSelectedDevices(tappedDevice);
            }
        }

		private Task SaveVMDataAsync()
		{
			return Task.Run(async () =>
			{
				await _appSettings.InsertObjectAsync<PatientDetailsViewModel>(Constants.StorageKeys.PATIENT_DETAIL, PatientDetailsViewModel);
				await _appSettings.InsertObjectAsync<SessionViewModel>(Constants.StorageKeys.SESSION_DETAIL, SessionViewModel);
				await _appSettings.InsertObjectAsync<UserDetailsViewModel>(Constants.StorageKeys.USER_DETAIL, UserDetailsViewModel);
				await _appSettings.InsertObjectAsync<OtherDetailsViewModel>(Constants.StorageKeys.OTHER_DETAIL, OtherDetailsViewModel);
			});
		}

        /// <summary>
        /// Updates status of selected devices.
        /// </summary>
        /// <param name="aCheckedDevice">The actual device to be selected, or null.</param>
        private void UpdateSelectedDevices(DeviceConnectionViewModel aCheckedDevice)
        {
            if (Equals(CurrentDevice?.Device.Id, aCheckedDevice?.Model.Device.Id))
            {
                // same device is already selected, nothing to do
                return;
            }

            // uncheck any checked device at the moment
            foreach (var device in Devices
                .Where(device => !ReferenceEquals(aCheckedDevice, device))
                .Where(device => device.IsSelected))
            {
                device.IsSelected = false;
            }

            CurrentDevice = null;

            if (aCheckedDevice != null)
            {
                // check the device
                aCheckedDevice.IsSelected = true;

                _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Selected device '{aCheckedDevice.DeviceName}'");

                CurrentDevice = aCheckedDevice.Model;
                _lastConnectedDeviceId = null;
            }
        }

        /// <summary>
        /// Checks if the bluetooth enabled, and shows an alert if it's disabled.
        /// </summary>
        /// <returns>true, if bluetooth is enabled; otherwise, false.</returns>
        private async Task<bool> CheckBluetoothEnabledOrAlert()
        {
            if (_blueToothService.BlueToothIsOn)
            {
                return true;
            }

            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Bluetooth turned off");
            await _userDialogs.AlertAsync(Strings.BluetoothNotEnabledAlert);

            // clean any connected/chosen device and list of found devices
            Devices = new ObservableCollection<DeviceConnectionViewModel>();
            UpdateSelectedDevices(Devices.FirstOrDefault());

            return false;
        }

        /// <summary>
        /// Tries to scan for last connected device, if any.
        /// </summary>
        /// <returns>last connected device, if it's in coverage; otherwise, false.</returns>
        private async Task<DeviceModel> TryFindLastConnectedDeviceAsync()
        {
            if (!string.IsNullOrEmpty(_lastConnectedDeviceId))
            {
                var isDeviceFound = await _blueToothService.ScanningDeviceByIdAsync(_lastConnectedDeviceId);
                if (isDeviceFound)
                {
                    return _blueToothService.CurrentDeviceAsModel;
                }
            }

            return null;
        }

        /// <summary>
        /// Discovers more devices and updates UI.
        /// </summary>
        /// <returns></returns>
        private async Task StartDiscoverDevicesAsync()
        {
            var isFoundDevices = await _blueToothService.ScanningListOfDevicesAsync();
            if (isFoundDevices)
            {
                Devices = _blueToothService.Devices
                    .Select(device => new DeviceConnectionViewModel(device))
                    .ToList();
                UpdateSelectedDevices(Devices.FirstOrDefault());

                await _logService.CreateLogDataAsync(
                    $"{Constants.Logs.EVENT}: Device was found. Device count = {Devices.Count}");
            }
            else
            {
                await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: No device found");
            }
        }

        /// <summary>
        /// Stops discovering devices, if it was launched before.
        /// </summary>
        private async Task StopDiscoverDevicesAsync()
        {
            await _blueToothService.StopScanningDevicesAsync();
        }

        /// <summary>
        /// Establishes connection to a remote device, if possible.
        /// </summary>
        /// <param name="aDevice">The device to connect to.</param>
        /// <returns>true, if successfully connected; otherwise, false.</returns>
        private async Task<bool> ConnectToRemoteDeviceAsync(DeviceModel aDevice)
        {
            var isConnectSuccess = await _blueToothService.ConnectDeviceAsync(aDevice.Device);
            return isConnectSuccess;
        }

        /// <summary>
        /// Remembers the device as chosen and connected, and navigates to the charts page.
        /// </summary>
        private async Task RememberCurrentConnectedDeviceAndNavigateToCharts()
        {
            await _logService.CreateLogDataAsync(
                $"{Constants.Logs.EVENT}: Device '{CurrentDevice.DeviceName}' connected successfully");

            // remember selected device
            _lastConnectedDeviceId = CurrentDevice.Device.Id.ToString();
            await _appSettings.InsertObjectAsync(Constants.StorageKeys.CONNECTED_DEVICE, _lastConnectedDeviceId);
            _blueToothService.CurrentDevice = CurrentDevice.Device;

            // show the 'charts' page
            NavigateToCharts();
        }

        /// <summary>
        /// Tries to connect to last used device. Otherwise starts discovering new devices.
        /// </summary>
        private async void TryConnectLastDeviceOrDiscoverDevices()
        {
            await _navigationService.NavigateAsync(nameof(AudioRecognitionView));
            return;


            _userDialogs.HideLoading();//H.H.

            //await DisconnectCurrentDeviceAsync();

            // check bluetooth enabled
            if (await CheckBluetoothEnabledOrAlert())
            {
                // reset UI state
                IsShowWorningAlert = false;
                _wasDisconnectedDeviceOnChartsView = false;

                try
                {
                    IsShowLoader = true;

                    // try to find/connect to last connected device, if any.
                    var device = await TryFindLastConnectedDeviceAsync();

                    /*
                    if (Constants.ForceReconnection == true && device == null)
                    {
                        TryConnectLastDeviceOrDiscoverDevices();
                        return;
                    }*/

                  
                    while (Constants.ForceReconnection == true && device == null)//H.H.
                    {
                        await Task.Delay(1000);
                        device = await TryFindLastConnectedDeviceAsync();
                    }

                    if (device != null)
                    {
                        CurrentDevice = device;

                        // try to connect to the device
                        var isConnected = await ConnectToRemoteDeviceAsync(CurrentDevice);
                        if (isConnected)
                        {
                            // remember the device, and open charts
                            await RememberCurrentConnectedDeviceAndNavigateToCharts();
                            Constants.ForceReconnection = true;
                        }
                        else
                        {
                            // start searching new devices
                            await StartDiscoverDevicesAsync();
                        }
                }
                    else
                    {
                        // start searching new devices
                        await StartDiscoverDevicesAsync();
                    }
                }
                finally
                {
                    IsShowLoader = false;
                }
            }
        }

        private async void SetDisconnectedDevicesStatus()
		{
			CurrentDevice = null;

            if (Devices.Count > 0)
			{
				IsShowWorningAlert = true;

				await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Show disconnected device status = {IsShowWorningAlert}");
				await DisconnectCurrentDeviceAsync();
			}
			else
			{
				IsShowWorningAlert = false;
			}
		}

		private async Task DisconnectCurrentDeviceAsync()
        {
            var currentDevice = _blueToothService.CurrentDevice;
            if (currentDevice != null)
			{
				await _blueToothService.DisconnectDeviceAsync(currentDevice);

				CurrentDevice = null;
				//_lastConnectedDeviceId = null;
                 //Constants.ForceReconnection=false;
                _wasDisconnectedDeviceOnChartsView = false;

                await _appSettings.InsertObjectAsync<string>(Constants.StorageKeys.CONNECTED_DEVICE, null);
                await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Device disconnected '{currentDevice.Name}'");
			}
        }

        /// <summary>
        /// Disconnects from any currently connected device, and resets any remembered connected device, if any.
        /// </summary>
        private async Task DisconnectedAndResetCurrentAndLastConnectedDevicesAsync()
        {
            await DisconnectCurrentDeviceAsync();

            CurrentDevice = null;
            //_lastConnectedDeviceId = null;
            _wasDisconnectedDeviceOnChartsView = false;

            await _appSettings.InsertObjectAsync<string>(Constants.StorageKeys.CONNECTED_DEVICE, null);
        }

        /// <summary>
        /// Updates availability of all commands in the view model.
        /// </summary>
        private void UpdateCommandsAvailability()
        {
            IsStartAllowed = CurrentDevice != null && !IsShowLoader;
        }

        #endregion

	}
}