using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services
{
	public interface IDeviceDataTransferService
	{
		Task WriteDataAsync(IDevice device, string data);
		Task ReadDataAsync(IDevice device);
	}
}
