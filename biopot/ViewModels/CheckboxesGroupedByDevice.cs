using System;
using System.Collections.Generic;
using biopot.Enums;

namespace biopot.ViewModels
{
    public class ChartCheckboxModel
    {
        public int Id { get; set; }
        public EDeviceType DeviceType { get; set; }
        public bool IsChecked { get; set; }
    }
    public class CheckboxesGroupedByDevice : List<ChartCheckboxModel>
    {
        private bool _SelectAll;
        public bool SelectAll
        {
            get { return _SelectAll; }
            set { _SelectAll = value; }
        }

        private EDeviceType _DeviceType;
        public EDeviceType DeviceType
        {
            get { return _DeviceType; }
            set
            {
                _DeviceType = value;
            }
        }
    }
}
