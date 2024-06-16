using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace SharedCore.Services.Bluetooth.Fake
{
    /// <summary>
    /// The fake characteristic for testing purposes, which can generate updates.
    /// </summary>
    public class FakeLiveCharacteristic : FakeCharacteristic
    {
        /// <inheritdoc />
        public FakeLiveCharacteristic(IService aService, Guid aUuid,
            CharacteristicPropertyType aProperties)
            : base(aService, aUuid, aProperties)
        {
        }

        /// <summary>
        /// Gets/sets the value update interval.
        /// </summary>
        public TimeSpan UpdateInterval { get; set; }

        /// <summary>
        /// Sets/gets the values to use for live updates.
        /// </summary>
        public IReadOnlyCollection<byte[]> LiveValues { get; set; } = new byte[0][];

        /// <inheritdoc />
        protected override async Task StartUpdatesNativeAsync()
        {
            await Task.Delay(10);
            await StopUpdatesNativeAsync();

            var _ = Task.Run(async () =>
            {
                using (var tokenSource = new CancellationTokenSource())
                {
                    // ReSharper disable once AccessToDisposedClosure
                    Action callback = () => { tokenSource.Cancel(); };

                    StopUpdatesRequested += callback;

                    try
                    {
                        await RunLiveCharacteristic(LiveValues.ToList(), UpdateInterval, tokenSource.Token);
                    }
                    finally
                    {
                        StopUpdatesRequested -= callback;
                    }
                }
            });
        }

        /// <inheritdoc />
        protected override async Task StopUpdatesNativeAsync()
        {
            StopUpdatesRequested?.Invoke();

            await Task.Delay(10);
        }

        /// <summary>
        /// The event for internal use to stop updates.
        /// </summary>
        private event Action StopUpdatesRequested;

        /// <summary>
        /// Runs the characteristic live updates using the given values with the given update interval.
        /// </summary>
        /// <param name="aValues">The values collection to run with.</param>
        /// <param name="aInterval">The update interval.</param>
        /// <param name="aToken">The cancellation token.</param>
        private async Task RunLiveCharacteristic(IReadOnlyList<byte[]> aValues, TimeSpan aInterval,
            CancellationToken aToken)
        {
            while (!aToken.IsCancellationRequested)
            {
                for (int i = 0; i < aValues.Count && !aToken.IsCancellationRequested; i++)
                {
                    await Task.Delay(aInterval, aToken);

                    SetValue(aValues[i]);
                }
            }
        }
    }
}