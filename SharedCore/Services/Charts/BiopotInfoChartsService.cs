using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using SharedCore.Enums;
using SharedCore.Extensions;
using SharedCore.Models;

namespace SharedCore.Services.Charts
{
	public class BiopotInfoChartsService : IBiopotInfoChartsService
	{
	    private const int MaxExpectedLength = 244;
        private const int ChannelsCountIndex = 5;
		private const int AccelerometerStatusIndex = 6;
		private const int BioImpedanceStatusIndex = 7;
		private const int NumberOfSamplesPerChannelIndex = 9;
		private const int InfoBufferMinLength = NumberOfSamplesPerChannelIndex + 1;

		private readonly Guid _serviceGuid = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
		private readonly Guid _characterGuid = new Guid("0000fff1-0000-1000-8000-00805f9b34fb");

		private readonly IBlueToothService _blueToothService;

	    private readonly IDictionary<Guid, BiopotGenericInfo> _cachedBiopotInfos
	        = new ConcurrentDictionary<Guid, BiopotGenericInfo>();

		public BiopotInfoChartsService(IBlueToothService blueToothService)
		{
			_blueToothService = blueToothService;
		}

		#region -- IBiopotInfoChartsService implemetations --

		/// <inheritdoc />
		async Task<AOResult<string>> IBiopotInfoChartsService.GetInfoAsync()
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
			        result.SetFailure($"{(int) SignalRetrievalErrors.CharacteristicReadException}");
			    }
			    catch (InvalidOperationException e)
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
		async Task<AOResult<BiopotGenericInfo>> IBiopotInfoChartsService.GetDeviceInfoAsync()
		{
			var result = new AOResult<BiopotGenericInfo>();
			if (IsDeviceConnected())
			{
				var currentDevice = _blueToothService.CurrentDeviceAsModel;
				if (_cachedBiopotInfos.TryGetValue(
					currentDevice.Device.Id, out BiopotGenericInfo info))
				{
					// use cached info, if present
					result.SetSuccess(info);
				}
				else
				{
					var buffer = await ReadCharacteristicValueAsync();
				    info = buffer.Length >= InfoBufferMinLength
                        ? GetInfoFromCharacteristic(buffer)
                        : GetInfoFromName(currentDevice.Device);

					info.DeviceName = currentDevice.DeviceName;

					// cache received info for easier access;
					// it expected to never change on particular device.
					_cachedBiopotInfos[currentDevice.Device.Id] = info;
				}

				result.SetSuccess(info);
			}
			else
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
            }

			return await Task.FromResult(result);
		}

        /// <inheritdoc/>
        async Task<AOResult> IBiopotInfoChartsService.SetDataAsync(string aData)
	    {
	        var result = new AOResult();
	        result.SetSuccess();

            if (!IsDeviceConnected())
	        {
	            result.SetFailure($"{SignalRetrievalErrors.DeviceNotConnected}");
	            return result;
	        }

	        byte[] bytes = aData.ToByteArray();
	        if (bytes.Length > MaxExpectedLength)
	        {
	            result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                return result;
	        }

	        try
	        {
	            await WriteCharacteristicValueAsync(bytes);
            }
	        catch (InvalidOperationException e)
	        {
	            result.SetFailure($"{SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
            }
            
	        return result;
        }

	    /// <inheritdoc/>
	    bool IBiopotInfoChartsService.IsValidDataToWrite(string aData)
	    {
	        return aData.CanConvertToByteArrayOfSize();
        }

	    #endregion

		#region -- Private helpers -- 

		private bool IsDeviceConnected()
		{
			return _blueToothService.CurrentDevice != null && _blueToothService.CurrentDevice.State == DeviceState.Connected;
		}

		/// <summary>
		/// Reads characteristic value of the service.
		/// </summary>
		/// <returns>read value buffer.</returns>
		private async Task<byte[]> ReadCharacteristicValueAsync()
		{
			var service = await _blueToothService.CurrentDevice.GetServiceAsync(_serviceGuid);
			var characteristic = await service.GetCharacteristicAsync(_characterGuid);

			return await characteristic.ReadAsync();
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
        /// Gets information from characteristic's value.
        /// </summary>
        /// <param name="aValue">The BLE characteristics value.</param>
        /// <returns>biopot device info.</returns>
        private BiopotGenericInfo GetInfoFromCharacteristic(byte[] aValue)
        {
            var info = new BiopotGenericInfo
            {
                ChannelsNumber = aValue[ChannelsCountIndex],
                SamplesPerChannelNumber = aValue[NumberOfSamplesPerChannelIndex],
                AccelerometerMode = aValue[AccelerometerStatusIndex],
                IsAccelerometerPresent = aValue[AccelerometerStatusIndex] != 0,
                IsBioImpedancePresent = aValue[BioImpedanceStatusIndex] != 0,
            };
            info.BioImpedanceChannelNumber = info.IsBioImpedancePresent
                ? BiopotGenericInfo.DefaultBioImpedanceChannelCount
                : 0;
            info.AccelerometerChannelNumber = info.IsAccelerometerPresent
                ? BiopotGenericInfo.DefaultAccelerometerChannelCount
                : 0;
            return info;
        }

		/// <summary>
		/// Gets information from device's name.
		/// </summary>
		/// <param name="aDevice">The BLE device to get info from.</param>
		/// <returns>biopot device info.</returns>
		private BiopotGenericInfo GetInfoFromName(IDevice aDevice)
		{
			BiopotGenericInfo info;
			if ("SML BIO".Equals(aDevice.Name))
			{
				info = new BiopotGenericInfo
				{
					ChannelsNumber = 19,
					SamplesPerChannelNumber = 6,
					IsAccelerometerPresent = true,
					AccelerometerChannelNumber = BiopotGenericInfo.DefaultAccelerometerChannelCount,
					IsBioImpedancePresent = false,
					BioImpedanceChannelNumber = 0,
				};
			}
			else
			{
				info = new BiopotGenericInfo
				{
					ChannelsNumber = 16,
					SamplesPerChannelNumber = 7,
					IsAccelerometerPresent = true,
                    AccelerometerChannelNumber = BiopotGenericInfo.DefaultAccelerometerChannelCount,
					IsBioImpedancePresent = true,
					BioImpedanceChannelNumber = BiopotGenericInfo.DefaultBioImpedanceChannelCount,
				};
			}

			return info;
		}

		#endregion
	}
}
