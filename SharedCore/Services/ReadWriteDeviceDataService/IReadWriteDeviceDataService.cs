using System;
using System.Threading.Tasks;

namespace SharedCore.Services
{
    public interface IReadWriteDeviceDataService
    {
        Task<bool> SubscribeUpdateValues();
    }
}
