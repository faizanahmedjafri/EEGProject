using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services.Bluetooth.Fake
{
    /// <summary>
    /// The fake implementation for testing/developing purposes.
    /// </summary>
    public sealed class FakeDescriptor : DescriptorBase
    {
        private byte[] iValue = new byte[0];

        /// <inheritdoc />
        public FakeDescriptor(Guid aId, ICharacteristic aCharacteristic)
            : base(aCharacteristic)
        {
            Id = aId;
        }

        /// <inheritdoc />
        protected override async Task<byte[]> ReadNativeAsync()
        {
            await Task.Delay(10);

            return iValue;
        }

        /// <inheritdoc />
        protected override async Task WriteNativeAsync(byte[] data)
        {
            await Task.Delay(10);

            iValue = (byte[]) data.Clone();
        }

        /// <inheritdoc />
        public override Guid Id { get; }

        /// <inheritdoc />
        public override byte[] Value => iValue;

        /// <summary>
        /// Sets the value directly.
        /// </summary>
        public void SetValue(byte[] aValue)
        {
            iValue = aValue;
        }
    }
}