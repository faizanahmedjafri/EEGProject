using biopot.Helpers;
using biopot.Views;
using Prism.Navigation;
using SharedCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace biopot.ViewModels
{
    public class TestResultViewModel : BaseViewModel
    {
        private readonly ILogService _logService;
        private readonly INavigationService _navigationService;
        private string _audioName;
        private string _audioData;

        public TestResultViewModel(ILogService logService, INavigationService navigationService)
        {
            _logService = logService;
            _navigationService = navigationService;
        }

        private ICommand _CloseCommand;
        public ICommand CloseCommand => _CloseCommand ?? (_CloseCommand = SingleExecutionCommand.FromFunc(OnCloseCommand));
        private Task OnCloseCommand()
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.NAVIGATION}: Close button Navigate from Test results page");

            NavigationParameters navigationParameters = new NavigationParameters();
            navigationParameters.Add("EndTask", true);
            navigationParameters.Add("AudioData", _audioData);
            navigationParameters.Add("AudioName", _audioName);

            return _navigationService.NavigateAsync('/' + nameof(NavigationPage) + '/' + nameof(MainView), navigationParameters);
        }

        public override async void OnNavigatingTo(NavigationParameters parameters)
        {
            parameters.TryGetValue("AudioName", out _audioName);
            parameters.TryGetValue("AudioData", out _audioData);
        }
    }
}
