using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services.Bluetooth.Fake
{
    /// <summary>
    /// The fake implementation of the core BLE device for testing/development purposes.
    /// </summary>
    public sealed class FakeDevice : DeviceBase
    {
        /// <summary>
        /// Gets/sets fake services.
        /// </summary>
        public IReadOnlyList<IService> Services { get; set; }

        /// <inheritdoc />
        public FakeDevice(IAdapter aAdapter, string aName)
            : base(aAdapter)
        {
            Services = new IService[0];
            AdvertisementRecords = new List<AdvertisementRecord>(0);

            Id = Guid.NewGuid();
            Name = aName;
        }

        /// <inheritdoc />
        public override async Task<bool> UpdateRssiAsync()
        {
            await Task.Delay(5);

            Rssi = 100;

            return true;
        }

        /// <inheritdoc />
        public override object NativeDevice => this;

        /// <inheritdoc />
        protected override DeviceState GetState()
        {
            return DeviceState.Connected;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IService>> GetServicesNativeAsync()
        {
            await Task.Delay(10);

            return new List<IService>(Services);
        }

        /// <inheritdoc />
        protected override async Task<int> RequestMtuNativeAsync(int aRequestValue)
        {
            await Task.Delay(30);

            // assume, MTU size is agreed.
            return aRequestValue;
        }

        /// <inheritdoc />
        protected override bool UpdateConnectionIntervalNative(ConnectionInterval aInterval)
        {
            return true;
        }
    }
}