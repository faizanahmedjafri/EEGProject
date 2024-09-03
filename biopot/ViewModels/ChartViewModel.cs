using System;
using System.Collections.Generic;
using System.Linq;
using biopot.Enums;
using biopot.Services.Charts;
using Microcharts;
using SkiaSharp;
using System.Diagnostics;
using System.Windows.Input;
using biopot.Controls;
using Prism.Commands;
using SharedCore.Services.Performance;
using Xamarin.Forms;
using Prism.Navigation;

namespace biopot.ViewModels
{
    public class ChartViewModel : BaseViewModel
    {
        public const int MontageReferenceValue = 0;
        private readonly IChartManagerService _chartsService;
        private readonly INavigationService _navigationService;

        public ChartViewModel(IChartManagerService chartsService, EDeviceType deviceType,
            int chartId, INavigationService navigationService, IEnumerable<int> aChannelIds = null)
        {
            _chartsService = chartsService;
            _navigationService = navigationService;
            ChartId = chartId;
            ChannelIds = aChannelIds?.Where(id => id != ChartId).ToList();
            _deviceType = deviceType;
            _chartsService.DataLoaded += this.DataUpdater;
            ApplyMontageChannelCommand = new DelegateCommand<View>(OnApplyMontageChannelCommand).ObservesCanExecute(() => IsMontageChannelChanged);
        }

        public void Unsubscribe()
        {
            _chartsService.DataLoaded -= this.DataUpdater;
        }

        public void DataUpdater(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Performance.Start(out var reference, "CreateChart");
                int maxValue = ViewportHalfY;
                int minValue = -ViewportHalfY;

                if (_deviceType == EDeviceType.SPSValue)
                {
                    maxValue = 1000;
                    minValue = 0;
                }
                var actualData = this._chartsService.GetActualData(this._deviceType, this._ChartId).ToArray();
                SensorValue = (short)actualData.LastOrDefault();
                this.Chart = new CustomLineChart
                {
                    Color = _ChartColor,

                    Values = GetXViewportValues(actualData),
                    LineMode = LineMode.Straight,
                    LineSize = 1.6F,
                    PointMode = PointMode.None,
                    BackgroundColor = SKColors.Transparent,
                    LineAreaAlpha = 0,
                    PointAreaAlpha = 0,
                    MaxValue = maxValue,
                    MinValue = minValue,
                    Margin = 0
                };
                Performance.Stop(reference, "CreateChart");
            });
        }

        private EDeviceType _deviceType;
        public EDeviceType DeviceType
        {
            set
            {
                SetProperty(ref _deviceType, value);
                RaisePropertyChanged(nameof(IsEEGChannel));
            }
            get => _deviceType;
        }

        public bool IsEEGChannel => DeviceType == EDeviceType.EEGorEMG;

        private int _ChartId;
        public int ChartId
        {
            get => _ChartId;
            set
            {
                SetProperty(ref _ChartId, value);

                RaisePropertyChanged(nameof(IsMontageApplied));
                RaisePropertyChanged(nameof(ChannelWithMontageFormatted));
            }
        }

        private IEnumerable<int> _channelIds;
        public IEnumerable<int> ChannelIds
        {
            get => _channelIds;
            set
            {
                SetProperty(ref _channelIds, value);
                RaisePropertyChanged(nameof(MontageChannelsFormatted));
            }
        }

        /// <summary>
        /// The list of channels to montage.
        /// </summary>
        /// FIXME move strings to resources.
        public List<string> MontageChannelsFormatted
        {
            get
            {
                var channelsNames = new List<string> { "Reference" };
                if (ChannelIds != null)
                {
                    foreach (var channelId in ChannelIds)
                    {
                        channelsNames.Add($"Ch. {channelId}");
                    }
                }

                return channelsNames;
            }
        }

        private int _selectedMontageIndex;
        public int SelectedMontageIndex
        {
            get => _selectedMontageIndex;
            set
            {
                SetProperty(ref _selectedMontageIndex, value);
                SelectedMontageChannel = ChannelIds == null || value < 1
                    ? MontageReferenceValue : ChannelIds.ElementAt(value - 1);

            }
        }

        /// <summary>
        /// True - selected montage channel differs from the applied one, otherwise - false.
        /// </summary>
        public bool IsMontageChannelChanged => SelectedMontageChannel != MontageChannel;

        /// <summary>
        /// Selected but not yet applied montage channel
        /// </summary>
        private int _selectedMontageChannel;
        public int SelectedMontageChannel
        {
            get => _selectedMontageChannel;
            set
            {
                SetProperty(ref _selectedMontageChannel, value);
                RaisePropertyChanged(nameof(IsMontageChannelChanged));
            }
        }

        /// <summary>
        /// Applied montage channel.
        /// </summary>
        private int _montageChannel = MontageReferenceValue;
        public int MontageChannel
        {
            get => _montageChannel;
            set
            {
                SetProperty(ref _montageChannel, value);
                SelectedMontageChannel = value;
                RaisePropertyChanged(nameof(IsMontageApplied));
                RaisePropertyChanged(nameof(MontageChannelFormatted));
                RaisePropertyChanged(nameof(ChannelWithMontageFormatted));
                RaisePropertyChanged(nameof(IsMontageChannelChanged));
            }
        }

        /// <summary>
        /// Applied montage channel formatted: '(MontageId)'.
        /// </summary>
	    public string MontageChannelFormatted => $"({MontageChannel})";

        /// <summary>
        /// Current channel with applied montage channel: 'ChannelId(MontageId)'.
        /// </summary>
	    public string ChannelWithMontageFormatted => IsMontageApplied
            ? $"{ChartId}{MontageChannelFormatted}" : $"{ChartId}";

        public bool IsMontageApplied => MontageChannel > MontageReferenceValue && MontageChannel != ChartId;

        public int ViewportHalfY { get; set; }

        private float iViewportX = 1.0f;
        /// <summary>
        /// Gets/sets the width of the viewport on X-axis in range [0.0..1.0], aligned to the right.
        /// </summary>
        public float ViewportX
        {
            get => iViewportX;
            set => iViewportX = Math.Max(Math.Min(value, 1.0f), 0.0f);
        }

        private short _SensorValue;
        public short SensorValue
        {
            get { return _SensorValue; }
            set { SetProperty(ref _SensorValue, value); }
        }

        private Chart _Chart;
        public Chart Chart
        {
            get => _Chart;
            set => SetProperty(ref _Chart, value);
        }

        private SkiaSharp.SKColor _ChartColor = SkiaSharp.SKColors.Black;
        public SkiaSharp.SKColor ChartColor
        {
            get { return _ChartColor; }
            set { SetProperty(ref _ChartColor, value); }
        }

        private ICommand _closeMontageChannelCommand;
        public ICommand CloseMontageChannelCommand => _closeMontageChannelCommand ?? (_closeMontageChannelCommand = new Command<View>(OnCloseMontageForItemCommand));

        public ICommand ApplyMontageChannelCommand { get; }

        /// <summary>
        /// Handles applying montage for specific channel.
        /// </summary>
        /// <param name="aView"> The child of swipped view. </param>
        private void OnApplyMontageChannelCommand(View aView = null)
        {
            MontageChannel = SelectedMontageChannel;

            if (IsMontageApplied)
            {
                _chartsService.RegisterMontageChannel(
                    new MontageChannelPair(ChartId, MontageChannel));
            }
            else
            {
                _chartsService.UnregisterMontageChannel(ChartId);
            }

            if (aView != null)
            {
                var swippedView = (SwipableListItem)aView.Parent;
                swippedView.ShowInitialView();
            }
        }

        /// <summary>
        /// Handles closing of montage for specific channel.
        /// </summary>
        /// <param name="aView"> The child of swipped view. </param>
        private void OnCloseMontageForItemCommand(View aView = null)
        {
            if (aView != null)
            {
                var swippedView = (SwipableListItem)aView.Parent;
                swippedView.ShowInitialView();
            }
        }

        /// <summary>
        /// Gets the values matching the selected X viewport.
        /// </summary>
        private IReadOnlyList<double> GetXViewportValues(double[] aValues)
        {
            int length = (int)(aValues.Length * ViewportX);
            int offset = aValues.Length - length;

            return new ArraySegment<double>(aValues, offset, length);
        }
    }

    public class CustomLineChart : PointChart
    {
        #region Constructors

        public CustomLineChart()
        {
            this.PointSize = 10;
        }

        #endregion

        #region Properties

        public SkiaSharp.SKColor Color { get; set; }

        public IReadOnlyList<double> Values { get; set; }

        /// <summary>
        /// Gets or sets the size of the line.
        /// </summary>
        /// <value>The size of the line.</value>
        public float LineSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets the line mode.
        /// </summary>
        /// <value>The line mode.</value>
        public LineMode LineMode { get; set; } = LineMode.Spline;

        /// <summary>
        /// Gets or sets the alpha of the line area.
        /// </summary>
        /// <value>The line area alpha.</value>
        public byte LineAreaAlpha { get; set; } = 32;

        #endregion

        #region Methods

        public new float MaxValue { get; set; }
        public new float MinValue { get; set; }

        private float ValueRange => MaxValue - MinValue;

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            Performance.Start(out var reference);

            var total = Values.Count;
            var w = (width - ((total + 1) * Margin)) / total;
            var h = height - Margin;
            var itemSize = new SKSize(w, h);

            var points = new SKPoint[total];
            for (int i = 0; i < total; i++)
            {
                points[i].X = (float)(Margin + itemSize.Width / 2.0 + i * ((double)itemSize.Width + Margin));
                points[i].Y = 0 + (MaxValue - (float)Values[i]) / ValueRange * itemSize.Height;
            }

            this.DrawLine(canvas, points, itemSize);

            Performance.Stop(reference);
        }

        protected void DrawLine(SKCanvas canvas, SKPoint[] points, SKSize itemSize)
        {
            Performance.Start(out var reference);

            if (points.Length > 1 && this.LineMode != LineMode.None)
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = Color,
                    StrokeWidth = this.LineSize,
                    IsAntialias = false,
                })
                {
                    var path = new SKPath();

                    path.MoveTo(points.First());

                    var last = (this.LineMode == LineMode.Spline) ? points.Length - 1 : points.Length;
                    for (int i = 0; i < last; i++)
                    {
                        if (this.LineMode == LineMode.Straight)
                        {
                            path.LineTo(points[i]);
                        }
                    }

                    canvas.DrawPath(path, paint);
                }
            }

            Performance.Stop(reference);
        }

        #endregion
    }

}