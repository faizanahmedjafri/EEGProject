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
    public class TestPageViewModel : BaseViewModel
    {
        private readonly ILogService _logService;
        private readonly INavigationService _navigationService;

        public TestPageViewModel(ILogService logService, INavigationService navigationService)
        {
            _logService = logService;
            _navigationService = navigationService;
        }

        private ICommand _PreviousCommand;
        public ICommand PreviousCommand => _PreviousCommand ?? (_PreviousCommand = SingleExecutionCommand.FromFunc(OnPreviousCommand));
        private async Task OnPreviousCommand()
        {
            string[] audioOptions = { "Audio 1", "Audio 2" };
            string selectedAudio = await Application.Current.MainPage.DisplayActionSheet("Select Audio", "Cancel", null, audioOptions);

            string audioName = "";
            if (selectedAudio == "Audio 1")
            {
                audioName = "oddball_sequence_6_minutes_10_percent.wav";
            }
            else if (selectedAudio == "Audio 2")
            {
                audioName = "oddball_sequence_6_minutes_20%.wav";
            }

            NavigationParameters navigationParameters = new NavigationParameters();
            navigationParameters.Add("AudioName", audioName);
            await _navigationService.NavigateAsync(nameof(AudioRecognitionView), navigationParameters);
        }
    }
}
