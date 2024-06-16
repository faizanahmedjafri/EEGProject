using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Exceptions;
using SharedCore.Enums;
using SharedCore.Extensions;
using SharedCore.Models;

namespace SharedCore.Services.Characteristic3Service
{
    public class Characteristic3Service : ICharacteristic3Service
    {
        private enum Characteristic
        {
            ImpedanceEnabledIndex = 0,
            MinLength = 1,
            ExpectedLength = 34,
            MaxExpectedLength = 244,
        }

        private readonly Guid _serviceGuid = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
        private readonly Guid _characterGuid = new Guid("0000fff3-0000-1000-8000-00805f9b34fb");

        private readonly IBlueToothService _blueToothService;

        public Characteristic3Service(IBlueToothService blueToothService)
        {
            _blueToothService = blueToothService;
        }

        /// <inheritdoc/>
        async Task<AOResult<string>> ICharacteristic3Service.GetInfoAsync()
        {
            var result = new AOResult<string>();
            if (IsDeviceConnected())
            {
                try
                {
                    var data = await ReadCharacteristicValueAsync();
                    var val = BitConverter.ToString(data);
                    result.SetSuccess(val);
                }
                catch (CharacteristicReadException e)
                {
                    result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
                }
            }
            else
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
            }

            return result;
        }

        /// <inheritdoc/>
        async Task<AOResult> ICharacteristic3Service.SetDataAsync(string aData)
        {
            var result = new AOResult();
            result.SetSuccess();

            if (!IsDeviceConnected())
            {
                result.SetFailure();
                return result;
            }

            byte[] bytes = aData.ToByteArray();
            if (bytes.Length > (int) Characteristic.MaxExpectedLength)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                return result;
            }

            try
            {
                await WriteCharacteristicValueAsync(bytes);
            }
            catch (InvalidOperationException)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
            }

            return result;
        }

        /// <inheritdoc />
        async Task<AOResult> ICharacteristic3Service.SetImpedanceEnabledAsync(bool aEnabled)
        {
            var result = new AOResult();
            if (!IsDeviceConnected())
            {
                result.SetFailure();
                return result;
            }

            try
            {
                var buffer = await ReadCharacteristicValueAsync();
                if (buffer.Length < (int) Characteristic.MinLength)
                {
                    result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
                }

                buffer[(int) Characteristic.ImpedanceEnabledIndex] = (byte) (aEnabled ? 1 : 0);

                await WriteCharacteristicValueAsync(buffer);

                result.SetSuccess();
            }
            catch (CharacteristicReadException)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
            }
            catch (InvalidOperationException)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
            }

            return result;
        }

        /// <inheritdoc/>
        bool ICharacteristic3Service.IsValidDataToWrite(string aData)
        {
            return aData.CanConvertToByteArrayOfSize();
        }

        #region -- Private helpers -- 

        /// <summary>
        /// Reads characteristic value of the service.
        /// </summary>
        /// <returns>read value buffer.</returns>
        private async Task<byte[]> ReadCharacteristicValueAsync()
        {
            var service = await _blueToothService.CurrentDevice.GetServiceAsync(_serviceGuid);
            var characteristic = await service.GetCharacteristicAsync(_characterGuid);

            if (!characteristic.CanRead)
            {
                // just create a stub buffer, which will override any existing buffer,
                // currently, we don't care about data loss in the characteristic, if any happens.
                return new byte[(int) Characteristic.ExpectedLength];
            }
            try
            {
                return await characteristic.ReadAsync();
            }
            catch(InvalidOperationException e1)
            {
                Debug.WriteLine($" ---- Invalid Read operation --- {e1}");
                return new byte[(int)Characteristic.ExpectedLength];
            }
            catch (CharacteristicReadException e2)
            {
                Debug.WriteLine($" ---- Read Exception --- {e2}");
                return new byte[(int)Characteristic.ExpectedLength];
            }
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

        /// <summary>
        /// Checkes whether device is connected or not.
        /// </summary>
        /// <returns> True - device is connected, otherwise - false. </returns>
        private bool IsDeviceConnected()
        {
            return _blueToothService.CurrentDevice != null && _blueToothService.CurrentDevice.State == DeviceState.Connected;
        }
        #endregion
    }
}
