using biopot.Extensions;
using biopot.ViewModels;

namespace biopot.Views
{
	public partial class TechnicalSettingsView : BaseContentPage
	{
		public TechnicalSettingsView ()
		{
			InitializeComponent ();
		}

	    /// <inheritdoc />
	    protected override bool OnBackButtonPressed()
	    {
	        if (BindingContext is TechnicalSettingsViewModel viewModel)
	        {
	            viewModel.BackCommand?.ExecuteIfCan();

	            return true;
	        }

	        return base.OnBackButtonPressed();
	    }
    }
}