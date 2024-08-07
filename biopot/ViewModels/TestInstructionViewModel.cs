using biopot.Helpers;
using biopot.Views;
using Prism.Navigation;
using SharedCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        private async Task OnStartTestCommand()
        {
            await _navigationService.NavigateAsync(nameof(AudioRecognitionView));
        }
    }
}
