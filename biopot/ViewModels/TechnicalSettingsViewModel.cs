using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using biopot.Helpers;
using biopot.Resources.Strings;
using Prism.Navigation;
using SharedCore.Extensions;
using SharedCore.Models;
using SharedCore.Services;
using SharedCore.Services.Characteristic3Service;
using SharedCore.Services.Charts;
using SharedCore.Services.RateInfoService;
using SharedCore.Services.TemperatureDataService;

namespace biopot.ViewModels
{
    public class TechnicalSettingsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserDialogs _userDialogs;
        private readonly ILogService _logService;
        private readonly IBiopotInfoChartsService _biopotInfoService;
        private readonly IBiobotDataForChartsService _biopotDataForChartsService;
        private readonly ISamplesRateInfoService _samplesRateInfoService;
        private readonly ITemperatureDataService _batteryDataService;
        private readonly IBlueToothService _blueToothService;
        private readonly ICharacteristic3Service _characteristic3Service;

        private CancellationTokenSource _dialogCancellationTokenSource;

        public TechnicalSettingsViewModel(IUserDialogs aUserDialogs,
            INavigationService navigationService,
            IBlueToothService blueToothService,
            ILogService aLogService,
            IBiopotInfoChartsService biopotInfoService,
            IBiobotDataForChartsService biopotDataForChartsService,
            ISamplesRateInfoService samplesRateInfoService,
            ITemperatureDataService batteryDataService, 
            ICharacteristic3Service characteristic3Service)
        {
            _userDialogs = aUserDialogs;
            _logService = aLogService;
            _navigationService = navigationService;
            _blueToothService = blueToothService;
            _biopotInfoService = biopotInfoService;
            _biopotDataForChartsService = biopotDataForChartsService;
            _samplesRateInfoService = samplesRateInfoService;
            _batteryDataService = batteryDataService;
            _characteristic3Service = characteristic3Service;

            _dialogCancellationTokenSource = new CancellationTokenSource();

            ReadSettingsAsync();
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            _dialogCancellationTokenSource.Cancel();
            _dialogCancellationTokenSource.Dispose();
            _dialogCancellationTokenSource = null;

            base.Destroy();
        }

        #region -- Public properties --

        private string _ch1Value = "00";
        public string Ch1Value
        {
            get => _ch1Value;
            set
            {
                if (SetProperty(ref _ch1Value, value))
                {
                    AreSettingsChanged = true;
                    Char1Changed = true;
                }
            }
        }

        private string _ch2Value = "00";
        public string Ch2Value
        {
            get => _ch2Value;
            set
            {
                if (SetProperty(ref _ch2Value, value))
                {
                    AreSettingsChanged = true;
                    Char2Changed = true;
                }
            }
        }

        private string _ch3Value = "00";
        public string Ch3Value
        {
            get => _ch3Value;
            set
            {
                if (SetProperty(ref _ch3Value, value))
                {
                    AreSettingsChanged = true;
                    Char3Changed = true;
                }
            }
        }

        private string _ch5Value = "00";
        public string Ch5Value
        {
            get => _ch5Value;
            set
            {
                if (SetProperty(ref _ch5Value, value))
                {
                    AreSettingsChanged = true;
                    Char5Changed = true;
                }
            }
        }

        private string _ch6Value = "00";
        public string Ch6Value
        {
            get => _ch6Value;
            set => SetProperty(ref _ch6Value, value);
        }

        private bool _areSettingsChanged;
        public bool AreSettingsChanged
        {
            get => _areSettingsChanged;
            set => SetProperty(ref _areSettingsChanged, value);
        }

        private bool _char1Changed;
        public bool Char1Changed
        {
            get => _char1Changed;
            set => SetProperty(ref _char1Changed, value);
        }

        private bool _char2Changed;
        public bool Char2Changed
        {
            get => _char2Changed;
            set => SetProperty(ref _char2Changed, value);
        }

        private bool _char3Changed;
        public bool Char3Changed
        {
            get => _char3Changed;
            set => SetProperty(ref _char3Changed, value);
        }

        private bool _char5Changed;
        public bool Char5Changed
        {
            get => _char5Changed;
            set => SetProperty(ref _char5Changed, value);
        }




        private int _stripTabIndex;
        public int StripTabIndex
        {
            get => _stripTabIndex;
            set => SetProperty(ref _stripTabIndex, value);
        }


        private ICommand _backCommand;
        public ICommand BackCommand => _backCommand ?? (_backCommand = SingleExecutionCommand.FromFunc(OnBackCommandAsync));

        private ICommand _applyCommand;
        public ICommand ApplyCommand => _applyCommand ?? (_applyCommand = SingleExecutionCommand.FromFunc(OnApplyCommandAsync));

        private ICommand _ch1ValueReadCommand;
        public ICommand Ch1ValueReadCommand => _ch1ValueReadCommand ?? (_ch1ValueReadCommand = SingleExecutionCommand.FromFunc(OnCh1ValueReadCommandAsync));

        private ICommand _ch2ValueReadCommand;
        public ICommand Ch2ValueReadCommand => _ch2ValueReadCommand ?? (_ch2ValueReadCommand = SingleExecutionCommand.FromFunc(OnCh2ValueReadCommandAsync));

        private ICommand _ch3ValueReadCommand;
        public ICommand Ch3ValueReadCommand => _ch3ValueReadCommand ?? (_ch3ValueReadCommand = SingleExecutionCommand.FromFunc(OnCh3ValueReadCommandAsync));

        private ICommand _ch5ValueReadCommand;
        public ICommand Ch5ValueReadCommand => _ch5ValueReadCommand ?? (_ch5ValueReadCommand = SingleExecutionCommand.FromFunc(OnCh5ValueReadCommandAsync));

        private ICommand _ch6ValueReadCommand;
        public ICommand Ch6ValueReadCommand => _ch6ValueReadCommand ?? (_ch6ValueReadCommand = SingleExecutionCommand.FromFunc(OnCh6ValueReadCommandAsync));

        #endregion

        #region Read values

        /// <summary>
        /// Reads Characteristic 1.
        /// </summary>
        private async Task OnCh1ValueReadCommandAsync()
        {
            var res = await _biopotInfoService.GetInfoAsync();
            if (res.IsSuccess)
            {
                Ch1Value = res.Result;
            }
            else
            {
                await ShowErrorDialog(res.Message);
            }

            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Ch1 Value Read Command");
        }

        /// <summary>
        /// Reads Characteristic 2.
        /// </summary>
        private async Task OnCh2ValueReadCommandAsync()
        {
            var res = await _biopotDataForChartsService.GetDataStateAsync();
            if (res.IsSuccess)
            {
                var val = ((int) res.Result);
                Ch2Value = val.ToString($"0{0}");
            }
            else
            {
                await ShowErrorDialog(res.Message);
            }

            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Ch2 Value Read Command");
        }

        /// <summary>
        /// Reads Characteristic 3.
        /// </summary>
        private async Task OnCh3ValueReadCommandAsync()
        {
            var res = await _characteristic3Service.GetInfoAsync();
            if (res.IsSuccess)
            {
                Ch3Value = res.Result;
            }
            else
            {
                await ShowErrorDialog(res.Message);
            }

            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Ch3 Value Read Command");
        }

        /// <summary>
        /// Reads Characteristic 5.
        /// </summary>
        private async Task OnCh5ValueReadCommandAsync()
        {
            var res = await _samplesRateInfoService.GetInfoAsync();
            if (res.IsSuccess)
            {
                Ch5Value = res.Result;
            }
            else
            {
                await ShowErrorDialog(res.Message);
            }

            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Ch5Value Read Cmmand");
        }

        /// <summary>
        /// Reads Characteristic 6.
        /// </summary>
        private async Task OnCh6ValueReadCommandAsync()
        {
            var res = await _batteryDataService.GetInfoAsync(_blueToothService.CurrentDevice);
            if (res.IsSuccess)
            {
                Ch6Value = res.Result;
            }
            else
            {
                await ShowErrorDialog(res.Message);
            }

            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Ch5Value Read Cmmand");
        }
        #endregion

        /// <summary>
        /// Handles 'apply' button click.
        /// </summary>
        private async Task OnApplyCommandAsync()
        {
            await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Apply Command. On Setup View");

            if (ValidateSettings())
            {
                await ApplySettingsAsync();

                await _navigationService.GoBackAsync();
            }
        }

        /// <summary>
        /// Reads values from all characteristics.
        /// </summary>
        private async void ReadSettingsAsync()
        {
            await OnCh1ValueReadCommandAsync();
            await OnCh2ValueReadCommandAsync();
            await OnCh3ValueReadCommandAsync();
            await OnCh5ValueReadCommandAsync();
            await OnCh6ValueReadCommandAsync();

            AreSettingsChanged = false;
            Char1Changed = false;
            Char2Changed = false;
            Char3Changed = false;
            Char5Changed = false;
         
        }

        /// <summary>
        /// Shows error dialog. 
        /// </summary>
        /// <param name="aErrorCode"> The error code. </param>
        private async Task ShowErrorDialog(string aErrorCode)
        {
            _dialogCancellationTokenSource.Cancel();
            _dialogCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await _userDialogs.AlertAsync(string.Format(Strings.ErrorSignalRetrieveMessage, aErrorCode),
                    Strings.ErrorSignalRetrieveTitle, Strings.Ok, _dialogCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }

        /// <summary>
        /// Validates data.
        /// </summary>
        private bool ValidateSettings()
        {
            var ch1 = _biopotInfoService.IsValidDataToWrite(Ch1Value);
            var ch2 = _biopotDataForChartsService.IsValidDataToWrite(Ch2Value);
            var ch3 = _characteristic3Service.IsValidDataToWrite(Ch3Value);
            var ch5 = _samplesRateInfoService.IsValidDataToWrite(Ch5Value);
            var isValidData = ch1 && ch2 && ch3 && ch5;

            if (!isValidData)
            {
                // FIXME move strings to resources
                _userDialogs.AlertAsync($"Ch1: {ch1}, Ch2: {ch2}, Ch3: {ch3}, Ch5: {ch5},", 
                    "Not valid data", Strings.Ok);
            }

            return isValidData;
        }

        /// <summary>
        /// Applies new technichal settings.
        /// </summary>
        private async Task ApplySettingsAsync()
        {
            AOResult ch1Result = new AOResult();
            AOResult ch2Result = new AOResult(); 
            AOResult ch3Result = new AOResult(); 
            AOResult ch5Result = new AOResult();
            ch1Result.SetSuccess();
            ch2Result.SetSuccess();
            ch3Result.SetSuccess();
            ch5Result.SetSuccess();
            if (Char1Changed)
            {
                ch1Result = await _biopotInfoService.SetDataAsync(Ch1Value);
            }
            if (Char2Changed)
            {
                ch2Result = await _biopotDataForChartsService.SetDataAsync(Ch2Value);
            }
            if (Char3Changed)
            {
                ch3Result = await _characteristic3Service.SetDataAsync(Ch3Value);
            }
            if (Char5Changed)
            {
                ch5Result = await _samplesRateInfoService.SetDataAsync(Ch5Value);
            }

            var results = new List<AOResult>
            {
                ch1Result,
                ch2Result,
                ch3Result,
                ch5Result
            };

            // get first error code
            var errorResult = results.FirstOrDefault(result => !result.IsSuccess);
            if (errorResult != null)
            {
                await _userDialogs.AlertAsync(string.Format(Strings.ErrorSignalRetrieveMessage, errorResult.Message),
                    Strings.ErrorSignalRetrieveTitle, Strings.Ok);
            }
        }

        /// <summary>
        /// Handles back navigation.
        /// </summary>
        private async Task OnBackCommandAsync()
        {
            await _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Back command from Setup View");

            NavigationParameters keys = new NavigationParameters();
            keys.Add(Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN, true);

            // FIXME move strings to resources + on 'SetupViewModel'
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
    }
}
