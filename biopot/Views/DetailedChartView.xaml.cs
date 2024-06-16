using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using biopot.ViewModels;
using Xamarin.Forms;
using biopot.Controls;
using Prism.Navigation;

namespace biopot.Views
{
	public partial class DetailedChartView : BaseContentPage, IDestructible
	{
	    public DetailedChartViewModel ViewModel { get; protected set; }

        private const int GridCols = 7;
        private const int GridRows = 10;

	    private CancellationTokenSource _batteryAnimationCancellationTokenSource;
	    private Task _eegEmgBatteryImageAnimationTask;
	    private Task _bioImpedanceBatteryImageAnimationTask;
	    private Task _accelerometerBatteryImageAnimationTask;

        public DetailedChartView()
		{
			InitializeComponent();
			CreateScale(this.chartX_EEG, this.chartY_EEG, this.chartGrid_EEG, GridCols, GridRows);
			CreateScale(this.chartX_Bio, this.chartY_Bio, this.chartGrid_Bio, GridCols, GridRows);
			CreateScale(this.chartX_Acc, this.chartY_Acc, this.chartGrid_Acc, GridCols, GridRows);

			EEMorEMGValuePicker.PropertyChanged += EEMorEMGValuePickerPropertyChanged;
			bioImpedanceValuePicker.PropertyChanged += BioImpedanceValuePickerPropertyChanged;
			accelerometerValuePicker.PropertyChanged += AccelerometerValuePickerPropertyChanged;

            eemOrEegXScalePicker.PropertyChanged += OnXScaleChanged;
            accelerometerXScalePicker.PropertyChanged += OnXScaleChanged;
            bioImpedanceXScalePicker.PropertyChanged += OnXScaleChanged;

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

	        ViewModel = (DetailedChartViewModel)BindingContext;
	        if (ViewModel != null)
	        {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
	        }
	    }

        public void Destroy()
		{
			EEMorEMGValuePicker.PropertyChanged -= EEMorEMGValuePickerPropertyChanged;
			bioImpedanceValuePicker.PropertyChanged -= BioImpedanceValuePickerPropertyChanged;
			accelerometerValuePicker.PropertyChanged -= AccelerometerValuePickerPropertyChanged;
            eemOrEegXScalePicker.PropertyChanged -= OnXScaleChanged;
            accelerometerXScalePicker.PropertyChanged -= OnXScaleChanged;
            bioImpedanceXScalePicker.PropertyChanged -= OnXScaleChanged;
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

	        if (aArgs.PropertyName == nameof(DetailedChartViewModel.BatteryLevel))
	        {
                var detailedChartViewModel = (DetailedChartViewModel)aSender;
                OnBatteryLevelChanged(detailedChartViewModel.BatteryLevel);
	        }
	    }

        /// <summary>
        /// Handles changed battery level: starts or stops the animation.
        /// </summary>
        /// <param name="aBatteryLevel"> The battery level. </param>
        private void OnBatteryLevelChanged(int aBatteryLevel)
        {
            try
            {
                if (_eegEmgBatteryImageAnimationTask == null
                    && _accelerometerBatteryImageAnimationTask == null
                    && _bioImpedanceBatteryImageAnimationTask == null)
                {
                    if (aBatteryLevel < 15)
                    {
                        // start animation
                        _batteryAnimationCancellationTokenSource = new CancellationTokenSource();
                        _eegEmgBatteryImageAnimationTask = StartAnimateViewAsync(EegEmgBatteryImage, _batteryAnimationCancellationTokenSource);
                        _bioImpedanceBatteryImageAnimationTask = StartAnimateViewAsync(BioImpedanceBatteryImage, _batteryAnimationCancellationTokenSource);
                        _accelerometerBatteryImageAnimationTask = StartAnimateViewAsync(AccelerometerBatteryImage, _batteryAnimationCancellationTokenSource);
                    }
                }
                else
                {
                    if (aBatteryLevel >= 15)
                    {
                        // stop animation
                        _batteryAnimationCancellationTokenSource.Cancel();
                        _batteryAnimationCancellationTokenSource = null;

                        _eegEmgBatteryImageAnimationTask = null;
                        _accelerometerBatteryImageAnimationTask = null;
                        _bioImpedanceBatteryImageAnimationTask = null;

                        EegEmgBatteryImage.Opacity = 1;
                        BioImpedanceBatteryImage.Opacity = 1;
                        AccelerometerBatteryImage.Opacity = 1;
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

        private void CreateScale(Grid chartX, Grid chartY, Grid chartGrid, int cols, int rows)
		{
			for (int i = 0; i < cols; i++)
			{
				Label label = new Label();
				label.VerticalOptions = LayoutOptions.Start;

				Thickness margin = label.Margin;
				margin.Left = -10;
				label.Margin = margin;

				chartX.Children.Add(label);
				Grid.SetColumn(label, i);
			}

			for (int i = 0; i < rows; i++)
			{
				// rows               
				Label label = new Label();

				label.HorizontalOptions = LayoutOptions.End;
				label.VerticalOptions = LayoutOptions.End;
				Thickness margin = label.Margin;
				margin.Bottom = -10;
				label.Margin = margin;

				chartY.Children.Add(label);
				Grid.SetRow(label, i);

				//cells
				for (int j = 0; j < cols; j++)
				{
					// Chart grid
					BoxView boxView = new BoxView();
					boxView.BackgroundColor = Xamarin.Forms.Color.White;
					chartGrid.Children.Add(boxView);
					Grid.SetRow(boxView, i);
					Grid.SetColumn(boxView, j);
				}
			}
		}

		protected override bool OnBackButtonPressed()
		{
			((DetailedChartViewModel)BindingContext).OnBackCommand();
			return base.OnBackButtonPressed();
		}

        private void SetNewMvValue(Grid aYAxisGrid, int aYHalfScale)
        {
            IReadOnlyList<Label> allLabels = aYAxisGrid.Children
                .Where(x => x is Label)
                .Cast<Label>()
                .OrderBy(Grid.GetColumn)
                .ToArray();

            var halfLabels = allLabels.Count / 2 - 1;
            var step = aYHalfScale / (float) (halfLabels + 1);
            var format = (step < 1.0f) ? "{0:F1}" : "{0:F0}";

            // positive (higher half) labels
            var _ = allLabels
                .Take(halfLabels)
                .Select((aLabel, aIndex) =>
                {
                    var value = step * (halfLabels - aIndex);
                    aLabel.Text = string.Format(format, value);
                    return aLabel;
                }).Count();

            // zero label
            var zeroLabel = allLabels
                .Skip(halfLabels)
                .First();
            zeroLabel.Text = string.Format(format, 0.0f);

            // negative (lower half) labels
            _ = allLabels
                .Skip(halfLabels + 1)
                .Take(halfLabels)
                .Select((aLabel, aIndex) =>
                {
                    var value = -step * (aIndex + 1);
                    aLabel.Text = string.Format(format, value);
                    return aLabel;
                }).Count();
        }

        /// <summary>
        /// Updates values on X axis with given max value, which corresponds to range [0..max].
        /// </summary>
        /// <param name="aXAxisGrid">The grid representing X-axis with child labels.</param>
        /// <param name="aMaxValue">The maximum value for X-axis in seconds.</param>
        private void UpdateXAxisValues(Grid aXAxisGrid, int aMaxValue)
        {
            IReadOnlyList<Label> labelValues = aXAxisGrid.Children
                .Where(x => x is Label)
                .Cast<Label>()
                .OrderBy(Grid.GetColumn)
                .ToArray();

            var format = (aMaxValue < labelValues.Count) ? "{0:F1}s" : "{0:F0}s";
            float step = (aMaxValue - 0) / (float) labelValues.Count;
            for (int i = 0; i < labelValues.Count; i++)
            {
                var value = (aMaxValue - i * step);
                labelValues[i].Text = string.Format(format, value);
            }
        }

        private void EEMorEMGValuePickerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BaseValuePicker.PickerValueProperty.PropertyName)
            {
                var view = (BaseValuePicker) sender;
                SetNewMvValue(this.chartY_EEG, view.PickerValue);
            }
        }

        private void BioImpedanceValuePickerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BaseValuePicker.PickerValueProperty.PropertyName)
            {
                var view = (BaseValuePicker) sender;
                SetNewMvValue(this.chartY_Bio, view.PickerValue);
            }
        }

        private void AccelerometerValuePickerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BaseValuePicker.PickerValueProperty.PropertyName)
            {
                var view = (BaseValuePicker) sender;
                SetNewMvValue(this.chartY_Acc, view.PickerValue);
            }
        }

        private void OnXScaleChanged(object aSender, PropertyChangedEventArgs aArgs)
        {
            if (aArgs.PropertyName == BaseValuePicker.PickerValueProperty.PropertyName)
            {
                var view = (BaseValuePicker) aSender;

                if (ReferenceEquals(aSender, eemOrEegXScalePicker))
                {
                    // redraw X axis values for EEG/EMG
                    UpdateXAxisValues(chartX_EEG, view.PickerValue);
                }
                else if (ReferenceEquals(aSender, accelerometerXScalePicker))
                {
                    // redraw X axis values for accelerometer
                    UpdateXAxisValues(chartX_Acc, view.PickerValue);
                }
                else if (ReferenceEquals(aSender, bioImpedanceXScalePicker))
                {
                    // redraw X axis values for bio-impedance
                    UpdateXAxisValues(chartX_Bio, view.PickerValue);
                }
            }
        }
    }
}
