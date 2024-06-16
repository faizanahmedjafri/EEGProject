using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using biopot.ViewModels;
using Xamarin.Forms;

namespace biopot.Views
{
	public partial class ChartsView : BaseContentPage
	{
	    public ChartsViewModel ViewModel { get; protected set; }

	    private CancellationTokenSource _batteryAnimationCancellationTokenSource;
	    private Task _batteryImageAnimationTask;

        public ChartsView()
		{
			InitializeComponent();

            SetXAxisValue(chartX, ViewModel.XScaleValue);
            OnBatteryLevelChanged(ViewModel.BatteryLevel);
        }

        /// <inheritdoc/>
	    protected override void OnBindingContextChanged()
	    {
	        base.OnBindingContextChanged();

	        if (ViewModel != null)
	        {
	            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
	        }

	        ViewModel = (ChartsViewModel) BindingContext;
	        if (ViewModel != null)
	        {
	            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
	        }
	    }

        /// <summary>
        /// Handles properties change event of view model.
        /// </summary>
        /// <param name="aSender"> The event sender. </param>
        /// <param name="aArgs"> The event arguments. </param>
	    private void OnViewModelPropertyChanged(object aSender, PropertyChangedEventArgs aArgs)
	    {
	        if (!IsVisible)
	        {
	            return;
	        }

	        if (aArgs.PropertyName == nameof(ChartsViewModel.XScaleValue))
	        {
	            var chartsViewModel = (ChartsViewModel) aSender;
                UpdateXAxisValue(chartX, chartsViewModel.XScaleValue);
            }

	        if (aArgs.PropertyName == nameof(ChartsViewModel.BatteryLevel))
	        {
	            var chartsViewModel = (ChartsViewModel)aSender;
	            OnBatteryLevelChanged(chartsViewModel.BatteryLevel);
	        }
	        if (aArgs.PropertyName == nameof(ChartsViewModel.IsMenuOpened))
	        {
	            var chartsViewModel = (ChartsViewModel)aSender;
	            OnMenuStateChanged(chartsViewModel.IsMenuOpened);
	        }
        }

        /// <summary>
        /// Handles menu state changes.
        /// </summary>
        /// <param name="aIsMenuOpened"> True - menu is opened, otherwise - false. </param>
	    private void OnMenuStateChanged(bool aIsMenuOpened)
	    {
            Grid.SetRowSpan(TopMenu, aIsMenuOpened ? 3 : 1);
	    }

        /// <summary>
        /// Handles changed battery level: starts or stops the animation.
        /// </summary>
        /// <param name="aBatteryLevel"> The battery level. </param>
	    private void OnBatteryLevelChanged(int aBatteryLevel)
        {
            try
            {
                if (_batteryImageAnimationTask == null)
                {
                    if (aBatteryLevel < 15)
                    {
                        // start animation
                        _batteryAnimationCancellationTokenSource = new CancellationTokenSource();
                        _batteryImageAnimationTask = StartAnimateViewAsync(BatteryImage, _batteryAnimationCancellationTokenSource);
                    }
                }
                else
                {
                    if (aBatteryLevel >= 15)
                    {
                        // stop animation
                        _batteryAnimationCancellationTokenSource.Cancel();
                        _batteryAnimationCancellationTokenSource = null;

                        _batteryImageAnimationTask = null;

                        BatteryImage.Opacity = 1;

                    }
                }
            }
            catch (OperationCanceledException)
            {
                // nothing to do, ignored
            }
        }

        /// <summary>
        /// Starts continious animation until cancellation is requested.
        /// </summary>
        /// <param name="aView"> The view to animate. </param>
        /// <param name="aTokenSource"> The canccelation token source. </param>
        /// <returns></returns>
	    private async Task StartAnimateViewAsync(View aView, CancellationTokenSource aTokenSource)
	    {
	        while (!aTokenSource.IsCancellationRequested)
	        {
	            await aView.FadeTo(0, 350, Easing.SpringIn);
	            await aView.FadeTo(1, 350, Easing.SpringIn);
	        }
        }

	    /// <summary>
        /// Sets the initial grid with for X axis.
        /// </summary>
        /// <param name="aXAxisGrid"> The X axis grid. </param>
        /// <param name="aMaxValue"> The max X axis value. </param>
	    private void SetXAxisValue(Grid aXAxisGrid, int aMaxValue)
	    {
	        int cols = 7;

	        for (int i = 0; i < cols; i++)
	        {
	            if (i == 0 || i == 6)
	            {
	                aXAxisGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.05, GridUnitType.Star) });
	                continue;
	            }

	            aXAxisGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.16, GridUnitType.Star) });
	            Label label = new Label();

	            label.VerticalOptions = LayoutOptions.Start;
	            label.HorizontalOptions = LayoutOptions.Center;

	            aXAxisGrid.Children.Add(label);
	            Grid.SetColumn(label, i);

	        }

            UpdateXAxisValue(aXAxisGrid, aMaxValue);
        }

	    /// <summary>
	    /// Updates values on X axis with given max value, which corresponds to range [0..max].
	    /// </summary>
	    /// <param name="aXAxisGrid">The grid representing X-axis with child labels.</param>
	    /// <param name="aMaxValue">The maximum value for X-axis in seconds.</param>
        private void UpdateXAxisValue(Grid aXAxisGrid, int aMaxValue)
	    {
	        IReadOnlyList<Label> labelValues = aXAxisGrid.Children
	            .Where(x => x is Label)
	            .Cast<Label>()
	            .OrderBy(Grid.GetColumn)
	            .ToArray();

            // 2 values (extra left and rigth) are not shown 
            // but they should be taken into account during calculation.
	        var labelsCount = labelValues.Count + 2;
	        var format = (aMaxValue < labelsCount) ? "{0:F1}s" : "{0:F0}s";
	        float step = (aMaxValue - 0) / (float)labelsCount;
	        for (int i = 1; i <= labelValues.Count; i++)
	        {
	            var value = (aMaxValue - i * step);
	            labelValues[i-1].Text = string.Format(format, value);
	        }
        }
	}
}
