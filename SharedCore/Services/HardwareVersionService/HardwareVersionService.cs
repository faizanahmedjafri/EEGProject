using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using SharedCore.Models;
using System.Linq;
using System.Text;

namespace SharedCore.Services
{
    public class HardwareVersionService : IHardwareVersionService
    {
        private readonly Guid _deviceInformationServiceGuid = new Guid("0000180a-0000-1000-8000-00805f9b34fb");
        private readonly Guid _hardwareVersionCharacterGuid = new Guid("00002a27-0000-1000-8000-00805f9b34fb");

        private readonly IBlueToothService _blueToothService;

        public HardwareVersionService(IBlueToothService blueToothService)
        {
            _blueToothService = blueToothService;
        }

        #region -- IHardwareVersionService implemetations --

        public async Task<AOResult<string>> GetHardwareVersionAsync()
        {
            var result = new AOResult<string>();
            if (IsDeviceConnected())
            {
                var deviceInformationService = await _blueToothService.CurrentDevice.GetServiceAsync(_deviceInformationServiceGuid);
                var deviceInformationChar = await deviceInformationService.GetCharacteristicAsync(_hardwareVersionCharacterGuid);
                var data = await deviceInformationChar.ReadAsync();
                var val = deviceInformationChar.StringValue;
                //TODO: Some problem with data, Change to real data
                result.SetSuccess("1.0.0");
            }
            else
            {
                result.SetFailure();
            }
            return result;
        }

        #endregion

        #region -- Private helpers -- 

        private bool IsDeviceConnected()
        {
            return _blueToothService.CurrentDevice != null && _blueToothService.CurrentDevice.State == DeviceState.Connected;
        }

        #endregion
    }
}
