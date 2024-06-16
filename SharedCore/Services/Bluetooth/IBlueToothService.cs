using System;
using System.Threading.Tasks;
using SharedCore.Models;
using System.Collections.Generic;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services
{
    public interface IBlueToothService
    {
        event EventHandler<bool> OnChangedDeviceConnection;
        IDevice CurrentDevice { get; set; }
        Task<bool> ConnectDeviceAsync(IDevice device);
        Task DisconnectDeviceAsync(IDevice device);
        Task<bool> ScanningDevicesAsync();
        Task<bool> ScanningListOfDevicesAsync();
        Task StopScanningDevicesAsync();

        DeviceModel CurrentDeviceAsModel { get;  }
        IList<DeviceModel> Devices { get; }
        Task<bool> ScanningDeviceByIdAsync(string id);

        bool BlueToothIsOn { get; }

        event EventHandler<bool> BluetoothEnabledChanged;
    }
}
