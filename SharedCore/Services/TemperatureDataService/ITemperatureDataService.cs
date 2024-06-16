using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using SharedCore.Models;

namespace SharedCore.Services.TemperatureDataService
{
    public interface ITemperatureDataService
    {
        Task<AOResult<string>> GetInfoAsync(IDevice aDevice);

        /// <summary>
        /// Gets the latest actual value of temperature.
        /// </summary>
        int TemperatureActualValue { get; }

        /// <summary>
        /// The event invoked when the temperature value changes.
        /// </summary>
        event EventHandler<int> OnTemperatureChanged;

        /// <summary>
        /// Gets latest actual value of the battery level.
        /// </summary>
        int BatteryActualValue { get; }

        /// <summary>
        /// The event invoked when the battery level changes.
        /// </summary>
        event EventHandler<int> OnBatteryLevelChanged;

        /// <summary>
        /// Gets latest actual value of sync ratio.
        /// </summary>
        double SynchRatioActualValue { get; }

        /// <summary>
        /// Gets whether BLE device in erase mode or not.
        /// </summary>
        bool IsEraseModeActive { get; }

        /// <summary>
        /// The event invoked when the erase mode changes.
        /// </summary>
        event EventHandler<bool> OnEraseModeChanged;

        /// <summary>
        /// Gets whether BLE device in sync mode or not.
        /// </summary>
        bool IsSyncModeActive { get; }

        /// <summary>
        /// The event invoked when the sync mode changes.
        /// </summary>
        event EventHandler<bool> OnSyncModeChanged;

        /// <summary>
        /// Subscribes to temperature and battery changes.
        /// </summary>
        /// <param name="aDevice"> The BLE device to subscribe to. </param>
        Task<AOResult> SubscribeChangesAsync(IDevice aDevice);

        /// <summary>
        /// Unsubscribes from temperature and battery changes.
        /// </summary>
        Task UnsubscribeChangeAsync();
    }
}