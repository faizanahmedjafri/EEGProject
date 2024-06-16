using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using biopot.Helpers;
using biopot.ViewModels;
using Prism.Navigation;

namespace biopot.ViewModels
{
	public class BandWidthViewModel : BaseViewModel
	{
		private readonly INavigationService _navigationService;
		private readonly IUserDialogs _userDialogs;

		public BandWidthViewModel(IUserDialogs userDialogs, INavigationService navigationService)
		{
			_navigationService = navigationService;
			_userDialogs = userDialogs;
		}

		#region -- Public properties --

		private ICommand _BackCommand;
		public ICommand BackCommand => _BackCommand ?? (_BackCommand = SingleExecutionCommand.FromFunc(OnBackCommand));

		#endregion

		#region -- Private helpers --

		private Task OnBackCommand()
		{
			return _navigationService.GoBackAsync();
		}

		#endregion
	}
}
