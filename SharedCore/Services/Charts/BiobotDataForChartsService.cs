using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Exceptions;
using SharedCore.Enums;
using SharedCore.Extensions;
using SharedCore.Models;

namespace SharedCore.Services.Charts
{
    public class BiobotDataForChartsService : IBiobotDataForChartsService
    {
        private const int MaxExpectedLength = 244;
        private readonly Guid _serviceGuid = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
        private readonly Guid _characterGuid = new Guid("0000fff2-0000-1000-8000-00805f9b34fb");

        private readonly IBlueToothService _blueToothService;

        public BiobotDataForChartsService(IBlueToothService blueToothService)
        {
            _blueToothService = blueToothService;
        }

        #region -- IBiobotDataForChartsService implemetations --

        public async Task<AOResult<EDataForChartsState>> GetDataStateAsync()
        {
            var result = new AOResult<EDataForChartsState>();
            if (IsDeviceConnected())
            {
	            try
	            {
		            var service = await _blueToothService.CurrentDevice.GetServiceAsync(_serviceGuid);

		            var charact = await service.GetCharacteristicAsync(_characterGuid);
		            var data = await charact.ReadAsync();
		            var val = BitConverter.ToString(data);

		            EDataForChartsState state = EDataForChartsState.Stop; // default value

		            switch (val)
		            {
			            case "01":
				            state = EDataForChartsState.Start;
				            break;
			            case "02":
				            state = EDataForChartsState.Stop;
				            break;
			            case "00":
				            state = EDataForChartsState.Pause;
				            break;
			            default:
				            throw new Exception("Unknown DataForChartsState!");
		            }

		            result.SetSuccess(state);
	            }
	            catch (CharacteristicReadException)
	            {
		            result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
	            }
	            catch (InvalidOperationException)
	            {
	                result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
	            }
            }
            else
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
            }
            return result;
        }

        /// <inheritdoc />
        async Task<AOResult> IBiobotDataForChartsService.PauseAsync()
        {
            return await WriteDataAsync(new byte[] {00});
        }

        /// <inheritdoc />
        async Task<AOResult> IBiobotDataForChartsService.StartAsync()
        {
            return await WriteDataAsync(new byte[] {01});
        }

        /// <inheritdoc />
        async Task<AOResult> IBiobotDataForChartsService.StopAsync()
        {
            return await WriteDataAsync(new byte[] {02});
        }

        /// <inheritdoc/>
        async Task<AOResult> IBiobotDataForChartsService.SetDataAsync(string aData)
        {
            return await WriteDataAsync(aData.ToByteArray());
        }

        /// <summary>
        /// Writes specified data to currently connected device.
        /// </summary>
        /// <param name="aBytes">The buffer to write to.</param>
        /// <returns>operation result.</returns>
        private async Task<AOResult> WriteDataAsync(byte[] aBytes)
        {
            var result = new AOResult();
            result.SetSuccess();

            if (!IsDeviceConnected())
            {
                result.SetFailure($"{(int) SignalRetrievalErrors.DeviceNotConnected}");
                return result;
            }

            if (aBytes.Length > MaxExpectedLength)
            {
                result.SetFailure($"{(int) SignalRetrievalErrors.InsufficientBufferLength}");
                return result;
            }

            try
            {
                await WriteCharacteristicValueAsync(aBytes);
            }
            catch (CharacteristicReadException)
            {
                result.SetFailure($"{(int) SignalRetrievalErrors.CharacteristicReadException}");
            }
            catch (InvalidOperationException)
            {
                result.SetFailure($"{(int) SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
            }

            return result;
        }

        /// <inheritdoc/>
        bool IBiobotDataForChartsService.IsValidDataToWrite(string aData)
        {
            if (aData.CanConvertToByteArrayOfSize())
            {
                if (aData.Equals("00") || aData.Equals("01") || aData.Equals("02"))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region -- Private helpers -- 

        private bool IsDeviceConnected()
        {
            return _blueToothService.CurrentDevice != null && _blueToothService.CurrentDevice.State == DeviceState.Connected;
        }

        /// <summary>
        /// Writes characteristic value of the service.
        /// </summary>
        /// <param name="aValue">The value buffer to write.</param>
        private async Task WriteCharacteristicValueAsync(byte[] aValue)
        {
            var service = await _blueToothService.CurrentDevice.GetServiceAsync(_serviceGuid);
            var characteristic = await service.GetCharacteristicAsync(_characterGuid);

            await characteristic.WriteAsync(aValue);
        }

        #endregion
    }
}
