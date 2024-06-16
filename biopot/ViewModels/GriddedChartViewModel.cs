using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using biopot.Enums;
using biopot.Extensions;
using biopot.Services.Charts;
using Prism.Commands;

namespace biopot.ViewModels
{
	public class GriddedChartViewModel : BaseViewModel
	{
        private readonly IChartManagerService _chartsService;
        private int _currentChartIndex;

        public GriddedChartViewModel(IChartManagerService chartsService)
        {
            _chartsService = chartsService;

            _ChartViewModel = new ChartViewModel(_chartsService, EDeviceType.EEGorEMG, 1)
            {
                ViewportHalfY = Constants.Charts.ChartYAxisDefaultValue,
            };
            _currentChartIndex = 0;

            ResetChartScaleCommand = new DelegateCommand(ResetScaleToDefaultValues);
        }

        public EDeviceType DeviceType
        {
            get => _ChartViewModel.DeviceType;
            set => _ChartViewModel.DeviceType = value;
        }

        public string Name
        {
            get { return this._ChartViewModel.DeviceType.GetName(); }
        }

        private ChartViewModel _ChartViewModel;
        public ChartViewModel ChartViewModel
        {
            get { return _ChartViewModel; }
            set { SetProperty(ref _ChartViewModel, value); }
        }

	    private int _PickerValue;
        public int PickerValue
        {
            get { return _PickerValue; }
            set 
            {
                if (SetProperty(ref _PickerValue, value))
                {
                    ChartViewModel.ViewportHalfY = _PickerValue;
                }
            }
        }

        private bool _HasChannels;
        public bool HasChannels
        {
            get { return _HasChannels; }
            set { SetProperty(ref _HasChannels, value); }
        }

        private List<int> _FilteredList = new List<int>();
        public List<int> FilteredList
        {
            get => _FilteredList;
            set 
            {
                if (value.Count == 0)
                {
                    HasChannels = false;
                    return;
                }

                SetProperty(ref _FilteredList, value);
                HasChannels = true;

                var channels = FilteredList.Where(id => id != ChartViewModel.ChartId);
                var referenceChannelId = ChartViewModel.MontageReferenceValue;
                if (ChartViewModel.DeviceType == EDeviceType.EEGorEMG)
                {
                    referenceChannelId = _chartsService.RegisteredMontageChannels
                        .Where(pair => pair.ChannelId == ChartViewModel.ChartId)
                        .Select(x => x.ReferenceChannelId)
                        .DefaultIfEmpty(ChartViewModel.MontageReferenceValue)
                        .First();
                }

                UpdateChartViewModel(_FilteredList[_currentChartIndex], channels, referenceChannelId);
                RaisePropertyChanged(nameof(CurrentChart));
            }
        }

        public int CurrentChart
        {
            get => FilteredList.Count == 0 ? 0 : ChartViewModel.ChartId;
            set 
            {
                if (FilteredList.Count == 0)
                    return;

                _currentChartIndex = FilteredList.FindIndex(x=>x==value);
                var channels = FilteredList.Where(id => id != value);

                var referenceChannelId = ChartViewModel.MontageReferenceValue;
                if (ChartViewModel.DeviceType == EDeviceType.EEGorEMG)
                {
                    referenceChannelId = _chartsService.RegisteredMontageChannels
                        .Where(pair => pair.ChannelId == value)
                        .Select(x => x.ReferenceChannelId)
                        .DefaultIfEmpty(ChartViewModel.MontageReferenceValue)
                        .First();
                }

                UpdateChartViewModel(value, channels, referenceChannelId);
            }
        }

	    private DelegateCommand<string> _CurrentChartChangedCommand;

		public DelegateCommand<string> CurrentChartChangedCommand =>
		_CurrentChartChangedCommand ?? (_CurrentChartChangedCommand = new DelegateCommand<string>(OnCurrentChartChangedCommandExecute));

        private void OnCurrentChartChangedCommandExecute(string obj)
        {
            if (FilteredList.Count == 0)
            {
                return;
            }

            var chartId = 0;
            if (obj == "next" && _currentChartIndex != FilteredList.Count-1)
            {
                chartId = FilteredList[++_currentChartIndex];
            }
            else if (obj == "prev" && _currentChartIndex != 0)
            {
                chartId = FilteredList[--_currentChartIndex];
            }

            if (chartId != 0)
            {
                var channels = FilteredList.Where(id => id != ChartViewModel.ChartId);

                var referenceChannelId = ChartViewModel.MontageReferenceValue;
                if (ChartViewModel.DeviceType == EDeviceType.EEGorEMG)
                {
                    referenceChannelId = _chartsService.RegisteredMontageChannels
                        .Where(pair => pair.ChannelId == chartId)
                        .Select(x => x.ReferenceChannelId)
                        .DefaultIfEmpty(ChartViewModel.MontageReferenceValue)
                        .First();
                }

                UpdateChartViewModel(chartId, channels, referenceChannelId);
                RaisePropertyChanged(nameof(CurrentChart));
            }
        }

        /// <summary>
        /// Updates some field of chart view model.
        /// </summary>
        /// <param name="aChartId"> The chart id. </param>
        /// <param name="aChannelIds"> The list of channel ids except the current channel. </param>
        /// <param name="aMontageChannelId"> The reference channel id. </param>
	    private void UpdateChartViewModel(int aChartId, IEnumerable<int> aChannelIds, int aMontageChannelId)
	    {
	        ChartViewModel.ChartId = aChartId;
	        ChartViewModel.ChannelIds = aChannelIds;
	        ChartViewModel.MontageChannel = aMontageChannelId;
        }

        private int _MinValue = Constants.Charts.ChartYAxisMinValue;
        public int MinValue
        {
            get => _MinValue;
            set => SetProperty(ref _MinValue, value);
        }
        private int _MaxValue = Constants.Charts.ChartYAxisMaxValue;
        public int MaxValue
        {
            get => _MaxValue;
            set => SetProperty(ref _MaxValue, value);
        }

        /// <summary>
        /// Gets a command to reset chart scales, pressed for particular device type.
        /// </summary>
        public ICommand ResetChartScaleCommand { get; }

        private int iXScaleValue;
        public int XScaleValue
        {
            get => iXScaleValue;
            set
            {
                if (SetProperty(ref iXScaleValue, value))
                {
                    // actual baseline is 0, while the min value can be larger.
                    ChartViewModel.ViewportX = XScaleValue / (float) (XScaleMaxValue - 0);
                }
            }
        }

        private int iXScaleMaxValue = Constants.Charts.ChartXAxisMaxValue;
        public int XScaleMaxValue
        {
            get => iXScaleMaxValue;
            set => SetProperty(ref iXScaleMaxValue, value);
        }

        private int iXScaleMinValue = Constants.Charts.ChartXAxisMinValue;
        public int XScaleMinValue
        {
            get => iXScaleMinValue;
            set => SetProperty(ref iXScaleMinValue, value);
        }

        #region Private Methods

        /// <summary>
        /// Resets scales for the chart.
        /// </summary>
        private void ResetScaleToDefaultValues()
        {
            // X-axis is reset
            XScaleValue = Constants.Charts.ChartXAxisDefaultValue;

            // Y-axis is reset
            PickerValue = Constants.Charts.ChartYAxisDefaultValue;
        }

        #endregion
    }
}
