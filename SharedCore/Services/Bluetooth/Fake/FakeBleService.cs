using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services.Bluetooth.Fake
{
    /// <summary>
    /// The fake implementation for testing/developing purposes.
    /// </summary>
    public sealed class FakeBleService : ServiceBase
    {
        /// <inheritdoc />
        public FakeBleService(IDevice aDevice, Guid aUuid)
            : base(aDevice)
        {
            Id = aUuid;
            Characteristics = new ICharacteristic[0];
        }

        /// <inheritdoc />
        protected override async Task<IList<ICharacteristic>> GetCharacteristicsNativeAsync()
        {
            await Task.Delay(10);

            return new List<ICharacteristic>(Characteristics);
        }

        /// <inheritdoc />
        public override Guid Id { get; }

        /// <inheritdoc />
        public override bool IsPrimary => true;

        /// <summary>
        /// Sets/gets fake characteristics.
        /// </summary>
        public IReadOnlyList<ICharacteristic> Characteristics { get; set; }
    }
}