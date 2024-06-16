using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using System.Threading;
using System.Diagnostics;
using Plugin.BLE.Abstractions.EventArgs;
using System.Linq;
using Plugin.BLE.Abstractions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SharedCore.Models;

namespace SharedCore.Services
{
	public class BlueToothService : IBlueToothService
	{
        private readonly Regex iDeviceParamsMatcher = 
            new Regex(@"BIO(?<params>\/v(?<ver>\d+)-(?<eeg>\d+)-(?<imp>\d+)-(?<acc>\d+))$", RegexOptions.None);

		private readonly IBluetoothLE _bluetooth;
		private readonly IAdapter _adapter;
		private const int SCAN_TIMEOUT = 2000; // milliseconds

		private CancellationTokenSource _findDeviceCancellationTokenSource;

		public BlueToothService(IAdapter adapter, IBluetoothLE bluetooth)
		{
			_adapter = adapter;
			_adapter.ScanTimeout = SCAN_TIMEOUT;
			_adapter.DeviceConnected += OnDeviceStateChanged;
			_adapter.DeviceDisconnected += OnDeviceStateChanged;
			_adapter.DeviceConnectionLost += OnDeviceStateChanged;

			_bluetooth = bluetooth;
            _bluetooth.StateChanged += OnBluetoothStateChanged;

        }

        ~BlueToothService()
		{
			_adapter.DeviceConnected -= OnDeviceStateChanged;
			_adapter.DeviceDisconnected -= OnDeviceStateChanged;
			_adapter.DeviceConnectionLost -= OnDeviceStateChanged;
		}

		#region -- Public properties -- 

        /// <inheritdoc />
        public bool BlueToothIsOn => _bluetooth.IsOn;

        /// <inheritdoc />
        public event EventHandler<bool> BluetoothEnabledChanged;

        /// <inheritdoc />
        public event EventHandler<bool> OnChangedDeviceConnection;

        /// <inheritdoc />
        public IDevice CurrentDevice { get; set; }

        /// <inheritdoc />
        public IList<DeviceModel> Devices { get; private set; } = new List<DeviceModel>();

        /// <inheritdoc />
        DeviceModel IBlueToothService.CurrentDeviceAsModel
        {
            get
            {
                var device = CurrentDevice;
                if (device == null)
                {
                    return null;
                }

                return FillChannelsCountForDevice(new DeviceModel(device));
            }
        }

        #endregion

        #region -- IBlueToothService implementation --

        public async Task<bool> ConnectDeviceAsync(IDevice device)
		{
			try
			{
				if (device == null)
				{
					return false;
				}
				else
				{
					if (device.State != DeviceState.Connecting || device.State != DeviceState.Connected)
					{

						var connectCancellationTokenSource = new CancellationTokenSource();
						connectCancellationTokenSource.CancelAfter(SCAN_TIMEOUT);
						await _adapter.ConnectToDeviceAsync(device, cancellationToken: connectCancellationTokenSource.Token);

						if (_adapter.IsScanning)
						{
							await _adapter.StopScanningForDevicesAsync();
						}
					}
				}

				return device.State == DeviceState.Connected;
			}
			catch (Exception e)
			{
#if DEBUG
				Debug.WriteLine(e.Message);
#endif
				return false;
			}
		}

		public async Task DisconnectDeviceAsync(IDevice device)
		{
			if (device != null && device.State != DeviceState.Disconnected)
			{
				await _adapter.DisconnectDeviceAsync(device);
			}
		}

		public async Task<bool> ScanningDevicesAsync()
		{
			_findDeviceCancellationTokenSource = new CancellationTokenSource();
			_findDeviceCancellationTokenSource.CancelAfter(SCAN_TIMEOUT);
			try
			{
				await _adapter.StartScanningForDevicesAsync(cancellationToken: _findDeviceCancellationTokenSource.Token);
			}
			catch (Exception e)
			{
#if DEBUG
				Debug.WriteLine(e.Message);
#endif
			}

			return CurrentDevice != null && CurrentDevice?.State == DeviceState.Connected || CurrentDevice?.State == DeviceState.Connecting;
		}

		public async Task StopScanningDevicesAsync()
		{
			if (_adapter.IsScanning)
			{
				await _adapter.StopScanningForDevicesAsync();
			}
		}

        public async Task<bool> ScanningListOfDevicesAsync()
		{
			await _adapter.StartScanningForDevicesAsync();

			Devices = new List<DeviceModel>(new HashSet<DeviceModel>(_adapter.DiscoveredDevices
                .Where(HasMatchingDeviceName)
                .Select(x => new DeviceModel(x))
                .Select(FillChannelsCountForDevice)));
			return Devices != null && Devices.Count != 0;
		}

		public async Task<bool> ScanningDeviceByIdAsync(string id)
		{
			await _adapter.StartScanningForDevicesAsync(deviceFilter: (arg) => arg.Id.ToString() == id);

			Devices = new List<DeviceModel>(new HashSet<DeviceModel>(_adapter.DiscoveredDevices
                    .Where(HasMatchingDeviceName)
                    .Select(x => new DeviceModel(x))
                    .Select(FillChannelsCountForDevice)));
			var isDeviceFound = Devices != null && Devices.Count != 0;
			if (isDeviceFound)
			{
				CurrentDevice = Devices.ElementAt(0).Device;
			}
			return isDeviceFound;
		}

        #endregion

		#region -- Private helpers --

		private void OnDeviceStateChanged(object sender, DeviceEventArgs e)
		{
			OnChangedDeviceConnection?.Invoke(null, e.Device.State == DeviceState.Connected);
#if DEBUG
			Debug.WriteLine("---- Device ---" + e.Device.State);
#endif
		}

        private void OnBluetoothStateChanged(object aSender, BluetoothStateChangedArgs aArgs)
        {
            BluetoothEnabledChanged?.Invoke(this, BlueToothIsOn);
        }

        /// <summary>
        /// Determines if the device has a name, which identifies a device, which we can work with.
        /// </summary>
        /// <param name="aDevice">The device to check.</param>
        /// <returns>true, if the device is supported; otherwise, false.</returns>
        private bool HasMatchingDeviceName(IDevice aDevice)
        {
            return !string.IsNullOrEmpty(aDevice.Name) 
                   && aDevice.Name.Contains(Constants.DEVICE_NAME);
        }

        /// <summary>
        /// Recognizes channels count from device's name.
        /// </summary>
        /// <param name="aDevice">The device to get params from, and fill to.</param>
        /// <returns>update device info.</returns>
        private DeviceModel FillChannelsCountForDevice(DeviceModel aDevice)
        {
            var match = iDeviceParamsMatcher.Match(aDevice.DeviceName);
            if (match.Success)
            {
                int parsedValue;

                if (int.TryParse(match.Groups["acc"].Value, out parsedValue))
                {
                    aDevice.AccelerometerChannelsCount = Math.Max(0, parsedValue);
                }

                if (int.TryParse(match.Groups["eeg"].Value, out parsedValue))
                {
                    aDevice.EEGChannelsCount = Math.Max(0, parsedValue);
                }

                if (int.TryParse(match.Groups["imp"].Value, out parsedValue))
                {
                    aDevice.BioImpendanceChannelsCount = Math.Max(0, parsedValue);
                }

                // leave device name without this parameters section
                var paramsGroup = match.Groups["params"];
                aDevice.DeviceName = aDevice.DeviceName.Remove(paramsGroup.Index, paramsGroup.Length);
            }

            return aDevice;
        }

        #endregion
    }
}
