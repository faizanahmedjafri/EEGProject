using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedCore.Models;
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics.Contracts;
using System.Text;

namespace SharedCore.Services
{
	public class DeviceDataTransferService : IDeviceDataTransferService
	{
		private IService _service;

		public DeviceDataTransferService()
		{

		}

		#region -- IBlueToothService implementation --

		public async Task ReadDataAsync(IDevice device)
		{
			try
			{
				if (_service == null)
				{
					await PrepareChannelAsync(device);
				}

				var characteristic = await _service.GetCharacteristicAsync(Guid.Parse("BEB89473-05FE-41A0-9896-2C082660F19A"));
				byte[] data = await characteristic.ReadAsync();
			}
			catch (Exception ex)
			{
				//UserDialogs.Instance.Toast(ex.Message);
			}
		}

		public async Task WriteDataAsync(IDevice device, string data)
		{
			try
			{
				if (_service == null)
				{
					await PrepareChannelAsync(device);
				}

				var characteristic = await _service.GetCharacteristicAsync(Guid.Parse("BEB89473-05FE-41A0-9896-2C082660F19A"));
				byte[] bytes = Encoding.UTF8.GetBytes(data);
				await characteristic.WriteAsync(bytes);
			}
			catch (Exception ex)
			{
				//UserDialogs.Instance.Toast(ex.Message);
			}
		}

		public Task WriteDataAsync(IDevice device)
		{
			throw new NotImplementedException();
		}


		#endregion

		#region -- Private helpers --

		private async Task PrepareChannelAsync(IDevice device)
		{
			try
			{
				int count = 0;
				do
				{
					if (count > 200)
					{
						throw new Exception("Number of attempts to create service is failed!");
					}
					count++;
					IList<IService> services = null;

					var cancelTokenSource = new CancellationTokenSource();
					CancellationToken cancelToken = cancelTokenSource.Token;
					var task = device.GetServicesAsync(cancelToken);
					var delay = Task.Delay(5000);
					await Task.WhenAny(task, delay);

					if (task.Status != TaskStatus.RanToCompletion)
						cancelTokenSource?.Cancel();
					else
						services = task.Result;

					if (services == null || services.Count == 0)
					{
						throw new Exception("Services not found(");
					}

					_service = await device.GetServiceAsync(Guid.Parse("C9282723-4680-491B-A904-C066FA81061F"));
				} while (_service == null);


				IList<ICharacteristic> characteristics = null;
				characteristics = await _service.GetCharacteristicsAsync();
				if (characteristics == null || characteristics.Count == 0)
				{
					throw new Exception("Characteristics not found(");
				}
			}
			catch (Exception ex)
			{
				//UserDialogs.Instance.Toast(ex.Message);
			}
		}

		#endregion
	}

}
