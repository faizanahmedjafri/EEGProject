using System.Collections.ObjectModel;
using System.ComponentModel;
using biopot.Enums;
using biopot.Extensions;

namespace biopot.ViewModels
{
	public class ChartsGroupedByDevice : ObservableCollection<ChartViewModel>
    {
        private EDeviceType _DeviceType;
        public EDeviceType DeviceType
        {
            get => _DeviceType;
            set
            {
                if (_DeviceType != value)
                {
                    _DeviceType = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DeviceType)));

                    // trigger dependent properties
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
		public string Name => DeviceType.GetName();

		private int _PickerValue;
		public int PickerValue
		{
			get { return _PickerValue; }
            set
            {
                if (_PickerValue != value)
                {
                    _PickerValue = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(PickerValue)));

                    foreach (var item in Items)
                    {
                        item.ViewportHalfY = _PickerValue;
                    }
                }
            }
		}

		private int _MinValue;
		public int MinValue
		{
			get { return _MinValue; }
			set
			{
				_MinValue = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinValue)));
			}
		}
		private int _MaxValue;
		public int MaxValue
		{
			get { return _MaxValue; }
			set
			{
				_MaxValue = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxValue)));
			}
		}

		private float _HeaderHeight = 65;
		public float HeaderHeight
		{
			get { return _HeaderHeight; }
			set
			{
				_HeaderHeight = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(HeaderHeight)));
			}
		}

		private bool _IsHeaderVisible = true;
		public bool IsHeaderVisible
		{
			get { return _IsHeaderVisible; }
			set
			{
				_IsHeaderVisible = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsHeaderVisible)));
			}
		}

        #region ObservableCollection<T>

        /// <inheritdoc />
        protected override void SetItem(int aIndex, ChartViewModel aItem)
        {
            base.SetItem(aIndex, aItem);

            // ensure, children have initial properties set
            aItem.ViewportHalfY = PickerValue;
        }

        /// <inheritdoc />
        protected override void InsertItem(int aIndex, ChartViewModel aItem)
        {
            base.InsertItem(aIndex, aItem);

            // ensure, children have initial properties set
            aItem.ViewportHalfY = PickerValue;
        }

        #endregion
    }
}

