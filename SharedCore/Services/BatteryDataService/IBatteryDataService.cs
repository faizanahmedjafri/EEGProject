using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services
{
    [Obsolete]
    public interface IBatteryDataService
    {
        Task<bool> SubscribeBatteryLevelChangeAsync(IDevice aDevice);
        Task UnSubscribeBatteryLevelChangeAsync();

        /// <summary>
        /// The event invoked when the battery level changes.
        /// </summary>
        event EventHandler<int> OnBatteryLevelChanged;

        /// <summary>
        /// Gets latest actual value of the battery level.
        /// </summary>
        int BatteryActualValue { get; }
    }
}