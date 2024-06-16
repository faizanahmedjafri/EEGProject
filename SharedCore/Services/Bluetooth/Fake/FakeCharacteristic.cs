using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace SharedCore.Services.Bluetooth.Fake
{
    /// <summary>
    /// The fake implementation for testing/developing purposes.
    /// </summary>
    public class FakeCharacteristic : CharacteristicBase
    {
        private byte[] iValue = new byte[0];

        /// <summary>
        /// Gets/sets fake descriptors.
        /// </summary>
        public IReadOnlyList<IDescriptor> Descriptors { get; set; }

        /// <inheritdoc />
        public FakeCharacteristic(IService aService, Guid aUuid, 
            CharacteristicPropertyType aProperties)
            : base(aService)
        {
            Id = aUuid;
            Properties = aProperties;
            Descriptors = new IDescriptor[0];
        }

        /// <inheritdoc />
        protected override async Task<IList<IDescriptor>> GetDescriptorsNativeAsync()
        {
            await Task.Delay(10);

            return new List<IDescriptor>(Descriptors);
        }

        /// <inheritdoc />
        protected override async Task<byte[]> ReadNativeAsync()
        {
            await Task.Delay(10);

            return iValue;
        }

        /// <inheritdoc />
        protected override async Task<bool> WriteNativeAsync(byte[] data, CharacteristicWriteType writeType)
        {
            await Task.Delay(10);

            iValue = data;

            return true;
        }

        /// <inheritdoc />
        protected override async Task StartUpdatesNativeAsync()
        {
            await Task.Delay(10);

            // do nothing actually
        }

        /// <inheritdoc />
        protected override async Task StopUpdatesNativeAsync()
        {
            await Task.Delay(10);

            // do nothing actually
        }

        /// <inheritdoc />
        public override Guid Id { get; }

        /// <inheritdoc />
        public override string Uuid => Id.ToString();

        /// <inheritdoc />
        public override byte[] Value => iValue;

        /// <inheritdoc />
        public override CharacteristicPropertyType Properties { get; }

        /// <inheritdoc />
        public override event EventHandler<CharacteristicUpdatedEventArgs> ValueUpdated;

        /// <summary>
        /// Sets the value directly.
        /// </summary>
        public void SetValue(byte[] aValue)
        {
            iValue = aValue;

            ValueUpdated?.Invoke(this, new CharacteristicUpdatedEventArgs(this));
        }
    }
}
