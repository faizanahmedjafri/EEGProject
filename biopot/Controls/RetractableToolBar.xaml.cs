using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;
using biopot.Helpers;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using biopot.Extensions;
using biopot.ViewModels;
using System.Runtime.CompilerServices;
using biopot.Enums;
using biopot.Resources.Strings;
using Prism.Commands;
using SharedCore.Services.Charts;

namespace biopot.Controls
{
    public partial class RetractableToolBar : ContentView
    {
        private double _y = 0;

        private float _menuDistance = 0;
        private Rectangle screenBounds;

        private int _menuMoveDistance = 100;
        private int _menuDistanceForOpacityApply = 70;
        private double _overlayBoxOpasityPic = 0.4;

        private int _menuHeight;
        private int _menuContentHeight;

        private const int MENU_FOOTER = 90;
        private const int MENU_TABSTRIPBUTTON = 50;
        private const int MENU_CLOSE_STATE = 90;
        private const int MENU_BUTTONS = 80;

        private int _heightEEGEMGrid;
        private int _heightBioGrid;
        private int _heightAccelerGrid;

        private int _conutCheckboxInRow = 4; //4 - conut checkbox in row
        private int _heightSingleRow = 50; //50 - height single row

        public RetractableToolBar()
        {
            InitializeComponent();

            _menuContentHeight = 250;//135
            _menuHeight = MENU_FOOTER + MENU_TABSTRIPBUTTON + _menuContentHeight + 5; // 5 space for shadow icon

            omegaIconGrid.SizeChanged += OnSizeOmegaIconGridChanged;
            SPStack.SizeChanged += SPStackSizeChanged;

            // Set heights for menu items
            main.HeightRequest = MENU_CLOSE_STATE;
            whiteOverlay.HeightRequest = MENU_CLOSE_STATE;
            boxFooter.HeightRequest = MENU_FOOTER;
            menuButtons.HeightRequest = MENU_BUTTONS;
            box.HeightRequest = _menuHeight;

            // placed in code behind not to copy-past hardcoded sizes to xaml.
            AbsoluteLayout.SetLayoutBounds(box, new Rectangle(0, MENU_BUTTONS, 1, -1));
            AbsoluteLayout.SetLayoutFlags(box, AbsoluteLayoutFlags.WidthProportional);

            BioImpedanceFormatted = Strings.DeviceConfigurationBoImpedance;
        }


        #region -- Public properties --
        private bool _IsOpenState = false;
        public bool IsOpenState
        {
            get => _IsOpenState;
            private set
            {
                if (_IsOpenState != value)
                {
                    _IsOpenState = value;
                    OnPropertyChanged(nameof(IsOpenState));

                    IsMenuOpened = value;
                }
            }
        }

        private int _StripTabIndex;
        public int StripTabIndex
        {
            get { return _StripTabIndex; }
            set { _StripTabIndex = value; UpdateMenuContentHeight(_StripTabIndex); }
        }

        private bool _SelectAllEEGEMG;
        public bool SelectAllEEGEMG
        {
            get { return _SelectAllEEGEMG; }
            set { _SelectAllEEGEMG = value; OnPropertyChanged(nameof(SelectAllEEGEMG)); }
        }

        private bool _SelectAllBioImpedance;
        public bool SelectAllBioImpedance
        {
            get { return _SelectAllBioImpedance; }
            set { _SelectAllBioImpedance = value; OnPropertyChanged(nameof(SelectAllBioImpedance)); }
        }

        private bool _SelectAllAccelerometer;
        public bool SelectAllAccelerometer
        {
            get { return _SelectAllAccelerometer; }
            set { _SelectAllAccelerometer = value; OnPropertyChanged(nameof(SelectAllAccelerometer)); }
        }

        private string _deviceName;
        public string DeviceName
        {
            get => _deviceName;
            set { _deviceName = value; OnPropertyChanged(nameof(DeviceName)); }
        }

        private uint _channelsNumber;
        public uint ChannelsNumber
        {
            get => _channelsNumber;
            set { _channelsNumber = value; OnPropertyChanged(nameof(ChannelsNumber)); }
        }

        private bool _isBioImpedancePresent;
        public bool IsBioImpedancePresent
        {
            get => _isBioImpedancePresent;
            set { _isBioImpedancePresent = value; OnPropertyChanged(nameof(IsBioImpedancePresent)); }
        }

        private string _bioImpedanceFormatted;

        public string BioImpedanceFormatted
        {
            get => _bioImpedanceFormatted;
            set
            {
                _bioImpedanceFormatted = value;
                OnPropertyChanged(nameof(BioImpedanceFormatted));
            }
        }

        private bool _isAccelerometerPresent;
        public bool IsAccelerometerPresent
        {
            get => _isAccelerometerPresent;
            set { _isAccelerometerPresent = value; OnPropertyChanged(nameof(IsAccelerometerPresent)); }
        }

        private uint _accelerometerMode;
        
        public uint AccelerometerMode
        {
            get => _accelerometerMode;
            set { _accelerometerMode = value; OnPropertyChanged(nameof(AccelerometerMode)); }
        }

        private ICommand _openCloseMenuCommand;
        public ICommand OpenCloseMenuCommand => _openCloseMenuCommand ?? (_openCloseMenuCommand = new DelegateCommand(OnOpenCloseMenuCommand)
            .ObservesCanExecute(() => CanOpenMenu));

        public static readonly BindableProperty DevicesProperty = BindableProperty.Create
            (nameof(Devices), typeof(IList<CheckboxesGroupedByDevice>), typeof(RetractableToolBar), default(IList<CheckboxesGroupedByDevice>), BindingMode.TwoWay, propertyChanged: OnDevicesChanged);

        public IList<CheckboxesGroupedByDevice> Devices
        {
            get { return (IList<CheckboxesGroupedByDevice>)GetValue(DevicesProperty); }
            set { SetValue(DevicesProperty, value); }
        }

        public static readonly BindableProperty UpdateChartStateProperty = BindableProperty.Create
            (nameof(UpdateChartState), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand UpdateChartState
        {
            get { return (ICommand)GetValue(UpdateChartStateProperty); }
            set { SetValue(UpdateChartStateProperty, value); }
        }

        public static readonly BindableProperty ConnectionStateCommandProperty = BindableProperty.Create
            (nameof(ConnectionStateCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand ConnectionStateCommand
        {
            get { return (ICommand)GetValue(ConnectionStateCommandProperty); }
            set { SetValue(ConnectionStateCommandProperty, value); }
        }

        public static readonly BindableProperty BandWidthCommandProperty = BindableProperty.Create
            (nameof(BandWidthCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand BandWidthCommand
        {
            get { return (ICommand)GetValue(BandWidthCommandProperty); }
            set { SetValue(BandWidthCommandProperty, value); }
        }

        public static readonly BindableProperty OpenFolderCommandProperty = BindableProperty.Create
            (nameof(OpenFolderCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand OpenFolderCommand
        {
            get { return (ICommand)GetValue(OpenFolderCommandProperty); }
            set { SetValue(OpenFolderCommandProperty, value); }
        }

        public static readonly BindableProperty OpenSettingsCommandProperty = BindableProperty.Create
            (nameof(OpenSettingsCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand OpenSettingsCommand
        {
            get { return (ICommand)GetValue(OpenSettingsCommandProperty); }
            set { SetValue(OpenSettingsCommandProperty, value); }
        }

        public static readonly BindableProperty AdditionalSettingsCommandProperty = BindableProperty.Create
            (nameof(AdditionalSettingsCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand AdditionalSettingsCommand
        {
            get { return (ICommand)GetValue(AdditionalSettingsCommandProperty); }
            set { SetValue(AdditionalSettingsCommandProperty, value); }
        }

        public static readonly BindableProperty UserSettingsCommandProperty = BindableProperty.Create
           (nameof(UserSettingsCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand UserSettingsCommand
        {
            get { return (ICommand)GetValue(UserSettingsCommandProperty); }
            set { SetValue(UserSettingsCommandProperty, value); }
        }

        public static readonly BindableProperty TechnicalSettingsCommandProperty = BindableProperty.Create
            (nameof(TechnicalSettingsCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand TechnicalSettingsCommand
        {
            get => (ICommand)GetValue(TechnicalSettingsCommandProperty);
            set => SetValue(TechnicalSettingsCommandProperty, value);
        }

        public static readonly BindableProperty SensorsConnectionStateProperty = BindableProperty.Create
            (nameof(SensorsConnectionState), typeof(ESensorConnectionState), typeof(RetractableToolBar), default(ESensorConnectionState));

        public ESensorConnectionState SensorsConnectionState
        {
            get { return (ESensorConnectionState)GetValue(SensorsConnectionStateProperty); }
            set { SetValue(SensorsConnectionStateProperty, value); }
        }

        public static readonly BindableProperty DeviceNameTappedCommandProperty = BindableProperty.Create
            (nameof(DeviceNameTappedCommand), typeof(ICommand), typeof(RetractableToolBar), default(ICommand));

        public ICommand DeviceNameTappedCommand
        {
            get { return (ICommand)GetValue(DeviceNameTappedCommandProperty); }
            set { SetValue(DeviceNameTappedCommandProperty, value); }
        }

        public static readonly BindableProperty DeviceInfoProperty = BindableProperty.Create
            (nameof(Devices), typeof(BiopotGenericInfo), typeof(RetractableToolBar), null, 
            BindingMode.TwoWay, propertyChanged: OnDeviceInfoChanged);

        public BiopotGenericInfo DeviceInfo
        {
            get => (BiopotGenericInfo)GetValue(DeviceInfoProperty);
            set => SetValue(DeviceInfoProperty, value);
        }

        public static readonly BindableProperty IsConnectionLostProperty = BindableProperty.Create
          (nameof(IsConnectionLost), typeof(bool), typeof(RetractableToolBar), default(bool));

        public bool IsConnectionLost
        {
            get { return (bool)GetValue(IsConnectionLostProperty); }
            set { SetValue(IsConnectionLostProperty, value); }
        }

        /** Bindable property to let the outer view know about menu state changes. */
        public static readonly BindableProperty IsMenuOpenedProperty = BindableProperty.Create
        (nameof(IsMenuOpened), typeof(bool), typeof(RetractableToolBar),
            default(bool), BindingMode.TwoWay,
            propertyChanged:
            (aBindable, aValue, aNewValue) =>
            {
                if (aBindable is RetractableToolBar toolBar)
                {
                    // when this flag changes separately from the actual 'open state',
                    // ensure they both are synchronized
                    toolBar.IsOpenState = toolBar.IsMenuOpened;
                }
            });

        public bool IsMenuOpened
        {
            get => (bool)GetValue(IsMenuOpenedProperty);
            set => SetValue(IsMenuOpenedProperty, value);
        }

        public static readonly BindableProperty CanOpenMenuProperty = BindableProperty.Create
        (nameof(CanOpenMenu), typeof(bool), typeof(RetractableToolBar), true, BindingMode.TwoWay);

        public bool CanOpenMenu
        {
            get => (bool)GetValue(CanOpenMenuProperty);
            set => SetValue(CanOpenMenuProperty, value);
        }

        public static readonly BindableProperty SPSValueProperty = BindableProperty.Create
            (nameof(SPSValue), typeof(short), typeof(RetractableToolBar), default(short));

        public short SPSValue
        {
            get { return (short)GetValue(SPSValueProperty); }
            set { SetValue(SPSValueProperty, value); }
        }

        #endregion

        #region -- Private helpers --

        private void OnSizeOmegaIconGridChanged(object sender, EventArgs e)
        {
            omegaIconGrid.HeightRequest = omegaIconGrid.Height;
            omegaIconGrid.WidthRequest = omegaIconGrid.Width;
            omegaIconGrid.MinimumHeightRequest = omegaIconGrid.Height;
            omegaIconGrid.MinimumWidthRequest = omegaIconGrid.Width;
            omegaIconGrid.SizeChanged -= OnSizeOmegaIconGridChanged;
        }

        private void SPStackSizeChanged(object sender, EventArgs e)
        {
            SPStack.HeightRequest = SPStack.Height;
            SPStack.WidthRequest = SPStack.Width;
            SPStack.MinimumHeightRequest = SPStack.Height;
            SPStack.MinimumWidthRequest = SPStack.Width;
            SPStack.SizeChanged -= SPStackSizeChanged;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(SelectAllEEGEMG))
            {
	            var device = Devices.FirstOrDefault(x => x.DeviceType == EDeviceType.EEGorEMG);
	            if (device != null)
	            {
		            device.SelectAll = SelectAllEEGEMG;
	            }
            }

            if (propertyName == nameof(SelectAllBioImpedance))
            {
	            var device = Devices.FirstOrDefault(x => x.DeviceType == EDeviceType.BioImpedance);
	            if (device != null)
	            {
		            device.SelectAll = SelectAllBioImpedance;
				}
            }

            if (propertyName == nameof(SelectAllAccelerometer))
            {
	            var device = Devices.FirstOrDefault(x => x.DeviceType == EDeviceType.Accelerometer);
	            if (device != null)
	            {
		            device.SelectAll = SelectAllAccelerometer;
	            }
            }

            if (propertyName == nameof(IsOpenState) && !IsOpenState)
            {
                UpdateChartState.Execute(null);
            }
        }

        /// <summary>
        /// Handles changes of device info property.
        /// </summary>
        /// <param name="bindable"> The bindable object. </param>
        /// <param name="oldValue"> Old value. </param>
        /// <param name="newValue"> New value. </param>
        private static void OnDeviceInfoChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == null)
            {
                return;
            }

            var toobar = (RetractableToolBar) bindable;
            var deviceInfo = (BiopotGenericInfo) newValue;

            toobar.ChannelsNumber = deviceInfo.ChannelsNumber;
            toobar.IsAccelerometerPresent = deviceInfo.IsAccelerometerPresent;
            toobar.IsBioImpedancePresent = deviceInfo.IsBioImpedancePresent;
            toobar.DeviceName = $"Biopot: {deviceInfo.DeviceName}";
            toobar.BioImpedanceFormatted = deviceInfo.BioImpedanceChannelNumber == 0 
                ? Strings.DeviceConfigurationBoImpedance 
                : string.Format(Strings.DeviceConfigurationBoImpedanceWithChannels, deviceInfo.BioImpedanceChannelNumber);
        }

        private static void OnDevicesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == null)
            {
                return;
            }
            var _this = (RetractableToolBar)bindable;
            _this.CreateCheckBoxContent((IList<CheckboxesGroupedByDevice>)newValue);
        }

        private void CreateCheckBoxContent(IList<CheckboxesGroupedByDevice> devices)
        {
            CleanPreviousCheckboxes();

            foreach (var chartsGroupe in devices)
            {
                BuildAndFillGridCheckbox(chartsGroupe);
            }
        }

        /// <summary>
        /// Cleans previous checkboxes in grid except 'Select all'.
        /// </summary>
        private void CleanPreviousCheckboxes()
        {
            var eegemCheckBoxes = EEGEMGrid.Children.Where(x => x.GetType() == typeof(ChartCheckBox)).ToList();
            foreach (var item in eegemCheckBoxes)
            {
                EEGEMGrid.Children.Remove(item);
            }

            var bioCheckBoxes = BioGrid.Children.Where(x => x.GetType() == typeof(ChartCheckBox)).ToList();
            foreach (var item in bioCheckBoxes)
            {
                BioGrid.Children.Remove(item);
            }

            var accelCheckBoxes = AccelerGrid.Children.Where(x => x.GetType() == typeof(ChartCheckBox)).ToList();
            foreach (var item in accelCheckBoxes)
            {
                AccelerGrid.Children.Remove(item);
            }

            _heightEEGEMGrid = 0;
            _heightAccelerGrid = 0;
            _heightBioGrid = 0;
        }

        private void BuildAndFillGridCheckbox(CheckboxesGroupedByDevice chartsGroupe)
        {
            int rowCount = chartsGroupe.Count / _conutCheckboxInRow; //4 - conut checkbox in row
            if (chartsGroupe.Count % _conutCheckboxInRow != 0)
                rowCount++;
            int heightContent = rowCount * _heightSingleRow; //50 - height single row

            for (int i = 0; i < chartsGroupe.Count; i++)
            {
                var row = (i / _conutCheckboxInRow) + 1;
                var column = i - (row - 1) * _conutCheckboxInRow;

                var checkBox = new ChartCheckBox();
                checkBox.SetBinding(ChartCheckBox.TextProperty, new Binding("Id", source: chartsGroupe[i]));
                checkBox.SetBinding(ChartCheckBox.IsCheckedProperty, new Binding("IsChecked", source: chartsGroupe[i]));

                switch (chartsGroupe.DeviceType)
                {
                    case EDeviceType.EEGorEMG:
                        _heightEEGEMGrid = heightContent + _heightSingleRow;
                        checkBox.SetBinding(ChartCheckBox.IsSelectedAllProperty, new Binding(nameof(SelectAllEEGEMG), source: this));
                        EEGEMGrid.Children.Add(checkBox, column, row);
                        break;
                    case EDeviceType.BioImpedance:
                        _heightBioGrid = heightContent + _heightSingleRow;
                        checkBox.SetBinding(ChartCheckBox.IsSelectedAllProperty, new Binding(nameof(SelectAllBioImpedance), source: this));
                        BioGrid.Children.Add(checkBox, column, row);
                        break;
                    case EDeviceType.Accelerometer:
                        _heightAccelerGrid = heightContent + _heightSingleRow;
                        checkBox.SetBinding(ChartCheckBox.IsSelectedAllProperty, new Binding(nameof(SelectAllAccelerometer), source: this));
                        AccelerGrid.Children.Add(checkBox, column, row);
                        break;
                    default:
                        throw new Exception("Unknow device type!");
                        break;
                }
            }
        }

        private void UpdateMenuContentHeight(int contentHeight)
        {
            switch (contentHeight)
            {
                case 0:
                    contentHeight = _heightEEGEMGrid;
                    break;
                case 1:
                    contentHeight = _heightBioGrid;
                    break;
                default:
                    contentHeight = _heightAccelerGrid;
                    break;
            }

            _menuContentHeight = contentHeight;//135
            _menuHeight = MENU_FOOTER + MENU_TABSTRIPBUTTON + _menuContentHeight + 5; // 5 space for shadow icon

            box.HeightRequest = _menuHeight;
        }

        /// <summary>
        /// Opens settings menu.
        /// </summary>
        /// FIXME
        private void OnOpenCloseMenuCommand()
        {
            IsOpenState = !IsOpenState;
        }
        #endregion
    }
}
