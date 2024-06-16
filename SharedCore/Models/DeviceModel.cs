using System;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace SharedCore.Models
{
	public class DeviceModel
    {
		public DeviceModel(IDevice device)
		{
		    Device = device;
            DeviceName = device.Name;
            RssiLevel = device.Rssi;
        }

		//public string Id => Device.Id.ToString();
		public IDevice Device { get; private set; }
		//public string Name => Device.Name;
		//public int RssiLevel => Device.Rssi;

		//Mock
		public string DeviceName { get; set; }
		public int RssiLevel { get; set; }

        /// <summary>
        /// Gets/sets count of EEG/EMG channels.
        /// </summary>
        public int EEGChannelsCount { get; set; }

        /// <summary>
        /// Gets/sets count of bio-impedance channels.
        /// If 0, then the bio-impedance data is not present.
        /// </summary>
        public int BioImpendanceChannelsCount { get; set; }

        /// <summary>
        /// Gets/sets count of accelerometer channels.
        /// If 0, then the accelerometer data is not present.
        /// </summary>
        public int AccelerometerChannelsCount { get; set; }
    }
}
