using System;
using System.ComponentModel;
using System.Windows.Input;
using biopot.Controls;
using biopot.Extensions;
using biopot.ViewModels;

namespace biopot.Views
{
    public partial class SetupView : BaseContentPage
    {
        public SetupView()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        protected override void OnAppearing()
        {
            base.OnAppearing();
            TabStrip.PropertyChanged += TabStripOnPropertyChanged;
        }

        /// <inheritdoc />
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            TabStrip.PropertyChanged -= TabStripOnPropertyChanged;
        }

        /// <inheritdoc />
        protected override bool OnBackButtonPressed()
        {
            if (BindingContext is SetupViewModel viewModel)
            {
                viewModel.BackCommand?.ExecuteIfCan();

                return true;
            }

            return base.OnBackButtonPressed();
        }

        /// <summary>
        /// Handles changed properties of <see cref="TabStrip"/> control. 
        /// </summary>
        /// <param name="aSender"> The event sender. </param>
        /// <param name="aArgs"> The event args. </param>
        private void TabStripOnPropertyChanged(object aSender, PropertyChangedEventArgs aArgs)
        {
            if (aArgs.PropertyName == TabStrip.ActivePageIndexProperty.PropertyName &&
                BindingContext is SetupViewModel viewModel)
            {
                var control = (TabStrip) aSender;
                viewModel.IsAdditionalSettingsActiveTab = control.ActivePageIndex == 0;
            }
        }
    }
}
