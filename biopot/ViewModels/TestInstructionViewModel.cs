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
    public class TestInstructionViewModel : BaseViewModel
    {
        private readonly ILogService _logService;
        private readonly INavigationService _navigationService;

        public TestInstructionViewModel(ILogService logService, INavigationService navigationService)
        {
            _logService = logService;
            _navigationService = navigationService;
        }

        private ICommand _StartTestCommand;
        public ICommand StartTestCommand => _StartTestCommand ?? (_StartTestCommand = SingleExecutionCommand.FromFunc(OnStartTestCommand));
        private Task OnStartTestCommand()
        {
            NavigationParameters navigationParameters = new NavigationParameters();
            navigationParameters.Add("FromTestInstruction", true);
            return _navigationService.NavigateAsync('/' + nameof(NavigationPage) + '/' + nameof(ChartsView), navigationParameters);
        }
    }
}
