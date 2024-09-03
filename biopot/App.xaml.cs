//#define PERFORMANCE
using Acr.UserDialogs;
using biopot.Services.Charts;
using biopot.Services.FileIO;
using biopot.Services.LogService;
using biopot.Services.SaveData;
using biopot.Services;
using biopot.Views;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Settings.Abstractions;
using Prism.Ioc;
using Prism.Unity;
using SharedCore.Services.Charts;
using SharedCore.Services.RateInfoService;
using SharedCore.Services;
using SharedCore.Services.Characteristic3Service;
using SharedCore.Services.Performance;
using SharedCore.Services.TemperatureDataService;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using SharedCore.Services.MediaService;
using biopot.Services.MediaService;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace biopot
{
	public partial class App : PrismApplication
	{
		private ILogService _logService;
        private int debugVal = 0;

		public static T Resolve<T>()
		{
			return (Current as App).Container.Resolve<T>();
		}

		public App(Prism.IPlatformInitializer initializer = null) : base(initializer)
		{
			_logService = Resolve<ILogService>();
		}

        /// <inheritdoc />
        public override void Initialize()
        {
#if PERFORMANCE
            Performance.Provider = new DefaultPerformanceProvider();
#endif

            base.Initialize();
        }

        /// <inheritdoc />
        protected override void OnInitialized()
		{
            debugVal++;
            InitializeComponent();

			MainPage = new NavigationPage(new MainView()) { BackgroundColor = Color.White };
		}

        /// <inheritdoc />
        protected override void OnStart()
		{
			// Handle when your app starts
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Started Application {Constants.Logs.APP_NAME}");

#if !DEBUG
            Microsoft.AppCenter.AppCenter.Start("android=65d63bcf-9648-49b9-99c4-8b418f94d8a4",
                typeof(Microsoft.AppCenter.Analytics.Analytics),
                typeof(Microsoft.AppCenter.Crashes.Crashes));
#endif
			ReadFile();
		}

		protected override void OnSleep()
		{
#if DEBUG
            if (Performance.Provider is DefaultPerformanceProvider provider)
            {
                _logService.CreateLogDataAsync($"{Constants.Logs.ALERT}: {provider.DumpStats()}");
            }
#endif

            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Closed Application {Constants.Logs.APP_NAME}");
			SaveFile();
		}

		protected override void OnResume()
		{
            // Handle when your app resumes
            debugVal++;


        }

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			//Services
			containerRegistry.RegisterInstance<IUserDialogs>(UserDialogs.Instance);
//#if DEBUG
//            containerRegistry.RegisterInstance<IBluetoothLE>(new SharedCore.Services.Bluetooth.Fake.FakeBluetoothLE());
//#else
            containerRegistry.RegisterInstance<IBluetoothLE>(Plugin.BLE.CrossBluetoothLE.Current);
//#endif

			containerRegistry.RegisterInstance<IAdapter>(Container.Resolve<IBluetoothLE>().Adapter);
			containerRegistry.RegisterInstance<IBlueToothService>(Container.Resolve<BlueToothService>());
			containerRegistry.RegisterInstance<IDeviceDataTransferService>(Container.Resolve<DeviceDataTransferService>());
			containerRegistry.RegisterInstance<ISettings>(Plugin.Settings.CrossSettings.Current);
			containerRegistry.RegisterInstance<IStorageService>(Container.Resolve<NativeStorageService>());
			containerRegistry.RegisterInstance<IAppSettingsManagerService>(Container.Resolve<AppSettingsManagerService>());
			containerRegistry.RegisterInstance(Container.Resolve<IFolderService>());
			containerRegistry.RegisterInstance<IFileIoService>(Container.Resolve<FileIoService>());
			containerRegistry.RegisterInstance<ISignalFilterService>(Container.Resolve<SignalFilterService>());
			containerRegistry.RegisterInstance<IBiopotInfoChartsService>(Container.Resolve<BiopotInfoChartsService>());
			containerRegistry.RegisterInstance<ISamplesRateInfoService>(Container.Resolve<SamplesRateInfoService>());
            containerRegistry.RegisterInstance<ICharacteristic3Service>(Container.Resolve<Characteristic3Service>());
            containerRegistry.RegisterInstance<IBiobotDataForChartsService>(Container.Resolve<BiobotDataForChartsService>());
			containerRegistry.RegisterInstance<IChartService>(Container.Resolve<ChartService>());
            containerRegistry.RegisterInstance<IChartManagerService>(Container.Resolve<ChartManagerService>());
			containerRegistry.RegisterInstance<ISaveDataService>(Container.Resolve<SaveDataService>());
			containerRegistry.RegisterInstance<ITemperatureDataService>(Container.Resolve<BatteryAndTemperatureDataService>());
			containerRegistry.RegisterInstance<ILogService>(Container.Resolve<LogService>());
			containerRegistry.RegisterInstance<IMediaService>(Container.Resolve<MediaService>());

			//Navigation
			containerRegistry.RegisterForNavigation<NavigationPage>();
			containerRegistry.RegisterForNavigation<MainView>();
			containerRegistry.RegisterForNavigation<ChartsView>();
			containerRegistry.RegisterForNavigation<DetailedChartView>();
			containerRegistry.RegisterForNavigation<ImpedanceView>();
			containerRegistry.RegisterForNavigation<FilesBrowserView>();
			containerRegistry.RegisterForNavigation<SetupView>();
			containerRegistry.RegisterForNavigation<BandWidthView>();
			containerRegistry.RegisterForNavigation<TechnicalSettingsView>();

            containerRegistry.RegisterForNavigation<TestPage>();
            containerRegistry.RegisterForNavigation<AudioRecognitionView>();
            containerRegistry.RegisterForNavigation<TestInstructionView>();
            containerRegistry.RegisterForNavigation<TestResultView>();

#if DEBUG
            Plugin.BLE.Abstractions.Trace.TraceImplementation = (aFormat, aArgs) =>
            {
                var logService = Container.Resolve<ILogService>();
                logService.CreateLogDataAsync(string.Format(aFormat, aArgs));
            };
#endif

		}


		#region -- Private helpers --

		private async void SaveFile()
		{
			await _logService.WriteInFileAsync();
		}

		private async void ReadFile()
		{
			await _logService.ReadFileAsync();
		}

		#endregion
	}
}
