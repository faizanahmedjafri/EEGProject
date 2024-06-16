using System;
using Prism.Mvvm;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace biopot.Views
{
	public class BaseContentPage : ContentPage
	{
		public BaseContentPage()
		{
			//this.Padding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0);

			if (Device.Idiom == TargetIdiom.Phone)
			{
				if (Xamarin.Forms.Application.Current.MainPage?.Height > 811)
				{
					On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
				}
			}

			ViewModelLocator.SetAutowireViewModel(this, true);
			Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

			BackgroundColor = Color.White;
		}

		#region -- Overrides --

		protected override void OnAppearing()
		{
			base.OnAppearing();
			var actionsHandler = BindingContext as IViewActionsHandler;

			if (actionsHandler != null)
			{
				actionsHandler.OnAppearing();
			}
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			var actionsHandler = BindingContext as IViewActionsHandler;
			if (actionsHandler != null)
			{
				actionsHandler.OnDisappearing();
			}
		}
        
		#endregion
	}
}

