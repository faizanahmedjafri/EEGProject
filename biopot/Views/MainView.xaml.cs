using Xamarin.Forms;
using biopot.ViewModels;
using biopot.Enums;
using biopot.Extensions;
using SharedCore.Services;

namespace biopot.Views
{
    public partial class MainView : BaseContentPage
    {
        private ILogService _logService;

        public MainView()
        {
            InitializeComponent();

            _logService = App.Resolve<ILogService>();

            this.entryFileName.Focused += EntryFileName_Focused;
            this.entryFolderName.Focused += EntryFolderName_Focused;

            this.switchDateTime.Toggled += SwitchDateTime_Toggled;
            this.switchTimeInName.Toggled += SwitchTimeInName_Toggled;
        }

        #region -- Overrides --

        protected override bool OnBackButtonPressed()
        {
            if (BindingContext is MainViewModel viewModel)
            {
                viewModel.BackCommand?.ExecuteIfCan();
            }

            return true;
        }

        #endregion

        #region -- Private helpers --

        private void SwitchTimeInName_Toggled(object sender, ToggledEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT} : User Switched Time in Name. Value = {(sender as Switch).IsToggled}");
        }

        private void SwitchDateTime_Toggled(object sender, ToggledEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: User Switched DateTime. Value = {(sender as Switch).IsToggled}");
        }

        private void EntryFolderName_Focused(object sender, FocusEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: User tapped on entry Folder Name");
        }

        private void EntryFileName_Focused(object sender, FocusEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: User tapped on entry File Name");
        }

        private void EntryId_Focused(object sender, FocusEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: User tapped on entry Id");
        }

        private void EntryName_Focused(object sender, FocusEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: User tapped on entry Name");
        }

        private void EntryPhone_Focused(object sender, FocusEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: User tapped on entry Phone");
        }

        private void EntryEmail_Focused(object sender, FocusEventArgs e)
        {
            _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: User tapped on entry Email");
        }

        #endregion
    }
}
