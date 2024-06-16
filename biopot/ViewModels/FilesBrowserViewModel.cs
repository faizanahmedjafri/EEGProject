using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using biopot.Enums;
using biopot.Helpers;
using biopot.Services;
using biopot.Services.Sharing;
using Prism.Navigation;
using SharedCore.Services;
using Xamarin.Forms.Internals;

namespace biopot.ViewModels
{
	public class FileViewModel : BaseViewModel
	{
		private readonly ILogService _logService;

		public string Name { get; set; }
		public string Path { get; set; }
		private bool _Checked = false;
		public bool Checked
		{
			get { return _Checked; }
			set { SetProperty(ref _Checked, value); }
		}
		private bool _Enabled = false;
		public bool Enabled
		{
			get { return _Enabled; }
			set { SetProperty(ref _Enabled, value); }
		}
	}
	public class FilesBrowserViewModel : BaseViewModel
	{
		private readonly INavigationService _navigationService;
		private readonly IUserDialogs _userDialogs;
		private readonly IFileIoService _fileIoService;
		private readonly IFolderService _folderService;
		private readonly IShareService _shareService;
		private SessionViewModel _sessionVM;
		private readonly IAppSettingsManagerService _appSettingsManagerService;
		private readonly ILogService _logService;
		private readonly IBlueToothService _blueToothService;

		public FilesBrowserViewModel(IUserDialogs userDialogs,
									 INavigationService navigationService,
									 IFileIoService fileIoService,
									 IFolderService folderService,
									 IAppSettingsManagerService appSettingsManagerService,
									 IShareService shareService,
									 ILogService logService,
									 IBlueToothService blueToothService)
		{
			_navigationService = navigationService;
			_userDialogs = userDialogs;
			_fileIoService = fileIoService;
			_folderService = folderService;
			_appSettingsManagerService = appSettingsManagerService;
			_shareService = shareService;
			_logService = logService;
			_blueToothService = blueToothService;

			LoadData();

			Subscribe();
		}

		public override void Destroy()
		{
			base.Destroy();
			Unsubscribe();
		}

		#region -- Public properties --

		private ICommand _BackCommand;
		public ICommand BackCommand => _BackCommand ?? (_BackCommand = SingleExecutionCommand.FromFunc(OnBackCommand));

		private ICommand _ItemTappedCommand;
		public ICommand ItemTappedCommand => _ItemTappedCommand ?? (_ItemTappedCommand = SingleExecutionCommand.FromFunc(OnItemTappedCommand));

		private ICommand _ShareFilesCommand;
		public ICommand ShareFilesCommand => _ShareFilesCommand ?? (_ShareFilesCommand = SingleExecutionCommand.FromFunc(OnShareFilesCommand));

		private ICommand _RefreshCommand;
		public ICommand RefreshCommand => _RefreshCommand ?? (_RefreshCommand = SingleExecutionCommand.FromFunc(OnRefreshCommand));

		private ICommand _RemoveFilesCommand;
		public ICommand RemoveFilesCommand => _RemoveFilesCommand ?? (_RemoveFilesCommand = SingleExecutionCommand.FromFunc(OnRemoveFilesCommand));

		private ObservableCollection<FileViewModel> _Files = new ObservableCollection<FileViewModel>();
		public ObservableCollection<FileViewModel> Files
		{
			get { return _Files; }
			set { SetProperty(ref _Files, value); }
		}

		public int SelectedFilesAmount
		{
			get { return this._Files.Where(x => x.Checked == true).Count(); }
		}

		public string CalendarDate
		{
			get
			{
				if (this.FilteringDate == DateTime.Today)
					return "Today";
				return this.FilteringDate.ToString("ddd, MMM yy");
			}
		}

		private bool _IsRefreshing;
		public bool IsRefreshing
		{
			get { return _IsRefreshing; }
			set { SetProperty(ref _IsRefreshing, value); }
		}

		private bool _SelectAll;
		public bool SelectAll
		{
			get { return _SelectAll; }
			set
			{
				if (value == true)
					this._Files.ForEach(x =>
					{
						x.Checked = true;
						x.Enabled = true;
					});
				else
					this._Files.ForEach(x =>
						x.Enabled = false
					);
				RaisePropertyChanged(nameof(SelectedFilesAmount));
				SetProperty(ref _SelectAll, value);
			}
		}

		private DateTime _MinFilteringDate;
		public DateTime MinFilteringDate
		{
			get
			{
				var date = DateTime.Today.AddDays(-30);
				return date;
			}
		}

		public DateTime MaxFilteringDate
		{
			get { return DateTime.Today; }
		}

		private DateTime _FilteringDate = DateTime.Today;
		public DateTime FilteringDate
		{
			get { return _FilteringDate; }
			set
			{
				LoadData();
				SetProperty(ref _FilteringDate, value);
				RaisePropertyChanged(nameof(CalendarDate));
			}
		}

		#endregion

		#region -- Private helpers --

		private Task OnBackCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: On Back Command Tapped on Files Browser");

			var navParams = new NavigationParameters();
			navParams.Add(Constants.NavigationParamsKeys.NAV_FROM_FILE_BROWSER, true);
			navParams.Add(Constants.NavigationParamsKeys.NAV_BACK_TO_SCREEN, true);
			return _navigationService.GoBackAsync(navParams);
		}

		private async Task OnItemTappedCommand(object obj)
		{
			FileViewModel fileModel = (FileViewModel)obj;

			if (fileModel == null)
			{
				return;
			}
			if (fileModel.Enabled == true)
			{
				return;
			}

			fileModel.Checked = !fileModel.Checked;
			RaisePropertyChanged(nameof(SelectedFilesAmount));
		}

		private Task OnShareFilesCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Share Files");

			var files = this.Files.Where(x => x.Checked == true).Select(x => x.Path);
			_shareService.ShareFileAsync(files);
			return Task.FromResult(0);
		}

		private async Task OnRemoveFilesCommand()
		{
			await _logService.CreateLogDataAsync($"{Constants.Logs.ALERT}: delete the file");

			bool resume = await _userDialogs.ConfirmAsync("Are you sure you want to delete the file?");
			await Task.Run(() =>
			{
				if (resume)
				{
					foreach (var file in this.Files)
						if (file.Checked)
						{
							_fileIoService.DeleteFile(file.Path);
							_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: delete the file:  = { file.Path}");

							//this.Files.Remove(file);
						}
					LoadData();
				}
			});
		}

		private void LoadData()
		{
			Task.Run(() =>
			{
				 //Add loader

				 _sessionVM = _appSettingsManagerService.GetObjectAsync<SessionViewModel>(Constants.StorageKeys.SESSION_DETAIL).Result;
				var folderPath = _folderService.GetFolderPath(StringTargetToEnum.ConvertStringTargetToEnum(_sessionVM.SavingTarget), _sessionVM.FolderName);
				var fileNames = _fileIoService.GetFilesGroupedByDay(folderPath, this.FilteringDate);
				ObservableCollection<FileViewModel> newFiles = new ObservableCollection<FileViewModel>();

				foreach (var fileName in fileNames)
				{
					newFiles.Add(new FileViewModel()
					{
						Name = Path.GetFileNameWithoutExtension(fileName),
						Path = fileName,
						Checked = this.SelectAll,
						Enabled = this.SelectAll
					});
				}
				this.Files = newFiles;
				RaisePropertyChanged(nameof(SelectedFilesAmount));
				 //Remove loader
			 });

		}

		private Task OnRefreshCommand()
		{
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Tapped Refresh Command on Files Browser View");

			IsRefreshing = true;
			LoadData();
			IsRefreshing = false;
			return Task.FromResult(0);
		}

		private void Subscribe()
		{
			_blueToothService.OnChangedDeviceConnection += OnDeviceConnectionChanged;
		}

		private void Unsubscribe()
		{
			_blueToothService.OnChangedDeviceConnection -= OnDeviceConnectionChanged;
		}

		private void OnDeviceConnectionChanged(object sender, bool isConnected)
		{
			if (!isConnected)
			{
				_logService.WriteInFileAsync();
			}
			_logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Device Connection State on Files Browser View. IsConnected = {isConnected}");
		}

		#endregion
	}
}
