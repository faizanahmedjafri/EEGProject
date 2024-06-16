using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Diagnostics;
using System.Linq;
using Plugin.BLE.Abstractions.Exceptions;
using SharedCore.Models;
using SharedCore.Services.Charts;
using SharedCore.Services.RateInfoService;

namespace SharedCore.Services
{
	/// <summary>
	/// Reads signal samples from a ble device and its services.
	/// </summary>
	public class ChartService : IChartService
	{
        private const int TryGetServiceCounter = 200;

        private readonly Guid _serviceGuid = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
		private readonly Guid _chartsDataCharacterGuid = new Guid("0000fff4-0000-1000-8000-00805f9b34fb");
		private ICharacteristic _chartsDataCharacter;
		private readonly Stopwatch _packageReceiveTime = new Stopwatch();

		private readonly IBlueToothService _blueToothService;
		private readonly IBiopotInfoChartsService _biopotInfoService;
        private readonly ISamplesRateInfoService _biopotSampleRateService;
        private readonly ConcurrentQueue<IChartDataSnapshot> _pendingDataQueue;

		private BiopotSignalParser _signalParser;
        private BiopotGenericInfo _deviceInfo = new BiopotGenericInfo();

        private int _deviceSamplingRate;

        public ChartService(IBlueToothService blueToothService,
			IBiopotInfoChartsService biopotInfoService,
            ISamplesRateInfoService aBiopotSampleRateService)
		{
			_blueToothService = blueToothService;
			_biopotInfoService = biopotInfoService;
            _biopotSampleRateService = aBiopotSampleRateService;
            _pendingDataQueue = new ConcurrentQueue<IChartDataSnapshot>();
        }


        #region -- IChartService --

        /// <inheritdoc />
        IReadOnlyList<IChartDataSnapshot> IChartService.TryTakePendingData()
        {
            return TakePendingSnapshots().ToArray();
        }

        /// <inheritdoc />
        async Task<bool> IChartService.SubscribeChartsDataChangeAsync()
        {
            IDevice newDevice = _blueToothService.CurrentDevice;

            if (Equals(_chartsDataCharacter?.Service.Device, newDevice))
            {
                // already subscribe to the device, nothing to do
                return true;
            }

            await (this as IChartService).UnSubscribeChartsDataChangeAsync();

            try
            {
                IService chartService = null;
                await newDevice.RequestMtuAsync(251);
                int counter = 0;

                while (chartService == null && counter != TryGetServiceCounter) //try to get characteristics N times
                {
                    chartService = await newDevice.GetServiceAsync(_serviceGuid);
                    counter++;
                }

                if (chartService == null)
                {
                    return false;
                }

                _chartsDataCharacter = await chartService.GetCharacteristicAsync(_chartsDataCharacterGuid);

                // read device information about available channels and other info
                var infoResult = await _biopotInfoService.GetDeviceInfoAsync();
                if (!infoResult.IsSuccess)
                {
                    return false;
                }

                // read current sampling rate
                var sampleRateResult = await _biopotSampleRateService.GetSamplingRateAsync();
                if (!sampleRateResult.IsSuccess)
                {
                    return false;
                }

                DeviceInfo = infoResult.Result;
                DeviceSamplingRate = sampleRateResult.Result;
                _signalParser = new BiopotSignalParser(DeviceInfo);

                _chartsDataCharacter.ValueUpdated += OnChartsDataUpdated;
                await _chartsDataCharacter?.StartUpdatesAsync();

                _biopotSampleRateService.SamplingRateChanged += OnSampleRateChanged;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(IChartService.SubscribeChartsDataChangeAsync)}:exception: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        async Task IChartService.UnSubscribeChartsDataChangeAsync()
        {
            _biopotSampleRateService.SamplingRateChanged -= OnSampleRateChanged;

            if (_chartsDataCharacter != null)
            {
                _chartsDataCharacter.ValueUpdated -= OnChartsDataUpdated;

                try
                {
                    await _chartsDataCharacter.StopUpdatesAsync();
                }
                catch (CharacteristicReadException e)
                {
                    Debug.WriteLine($"{nameof(IChartService.UnSubscribeChartsDataChangeAsync)}:{e}");
                }

                _chartsDataCharacter = null;
            }
        }

        /// <inheritdoc />
        public BiopotGenericInfo DeviceInfo
        {
            get => _deviceInfo;
            private set
            {
                _deviceInfo = value;
                DeviceInfoUpdated?.Invoke(this, _deviceInfo);
            }
        }

        /// <inheritdoc />
        public event EventHandler<BiopotGenericInfo> DeviceInfoUpdated;

        /// <inheritdoc />
        public int DeviceSamplingRate
        {
            get => _deviceSamplingRate;
            private set
            {
                if (_deviceSamplingRate != value)
                {
                    _deviceSamplingRate = value;
                    DeviceSamplingRateChanged?.Invoke(this, _deviceSamplingRate);
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<int> DeviceSamplingRateChanged;

        #endregion

        /// <summary>
        /// Handles that sampling rate for currently connected device has changed.
        /// </summary>
        /// <param name="aSender"></param>
        /// <param name="aNewSamplingRate"></param>
        private void OnSampleRateChanged(object aSender, int aNewSamplingRate)
        {
            DeviceSamplingRate = aNewSamplingRate;
        }

        /// <summary>
        /// Takes all currently pending snapshots in the queue.
        /// </summary>
        /// <returns>enumerable with the snapshots</returns>
        private IEnumerable<IChartDataSnapshot> TakePendingSnapshots()
        {
            while (_pendingDataQueue.TryDequeue(out var snapshot))
            {
                yield return snapshot;
            }
        }

        private void OnChartsDataUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            _packageReceiveTime.Stop();

            Performance.Performance.Start(out var reference);

            byte[] buffer = e.Characteristic.Value;

            if (_signalParser.TryParse(buffer))
            {
                _pendingDataQueue.Enqueue(new ChartDataSnapshot
                {
                    DataSamples = _signalParser.SpdData,
                    AccelerometerSamples = _signalParser.AccelerometerData,
                    BioImpedanceSamples = _signalParser.BioImpedanceData,
                    ElapsedTime = _packageReceiveTime.Elapsed,
                    PacketTimestamp = _signalParser.Timestamp,
                    RawBuffer = buffer,
                });
            }

            Performance.Performance.Stop(reference);

            _packageReceiveTime.Restart();
        }

        #region Chart Snapshot

        /// <summary>
        /// The implementation of <see cref="IChartDataSnapshot"/>.
        /// </summary>
        private class ChartDataSnapshot : BaseDisposable, IChartDataSnapshot
        {
            /// <inheritdoc />
            public short[,] DataSamples { get; set; }

            /// <inheritdoc />
            public short[] AccelerometerSamples { get; set; }

            /// <inheritdoc />
            public short[] BioImpedanceSamples { get; set; }

            /// <inheritdoc />
            public byte[] RawBuffer { get; set; }

            /// <inheritdoc />
            public int PacketTimestamp { get; set; }

            /// <inheritdoc />
            public TimeSpan ElapsedTime { get; set; }

            /// <inheritdoc />
            protected override void DisposeManagedObjects()
            {
                // TODO implement array/object pool recycle
            }
        }

        #endregion
    }
}
