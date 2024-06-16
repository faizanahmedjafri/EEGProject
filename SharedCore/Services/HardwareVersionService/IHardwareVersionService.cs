using System;
using System.Threading.Tasks;
using SharedCore.Models;

namespace SharedCore.Services
{
    public interface IHardwareVersionService
    {
        Task<AOResult<string>> GetHardwareVersionAsync();
    }
}
