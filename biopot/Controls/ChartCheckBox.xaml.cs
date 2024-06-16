using System;
using System.Runtime.CompilerServices;
using SharedCore.Services;
using Xamarin.Forms;

namespace biopot.Controls
{
    public partial class ChartCheckBox : ContentView
    {
        private readonly ILogService _logService;

        public ChartCheckBox()
        {
            InitializeComponent();

            _logService = App.Resolve<ILogService>();
        }

        public bool IsSelectedAllCheckBox
        {
            get;
            set;
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create
           (nameof(Text), typeof(string), typeof(ChartCheckBox), default(string));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create
            (nameof(IsChecked), typeof(bool), typeof(ChartCheckBox), default(bool), BindingMode.TwoWay);
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly BindableProperty IsSelectedAllProperty = BindableProperty.Create
            (nameof(IsSelectedAll), typeof(bool), typeof(ChartCheckBox), default(bool), BindingMode.TwoWay, propertyChanged: OnIsSelectedAllChanged);

        public bool IsSelectedAll
        {
            get { return (bool)GetValue(IsSelectedAllProperty); }
            set { SetValue(IsSelectedAllProperty, value); }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == nameof(IsChecked))
            {
                _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Check box Checked = {IsChecked}. Name = {Text}");
            }
        }

        #region -- Private helpers --

        private static void OnIsSelectedAllChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var _this = (ChartCheckBox)bindable;

            if ((bool)newValue)
            {
                _this.IsChecked = true;

                if (_this.IsSelectedAllCheckBox)
                {
                    _this.selectedAll.Source = "check_on";
                }
            }
        }

        private void OnTapedCommand(object sender, EventArgs e)
        {
            if (IsSelectedAllCheckBox)
            {
                IsSelectedAll = !IsSelectedAll;

                _logService.CreateLogDataAsync($"{Constants.Logs.EVENT}: Cheсл box IsSelectedAllCheckBox IsSelectedAll = {IsSelectedAll}");
            }

            if (IsSelectedAll)
            {
                return;
            }
            else
            {
                IsChecked = !IsChecked;
            }
        }

        #endregion
    }
}
