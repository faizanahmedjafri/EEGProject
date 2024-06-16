using Prism.Mvvm;
using SharedCore.Models;

namespace biopot.Models
{
    /// <summary>
    /// The view model for single device connection.
    /// </summary>
    public sealed class DeviceConnectionViewModel : BindableBase
    {
        private string iDeviceName;
        private int iEegChannelsCount;
        private int iBioImpendanceChannelsCount;
        private int iAccelerometerChannelsCount;
        private bool iIsSelected;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="aModel">The device model.</param>
        public DeviceConnectionViewModel(DeviceModel aModel)
        {
            Model = aModel;
            DeviceName = Model.DeviceName;
            EEGChannelsCount = Model.EEGChannelsCount;
            BioImpendanceChannelsCount = Model.BioImpendanceChannelsCount;
            AccelerometerChannelsCount = Model.AccelerometerChannelsCount;
            IsSelected = false;
        }

        /// <summary>
        /// Gets the device (connection) model.
        /// </summary>
        public DeviceModel Model { get; }

        /// <summary>
        /// Gets/sets if the device connection is selected by user.
        /// </summary>
        public bool IsSelected
        {
            get => iIsSelected;
            set => SetProperty(ref iIsSelected, value);
        }

        /// <summary>
        /// Gets/sets device name.
        /// </summary>
        public string DeviceName
        {
            get => iDeviceName;
            set => SetProperty(ref iDeviceName, value);
        }

        /// <summary>
        /// Gets/sets count of EEG/EMG channels.
        /// </summary>
        public int EEGChannelsCount
        {
            get => iEegChannelsCount;
            set => SetProperty(ref iEegChannelsCount, value);
        }

        /// <summary>
        /// Gets/sets count of bio-impedance channels.
        /// If 0, then the bio-impedance data is not present.
        /// </summary>
        public int BioImpendanceChannelsCount
        {
            get => iBioImpendanceChannelsCount;
            set => SetProperty(ref iBioImpendanceChannelsCount, value);
        }

        /// <summary>
        /// Gets/sets count of accelerometer channels.
        /// If 0, then the accelerometer data is not present.
        /// </summary>
        public int AccelerometerChannelsCount
        {
            get => iAccelerometerChannelsCount;
            set => SetProperty(ref iAccelerometerChannelsCount, value);
        }
    }
}