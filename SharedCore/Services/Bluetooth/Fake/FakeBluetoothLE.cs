using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services.Bluetooth.Fake
{
    /// <summary>
    /// Fake implementation of the core BLE services for testing/development purposes.
    /// </summary>
    public sealed class FakeBluetoothLE : BleImplementationBase
    {
        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        public FakeBluetoothLE()
        {
            Initialize();
        }

        /// <inheritdoc />
        protected override void InitializeNative()
        {
            // nothing to do
        }

        /// <inheritdoc />
        protected override BluetoothState GetInitialStateNative()
        {
            return BluetoothState.On;
        }

        /// <inheritdoc />
        protected override IAdapter CreateNativeAdapter()
        {
            return new FakeBleAdapter();
        }
    }
}