using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Exceptions;
using SharedCore.Enums;
using SharedCore.Extensions;
using SharedCore.Models;

namespace SharedCore.Services.RateInfoService
{
	public class SamplesRateInfoService : ISamplesRateInfoService
	{
	    private const int MaxExpectedLength = 244;
        private const int SoftwareLowPassFilterIndex = 10;
		private const int HardwareLowPassFilterIndex = 4;
		private const int HardwareHighPassFilterIndex = 5;
		private const int SamplingRateLowIndex = 7;
		private const int SamplingRateHighIndex = 6;
		private readonly Guid _serviceGuid = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
		private readonly Guid _characterGuid = new Guid("0000fff5-0000-1000-8000-00805f9b34fb");

		private readonly IBlueToothService _blueToothService;

		public SamplesRateInfoService(IBlueToothService blueToothService)
		{
			_blueToothService = blueToothService;
		}

		#region -- IBiopotInfoChartsService implemetations --

		public async Task<AOResult<string>> GetInfoAsync()
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
	    async Task<AOResult<int>> ISamplesRateInfoService.GetHardwareLowPassFilterAsync()
	    {
	        var result = new AOResult<int>();
	        if (!IsDeviceConnected())
	        {
	            result.SetFailure();
	            return result;
	        }

	        try
	        {
	            var buffer = await ReadCharacteristicValueAsync();
	            if (buffer.Length < (HardwareLowPassFilterIndex + 1))
	            {
	                result.SetFailure("Insufficient buffer length");
	                return result;
	            }

	            int filterCode = buffer[HardwareLowPassFilterIndex];
	            if (filterCode >= 0 && filterCode <= 16)
	            {
	                result.SetSuccess(filterCode);
	            }
	            else
	            {
	                // no filter
	                result.SetSuccess(0);
	            }
	        }
	        catch (CharacteristicReadException e)
	        {
	            result.SetFailure(e.Message);
	        }

	        return result;
	    }

	    /// <inheritdoc />
	    async Task<AOResult> ISamplesRateInfoService.SetHardwareLowPassFilterAsync(int aFilterIndex)
	    {
	        var result = new AOResult();
	        if (!IsDeviceConnected())
	        {
	            result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
                return result;
	        }

	        try
	        {
	            var buffer = await ReadCharacteristicValueAsync();
	            if (buffer.Length < (HardwareLowPassFilterIndex + 1))
	            {
	                result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
	            }

	            if (aFilterIndex < 0 || aFilterIndex > 16)
	            {
	                aFilterIndex = 0;
	            }

	            buffer[HardwareLowPassFilterIndex] = (byte)aFilterIndex;

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

        /// <inheritdoc />
        async Task<AOResult<int>> ISamplesRateInfoService.GetHardwareHighPassFilterAsync()
        {
            var result = new AOResult<int>();
            if (!IsDeviceConnected())
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
                return result;
            }

            try
            {
                var buffer = await ReadCharacteristicValueAsync();
                if (buffer.Length < (HardwareHighPassFilterIndex + 1))
                {
                    result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
                }

                int filterCode = buffer[HardwareHighPassFilterIndex];
                if (filterCode >= 0 && filterCode <= 24)
                {
                    result.SetSuccess(filterCode);
                }
                else
                {
                    // no filter
                    result.SetSuccess(0);
                }
            }
            catch (CharacteristicReadException e)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
            }
            catch (InvalidOperationException e)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
            }

            return result;
        }

        /// <inheritdoc />
        async Task<AOResult> ISamplesRateInfoService.SetHardwareHighPassFilterAsync(int aFilterIndex)
        {
            var result = new AOResult();
            if (!IsDeviceConnected())
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
                return result;
            }

            try
            {
                var buffer = await ReadCharacteristicValueAsync();
                if (buffer.Length < (HardwareHighPassFilterIndex + 1))
                {
                    result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
                }

                if (aFilterIndex < 0 || aFilterIndex > 24)
                {
                    aFilterIndex = 0;
                }

                buffer[HardwareHighPassFilterIndex] = (byte)aFilterIndex;

                await WriteCharacteristicValueAsync(buffer);

                result.SetSuccess();
            }
            catch (CharacteristicReadException e)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
            }
            catch (InvalidOperationException e)
            {
                result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
            }

            return result;
        }

        /// <inheritdoc />
        async Task<AOResult<int>> ISamplesRateInfoService.GetSoftwareLowPassFilterAsync()
		{
			var result = new AOResult<int>();
			if (!IsDeviceConnected())
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
                return result;
			}

			try
			{
				var buffer = await ReadCharacteristicValueAsync();
				if (buffer.Length < (SoftwareLowPassFilterIndex + 1))
				{
				    result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
				}

				int filterCode = buffer[SoftwareLowPassFilterIndex];
				if (filterCode >= 0 && filterCode <= 15)
				{
					result.SetSuccess(filterCode);
				}
				else
				{
					// no filter
					result.SetSuccess(0);
				}
			}
			catch (CharacteristicReadException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
            }
			catch (InvalidOperationException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
			}

            return result;
		}

		/// <inheritdoc />
		async Task<AOResult> ISamplesRateInfoService.SetSoftwareLowPassFilterAsync(int aFilterIndex)
		{
			var result = new AOResult();
			if (!IsDeviceConnected())
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
                return result;
			}

			try
			{
				var buffer = await ReadCharacteristicValueAsync();
				if (buffer.Length < (SoftwareLowPassFilterIndex + 1))
				{
				    result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
				}

				if (aFilterIndex < 0 || aFilterIndex > 15)
				{
					aFilterIndex = 0;
				}

				buffer[SoftwareLowPassFilterIndex] = (byte) aFilterIndex;

				await WriteCharacteristicValueAsync(buffer);

				result.SetSuccess();
			}
			catch (CharacteristicReadException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
            }
			catch (InvalidOperationException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
			}

            return result;
		}

		/// <inheritdoc />
		async Task<AOResult<int>> ISamplesRateInfoService.GetSamplingRateAsync()
		{
			var result = new AOResult<int>();
			if (!IsDeviceConnected())
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
                return result;
			}

			try
			{
				var buffer = await ReadCharacteristicValueAsync();
				if (buffer.Length < (Math.Max(SamplingRateLowIndex, SamplingRateHighIndex) + 1))
				{
				    result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
				}

				int low = buffer[SamplingRateLowIndex];
				int high = buffer[SamplingRateHighIndex];

				int samplingRate = high << 8 | low;

				result.SetSuccess(samplingRate);
			}
			catch (CharacteristicReadException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
            }
			catch (InvalidOperationException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
			}

            return result;
		}

		/// <inheritdoc />
		async Task<AOResult> ISamplesRateInfoService.SetSamplingRateAsync(int aSamplingRate)
		{
			var result = new AOResult();
			if (!IsDeviceConnected())
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
                return result;
			}

			try
			{
				var buffer = await ReadCharacteristicValueAsync();
				if (buffer.Length < (Math.Max(SamplingRateLowIndex, SamplingRateHighIndex) + 1))
				{
				    result.SetFailure($"{(int)SignalRetrievalErrors.InsufficientBufferLength}");
                    return result;
				}

				aSamplingRate = (aSamplingRate % short.MaxValue);
				buffer[SamplingRateLowIndex] = (byte) aSamplingRate;
				buffer[SamplingRateHighIndex] = (byte) (aSamplingRate >> 8);

				await WriteCharacteristicValueAsync(buffer);

				result.SetSuccess();

                SamplingRateChanged?.Invoke(this, aSamplingRate);
			}
			catch (CharacteristicReadException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.CharacteristicReadException}");
            }
			catch (InvalidOperationException e)
			{
			    result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
			}

            return result;
		}

        /// <inheritdoc/>
        async Task<AOResult> ISamplesRateInfoService.SetDataAsync(string aData)
	    {
	        var result = new AOResult();
            result.SetSuccess();

	        if (!IsDeviceConnected())
	        {
	            result.SetFailure($"{(int)SignalRetrievalErrors.DeviceNotConnected}");
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
	            result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
            }

	        return result;
        }

	    /// <inheritdoc />
	    bool ISamplesRateInfoService.IsValidDataToWrite(string aData)
	    {
	        return aData.CanConvertToByteArrayOfSize();
        }

        /// <inheritdoc />
        public event EventHandler<int> SamplingRateChanged;

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

		#endregion
	}
}
