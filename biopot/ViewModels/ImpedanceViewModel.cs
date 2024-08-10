using System;
using System.Diagnostics;
using System.Collections.Generic;
using biopot.Enums;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using biopot.Helpers;
using biopot.Models;
using biopot.Resources.Strings;
using Prism.Navigation;
using biopot.Services.Charts;
using SharedCore.Services;
using SharedCore.Services.Characteristic3Service;
using System.Linq;
using biopot.Views;

namespace biopot.ViewModels
{
	public class ImpedanceViewModel : BaseViewModel
	{
		private readonly INavigationService _navigationService;
		private readonly IUserDialogs _userDialogs;
		private readonly ILogService _logService;
		private readonly IBlueToothService _blueToothService;
		private readonly IChartManagerService _chartsManagerService;
        private readonly ICharacteristic3Service _characteristic3Service;
        private readonly IChartService _chartService;
        public event EventHandler<ImpedanceDataEventArgs> ImpedanceDataLoaded;
        private static readonly TimeSpan TimerLockRetryInterval = TimeSpan.FromMilliseconds(10);
        private static readonly TimeSpan TimerImpedanceInterval = TimeSpan.FromMilliseconds(1500);
        private readonly System.Timers.Timer _timerImpedance;
        private SpinLock _timerLockImp = new SpinLock();

        private String char3Value;
        public ImpedanceViewModel(IChartManagerService chartsManagerService
            ,IUserDialogs userDialogs,
            INavigationService navigationService,
            ILogService logService,
            IBlueToothService blueToothService,
            ICharacteristic3Service characteristic3Service,
            IChartService chartService)
		{
			_navigationService = navigationService;
			_userDialogs = userDialogs;
			_chartsManagerService = chartsManagerService;
			_logService = logService;
			_blueToothService = blueToothService;
            _characteristic3Service = characteristic3Service;
            _chartService = chartService;

            _timerImpedance = new System.Timers.Timer(TimerImpedanceInterval.TotalMilliseconds);
            _timerImpedance.Elapsed += (aSender, aArgs) =>
            {
                bool isLockTaken = false;
                _timerLockImp.TryEnter(TimerLockRetryInterval, ref isLockTaken);
                if (isLockTaken)
                {
                    try
                    {
                        HandleImpElapsedEventHandler();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"{nameof(HandleImpElapsedEventHandler)}: exception: {e}");
                    }
                    finally
                    {
                        _timerLockImp.Exit();
                    }
                }
                else
                {
                   
                }
            };

            _timerImpedance.Enabled = false;
            Subscribe();
		}

		#region -- Public properties --

		private IList<SensorConnectionModel> _SensorConnectionList;
		public IList<SensorConnectionModel> SensorConnectionList
		{
			get { return _SensorConnectionList; }
			set { SetProperty(ref _SensorConnectionList, value); }
		}

        public bool IsMajorityInRange => SensorConnectionList.Count > 0 &&
                                     SensorConnectionList.Count(s => (s.SignalValue >= 1500 && s.SignalValue <= 10000) || s.SignalValue >= 15000) > SensorConnectionList.Count / 2;

        private ICommand _BackCommand;
		public ICommand BackCommand => _BackCommand ?? (_BackCommand = SingleExecutionCommand.FromFunc(OnBackCommand));

        private ICommand _ContinueCommand;
        public ICommand ContinueCommand => _ContinueCommand ?? (_ContinueCommand = SingleExecutionCommand.FromFunc(OnContinueCommand));

        #endregion

        #region -- Overrides --

        public override void Destroy()
		{
			Unsubscribe();
			base.Destroy();
		}

		public override void OnNavigatingTo(NavigationParameters parameters)
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Navigating to Impedance page");

			IList<SensorConnectionModel> sensorConnectionList = null;
			parameters.TryGetValue("SensorConnectionList", out sensorConnectionList);
			if (sensorConnectionList != null)
			{
				SensorConnectionList = sensorConnectionList;
			}
		}

		#endregion

		#region -- Private helpers --

		private async void Subscribe()
		{
            ImpedanceDataLoaded += OnImpedanceDataLoaded;
            var asyncResult = await _characteristic3Service.SetImpedanceEnabledAsync(true);
            _timerImpedance.Enabled = true;
        }
        private async void Unsubscribe()
        {
            _timerImpedance.Enabled = false;
            var asyncResult = await _characteristic3Service.SetImpedanceEnabledAsync(false);
            ImpedanceDataLoaded -= OnImpedanceDataLoaded;
            await Task.Delay(10);
        }



        private async void HandleImpElapsedEventHandler()
        {
            //read char 3 here and update the impedance values
            var res = await _characteristic3Service.GetInfoAsync();
            if (res.IsSuccess)
            {
                char3Value = res.Result;
                ProcessChar3Data(char3Value);
            }
            else
            {
               // await ShowErrorDialog(res.Message);
            }

            //await _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Ch3 Value Read Command");
        }

        private void ProcessChar3Data(String Data )
        {
            var impedanceData = UpdateDataForImpedance(Data);
            if (impedanceData != null && impedanceData.Count > 0)
            {
                ImpedanceDataLoaded?.Invoke(this, new ImpedanceDataEventArgs(impedanceData));
            }
        }


        private IReadOnlyDictionary<int, double> UpdateDataForImpedance(String data)
        {
            var result = new Dictionary<int, double>();
            byte[] bytes = GetBytes(data);
            for (int i = 0; i < bytes.Length; i++)
            {
                result[i+1] = (double)(bytes[i] * 100);
            }
            return result;
        }
        public byte[] GetBytes(String str)
        {
            // str.Replace(@"-", "");
            string[] s = str.Split('-');
            byte[] bytes = new byte[s.Length - 6];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = byte.Parse(s[i+3], System.Globalization.NumberStyles.HexNumber);
            return bytes;
        }

        /// <summary>
        /// Handles update of impedance data.
        /// </summary>
        private void OnImpedanceDataLoaded(object aSender, ImpedanceDataEventArgs aArgs)
        {
            foreach (var sensorModel in SensorConnectionList)
            {
                if (aArgs.FullData.TryGetValue(sensorModel.SensorId, out var value))
                {
                    sensorModel.SignalValue = (int) value;
                }
                else
                {
                    sensorModel.SignalValue = 0;
                }
            }

            RaisePropertyChanged(nameof(SensorConnectionList));
            RaisePropertyChanged(nameof(IsMajorityInRange));
        }

        private async Task OnContinueCommand()
        {
            await _navigationService.NavigateAsync(nameof(TestInstructionView));
        }

        private Task OnBackCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Back button Navigate from Impedance page");
            return _navigationService.GoBackAsync(new NavigationParameters
            {
                {Constants.NavigationParamsKeys.NAV_FROM_IMPEDANCE, true},
                {Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN, true},
            });
		}

		private void OnDeviceConnectionChanged(object sender, bool isConnected)
		{
			if (!isConnected)
			{
				_logService.WriteInFileAsync();
			}
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Device connection state = {isConnected}");
		}

		#endregion
	}
}
