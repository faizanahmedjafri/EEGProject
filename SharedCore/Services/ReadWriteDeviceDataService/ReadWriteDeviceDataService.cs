using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
namespace SharedCore.Services
{
    public class ReadWriteDeviceDataService : IReadWriteDeviceDataService
    {
        private readonly IBlueToothService _blueToothService;
        protected readonly Guid _serviceGuid = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");

        private readonly Guid _writeCharacterGuid = new Guid("0000fff2-0000-1000-8000-00805f9b34fb");
        private readonly Guid _notificationCharacterGuid = new Guid("0000fff4-0000-1000-8000-00805f9b34fb");

        private readonly byte[] _valueForOpenChannel = new byte[] { 1 };
        private readonly byte[] _valueForCloseChannel = new byte[] { 0 };

        public ReadWriteDeviceDataService(IBlueToothService blueToothService)
        {
            _blueToothService = blueToothService;
        }

        public async Task<bool> SubscribeUpdateValues()
        {
            var deviceService = await _blueToothService.CurrentDevice.GetServiceAsync(_serviceGuid);
            await SubscribeAll();
            await Task.Delay(1000);
            await OpenChannel();
            return true;
        }

        private async Task SubscribeAll()
        {
            var service = await _blueToothService.CurrentDevice.GetServiceAsync(_serviceGuid);
            var characteristicFff4 = await service.GetCharacteristicAsync(_notificationCharacterGuid);
            characteristicFff4.ValueUpdated += (sender, e) =>
            {
                Debug.WriteLine(" ---- ValueUpdated --- " + e.Characteristic.StringValue);
            };
            await characteristicFff4.StartUpdatesAsync();
        }

        #region -- Private helpers --

        private Task OpenChannel()
        {
            return SendChar(_writeCharacterGuid, _valueForOpenChannel);
        }

        private Task CloseChannel()
        {
            return SendChar(_writeCharacterGuid, _valueForCloseChannel);
        }

        private async Task SendChar(Guid charGuid, byte[] msg, string serviceGuid = "0000fff0-0000-1000-8000-00805f9b34fb")
        {
            var service = await _blueToothService.CurrentDevice.GetServiceAsync(new Guid(serviceGuid));
            var characteristic = await service.GetCharacteristicAsync(charGuid);
            var result = await characteristic.WriteAsync(msg);
#if DEBUG
            Debug.WriteLine("---- IsSended? -----" + characteristic.Id + " ---$" + Encoding.UTF8.GetChars(msg) + "$--- " + result);
#endif
        }

        #endregion
    }
}
