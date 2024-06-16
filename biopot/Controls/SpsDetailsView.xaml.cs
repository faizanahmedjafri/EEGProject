using System.Windows.Input;
using Xamarin.Forms;

namespace biopot.Controls
{
	public partial class SpsDetailsView : ContentView
	{
		public SpsDetailsView ()
		{
			InitializeComponent ();
		}

	    public static readonly BindableProperty CloseSpsDetailsCommandProperty = BindableProperty.Create
	        (nameof(CloseSpsDetailsCommand), typeof(ICommand), typeof(SpsDetailsView), default(ICommand));

	    public ICommand CloseSpsDetailsCommand
        {
	        get => (ICommand)GetValue(CloseSpsDetailsCommandProperty);
	        set => SetValue(CloseSpsDetailsCommandProperty, value);
	    }
	}
}