using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using biopot.Enums;
using SharedCore.Services;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using biopot.Models;
using NeoSmart.AsyncLock;
using SharedCore.Enums;
using SharedCore.Models;
using SharedCore.Services.Characteristic3Service;
using SharedCore.Services.Charts;
using SharedCore.Services.Performance;

namespace biopot.Services.Charts
{
	/// <summary>
	/// The class contains all chart data and processes it before passing it to the charts.
	/// </summary>
	public class ChartManagerService : IChartManagerService
    {
        /// <remarks>Fastest interval for 2000 SPS is 12ms.</remarks>
        private static readonly TimeSpan TimerRefreshInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan TimerLockRetryInterval = TimeSpan.FromMilliseconds(10);

		private readonly IChartService _chartService;
        private readonly ICharacteristic3Service _characteristic3Service;
        private readonly IBiobotDataForChartsService _biopotChartsService;
        private readonly ISignalFilterService _filterService;
		private readonly System.Timers.Timer _timer;
        private SpinLock _timerLock = new SpinLock();

		private List<EDeviceType> _providedServices;
		private IReadOnlyList<ISignalFilter> _signalFilters;

		private ConcurrentDictionary<int, IList<double[]>> _channelsData;
		private ConcurrentDictionary<int, MontageDataHolder> _montageData;

        private ConcurrentDictionary<int, double[]> _accelerometerData;
        private ConcurrentDictionary<int, double[]> _bioImpedanceData;

		private ConcurrentQueue<double> _chSPSData;

		private BiopotGenericInfo _lastBiopotInfo = new BiopotGenericInfo();
        private int _currentSamplingRate = 50; // for small initial buffer

        private readonly AsyncLock _captureModeLock = new AsyncLock();
        private CaptureMode _activeCaptureMode = CaptureMode.CaptureNone;

        private int _lastReceivedPacketTimestamp = int.MaxValue;

        /// <summary>
        /// Gets maximum number of samples to hold per channel.
        /// </summary>
        private int PointPerGraph => _currentSamplingRate * Constants.Charts.MaximumBufferedSeconds;

        public ChartManagerService(IChartService chartService,
            ICharacteristic3Service characteristic3Service,
            IBiobotDataForChartsService biopotChartsService,
            ISignalFilterService filterService)
		{
			_chartService = chartService;
            _characteristic3Service = characteristic3Service;
            _biopotChartsService = biopotChartsService;
            _filterService = filterService;

			_signalFilters = _filterService.GetFilters(SignalFilterType.NoFilter, 0);
			CurrentSignalFilterType = SignalFilterType.NoFilter;

			ResetBuffers();

			_timer = new System.Timers.Timer(TimerRefreshInterval.TotalMilliseconds);
            _timer.Elapsed += (aSender, aArgs) =>
            {
                bool isLockTaken = false;
                _timerLock.TryEnter(TimerLockRetryInterval, ref isLockTaken);
                if (isLockTaken)
                {
                    try
                    {
                        HandleElapsedEventHandler();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"{nameof(HandleElapsedEventHandler)}: exception: {e}");
                    }
                    finally
                    {
                        _timerLock.Exit();
                    }
                }
                else
                {
                    // this timer's tick is ignored, and next will try try to process new chart data
                }
            };
            _timer.Enabled = false;
		}

        /// <summary>
        /// Resets buffers to match biopot device configuration
        /// </summary>
        private void ResetBuffers()
		{
			_providedServices = new List<EDeviceType>
			{
				EDeviceType.EEGorEMG
			};
			if (_lastBiopotInfo.IsBioImpedancePresent)
			{
				_providedServices.Add(EDeviceType.BioImpedance);
			}

			if (_lastBiopotInfo.IsAccelerometerPresent)
			{
				_providedServices.Add(EDeviceType.Accelerometer);
			}

            _channelsData = new ConcurrentDictionary<int, IList<double[]>>();
            _montageData = new ConcurrentDictionary<int, MontageDataHolder>();

            _accelerometerData = new ConcurrentDictionary<int, double[]>();
            _bioImpedanceData = new ConcurrentDictionary<int, double[]>();

            _chSPSData = new ConcurrentQueue<double>(Enumerable.Repeat<double>(0, 3600));

            ResetChannelData();
            ResetAccelerometerAndBioImpedanceData();
        }

        /// <summary>
        /// Converts the sample value to micro-volts.
        /// </summary>
        /// <param name="aValue">The sample value received from a device.</param>
        /// <returns>value of micro-volts, corresponding to the sample value.</returns>
        private static double MapSampleValueToMicroVolts(short aValue)
        {
            // 0.000195 mV * 10^(-3) = 0.195uV
            return 0.195d * aValue;
        }

        private double MapSampleValueToGravity(short aValue)
        {

            double bitResolution = 0;
            switch (_lastBiopotInfo.AccelerometerMode)
            {
                case 1:
                    //2g HR
                    bitResolution = 0.000976563d;
                    break;
                case 2:
                    //4g HR
                    bitResolution = 0.001953125d;
                    break;
                case 3:
                    //8g HR
                    bitResolution = 0.00390625d;
                    break;
                case 4:
                    //16g HR
                    bitResolution = 0.0078125d;
                    break;
                default:
                    break;
            }
            return aValue * bitResolution;
        }

        /// <summary>
        /// Resets channel data, accounting channels count and filters count.
        /// </summary>
        private void ResetChannelData()
        {
            // number of channel data is +1 of number of signal filters
            int requiredDataCountPerChannel = _signalFilters.Count + 1;
            foreach (var channelId in Enumerable.Range(1, (int) _lastBiopotInfo.ChannelsNumber))
            {
                IList<double[]> channelData;
                if (_channelsData.TryGetValue(channelId, out channelData))
                {
                    // alter the channel data, if needed
                    lock (channelData)
                    {
                        if (channelData.Count > requiredDataCountPerChannel)
                        {
                            // remove unneeded channel data
                            var toRemove = channelData.Skip(requiredDataCountPerChannel).ToArray();
                            foreach (var channelDataToRemove in toRemove)
                            {
                                channelData.Remove(channelDataToRemove);
                            }
                        }
                        else if (channelData.Count < requiredDataCountPerChannel)
                        {
                            // add more channel data as duplicate of last one
                            var filteredChannelData = channelData.Last();
                            int countToAdd = requiredDataCountPerChannel - channelData.Count;
                            for (int i = 0; i < countToAdd; i++)
                            {
                                channelData.Add((double[]) filteredChannelData.Clone());
                            }
                        }
                    }
                }
                else
                {
                    channelData = Enumerable.Range(0, requiredDataCountPerChannel)
                        .Select(_ => new double[PointPerGraph])
                        .ToList();
                }

                _channelsData[channelId] = channelData;
            }
        }

        /// <summary>
        /// Resets accelerometer and bio-impedance data based on its presents and channel number.
        /// </summary>
        private void ResetAccelerometerAndBioImpedanceData()
        {
            // accelerometer
            if (_lastBiopotInfo.IsAccelerometerPresent)
            {
                foreach (var channelId in
                    Enumerable.Range(1, (int)_lastBiopotInfo.AccelerometerChannelNumber))
                {
                    _accelerometerData[channelId] = new double[PointPerGraph];
                }
            }
            else
            {
                _accelerometerData.Clear();
            }

            // bio-impedance
            if (_lastBiopotInfo.IsBioImpedancePresent)
            {
                foreach (var channelId in
                    Enumerable.Range(1, (int) _lastBiopotInfo.BioImpedanceChannelNumber))
                {
                    _bioImpedanceData[channelId] = new double[PointPerGraph];
                }
            }
            else
            {
                _bioImpedanceData.Clear();
            }
        }

        /// <summary>
        /// Expands or shrinks existing channel buffers to hold
        /// required number of samples for changed sampling rate.
        /// </summary>
        private void ExpandOrShrinkAllBuffers()
        {
            Action[] parallelActions =
            {
                ExpandOrShrinkChannelBuffers,
                ExpandOrShrinkExistingMontageBuffers,
                ExpandOrShrinkAccelerometerBuffers,
                ExpandOrShrinkBioImpedanceBuffers,
            };

            Parallel.ForEach(parallelActions, aAction => aAction());
        }

        /// <summary>
        /// Expands or shrinks channel buffers.
        /// </summary>
        private void ExpandOrShrinkChannelBuffers()
        {
            var requiredSamplesCount = PointPerGraph;

            Parallel.ForEach(Partitioner.Create(_channelsData), aPair =>
            {
                var channelsData = aPair.Value;
                lock (channelsData)
                {
                    for (int i = 0; i < channelsData.Count; i++)
                    {
                        var channelData = channelsData[i];
                        channelsData[i] = ExpandOrShrinkChannelBuffer(channelData, requiredSamplesCount);
                    }
                }
            });
        }

        /// <summary>
        /// Expands or shrinks non-empty montage buffers.
        /// </summary>
        private void ExpandOrShrinkExistingMontageBuffers()
        {
            var requiredSamplesCount = PointPerGraph;

            var nonEmptyMontages = _montageData.Where(x => x.Value.Buffer.Length != 0);
            Parallel.ForEach(Partitioner.Create(nonEmptyMontages),
                aPair =>
                {
                    var newBuffer = ExpandOrShrinkChannelBuffer(aPair.Value.Buffer, requiredSamplesCount);
                    // save the buffer into the holder directly
                    aPair.Value.Buffer = newBuffer;
                });
        }

        /// <summary>
        /// Expands or shrinks bio-impedance buffers.
        /// </summary>
        private void ExpandOrShrinkBioImpedanceBuffers()
        {
            var requiredSamplesCount = PointPerGraph;

            Parallel.ForEach(Partitioner.Create(_bioImpedanceData), aPair =>
            {
                var newBuffer = ExpandOrShrinkChannelBuffer(aPair.Value, requiredSamplesCount);
                _bioImpedanceData[aPair.Key] = newBuffer;
            });
        }

        /// <summary>
        /// Expands or shrinks accelerometer buffers.
        /// </summary>
        private void ExpandOrShrinkAccelerometerBuffers()
        {
            var requiredSamplesCount = PointPerGraph;

            Parallel.ForEach(Partitioner.Create(_accelerometerData), aPair =>
            {
                var newBuffer = ExpandOrShrinkChannelBuffer(aPair.Value, requiredSamplesCount);
                _accelerometerData[aPair.Key] = newBuffer;
            });
        }

        /// <summary>
        /// Expands/shrinks a channel buffer, represented by array.
        /// </summary>
        /// <typeparam name="T">The array item type.</typeparam>
        /// <param name="aBuffer">The buffer to expand/shrink.</param>
        /// <param name="aNewLength">New buffer length.</param>
        /// <returns>created buffer of new size.</returns>
        private static T[] ExpandOrShrinkChannelBuffer<T>(T[] aBuffer, int aNewLength)
        {
            var newBuffer = aBuffer;
            if (aBuffer.Length < aNewLength)
            {
                // prepend from left
                newBuffer = new T[aNewLength];

                int targetOffset = aNewLength - aBuffer.Length;
                Array.Copy(aBuffer, 0,
                    newBuffer, targetOffset, aBuffer.Length);
            }
            else if (aBuffer.Length > aNewLength)
            {
                // truncate from left
                newBuffer = new T[aNewLength];

                int sourceOffset = aBuffer.Length - aNewLength;

                Array.Copy(aBuffer, sourceOffset,
                    newBuffer, 0, aNewLength);
            }

            return newBuffer;
        }

        private void HandleElapsedEventHandler()
        {
            Performance.Start(out var reference, $"{nameof(HandleElapsedEventHandler)}");

            var newBiopotInfo = _chartService.DeviceInfo;
            if (!_lastBiopotInfo.Equals(newBiopotInfo))
            {
                _lastBiopotInfo = newBiopotInfo;
                _currentSamplingRate = _chartService.DeviceSamplingRate;

                // reset internal buffers, because biopot configuration has changed
                Performance.Start(reference, $"{nameof(ResetBuffers)}");
                ResetBuffers();
                Performance.Stop(reference, $"{nameof(ResetBuffers)}");
            }
            else
            {
                var newSamplingRate = _chartService.DeviceSamplingRate;
                if (_currentSamplingRate != newSamplingRate)
                {
                    _currentSamplingRate = newSamplingRate;

                    Performance.Start(reference, $"{nameof(ExpandOrShrinkAllBuffers)}");
                    ExpandOrShrinkAllBuffers();
                    Performance.Stop(reference, $"{nameof(ExpandOrShrinkAllBuffers)}");
                }
            }


            Performance.Start(reference, $"{nameof(IChartService.TryTakePendingData)}");
            var snapshots = _chartService.TryTakePendingData();
            Performance.Stop(reference, $"{nameof(IChartService.TryTakePendingData)}");
            try
            {
                Performance.Start(reference, $"{nameof(ProcessDataSnapshots)}");
                ProcessDataSnapshots(snapshots);
                Performance.Stop(reference, $"{nameof(ProcessDataSnapshots)}");
            }
            finally
            {
                // ensure the snapshots are disposed/recycled
                BaseDisposable.SafeDispose(snapshots);
            }

            Performance.Stop(reference, $"{nameof(HandleElapsedEventHandler)}");
        }

        /// <summary>
        /// Processes the data snapshots.
        /// </summary>
        /// <param name="aSnapshots">The snapshots to process.</param>
        private void ProcessDataSnapshots(IReadOnlyCollection<IChartDataSnapshot> aSnapshots)
        {
            Performance.Start(out var reference);

            if (aSnapshots.Count > 0)
            {
                // necessary to do on new data received

                Performance.Start(reference, $"{nameof(DetectDataLoss)}");
                DetectDataLoss(aSnapshots);
                Performance.Stop(reference, $"{nameof(DetectDataLoss)}");

                Performance.Start(reference, "PrepareBuffers");
                var dataBuffers = aSnapshots.Select(x => x.DataSamples).ToArray();
                var bioImpedanceBuffers = aSnapshots.Select(x => x.BioImpedanceSamples).ToArray();
                var accelerometerBuffers = aSnapshots.Select(x => x.AccelerometerSamples).ToArray();
                var spsTimestamps = aSnapshots.Select(x => (long) x.ElapsedTime.TotalMilliseconds).ToArray();
                var rawBuffers = aSnapshots.Select(x => x.RawBuffer).ToArray();
                Performance.Stop(reference, "PrepareBuffers");


                Performance.Start(reference, $"{nameof(UpdateDataForSPS)}");
                UpdateDataForSPS(spsTimestamps);
                Performance.Stop(reference, $"{nameof(UpdateDataForSPS)}");

                // we don't actually care about precise thread-safety here,
                // but we might loose some data at some point in time in worst case.
                switch (_activeCaptureMode)
                {
                    case CaptureMode.CaptureEegEmg:
                    {
                        Performance.Start(reference, "ProcessEEG");

                        Performance.Start(reference, $"{nameof(UpdateData)}");
                        int newSamplesCount = UpdateData(dataBuffers);
                        Performance.Stop(reference, $"{nameof(UpdateData)}");

                        Performance.Start(reference, $"{nameof(UpdateExistingMontageData)}");
                        UpdateExistingMontageData(newSamplesCount);
                        Performance.Stop(reference, $"{nameof(UpdateExistingMontageData)}");

                        Performance.Start(reference, $"{nameof(UpdateNewMontageChannels)}");
                        UpdateNewMontageChannels();
                        Performance.Stop(reference, $"{nameof(UpdateNewMontageChannels)}");

                        Performance.Start(reference, $"{nameof(UpdateDataForAccelerometer)}");
                        UpdateDataForAccelerometer(accelerometerBuffers);
                        Performance.Stop(reference, $"{nameof(UpdateDataForAccelerometer)}");

                        Performance.Start(reference, $"{nameof(UpdateDataForBioImpedance)}");
                        UpdateDataForBioImpedance(bioImpedanceBuffers);
                        Performance.Stop(reference, $"{nameof(UpdateDataForBioImpedance)}");

                        Performance.Stop(reference, "ProcessEEG");

                        Performance.Start(reference, $"{nameof(DataLoaded)}");
                        DataLoaded?.Invoke(this, new BiopodDataEventArgs(rawBuffers));
                        Performance.Stop(reference, $"{nameof(DataLoaded)}");

                        break;
                    }

                    case CaptureMode.CaptureImpedance:
                    {
                        Performance.Start(reference, "ProcessImpedance");
                        var impedanceData = UpdateDataForImpedance(dataBuffers);
                        Performance.Stop(reference, "ProcessImpedance");

                        Performance.Start(reference, $"{nameof(ImpedanceDataLoaded)}");
                        if (impedanceData != null && impedanceData.Count > 0)
                        {
                            ImpedanceDataLoaded?.Invoke(this, new ImpedanceDataEventArgs(impedanceData));
                        }
                        Performance.Stop(reference, $"{nameof(ImpedanceDataLoaded)}");

                        break;
                    }
                }
            }
            else
            {
                // necessary to do on any tick, no matter if new data is present

                Performance.Start(reference, $"{nameof(UpdateNewMontageChannels)}");
                UpdateNewMontageChannels();
                Performance.Stop(reference, $"{nameof(UpdateNewMontageChannels)}");
            }

            Performance.Stop(reference);
        }

        /// <summary>
        /// Verifies timestamps of packets in their order, and logs info, if detects.
        /// </summary>
        private void DetectDataLoss(IReadOnlyCollection<IChartDataSnapshot> aSnapshots)
        {
            int lossCount = 0;
            foreach (var snapshot in aSnapshots)
            {
                var samplesCountPerChannel = snapshot.DataSamples.GetLength(1);
                var lostInterval = (snapshot.PacketTimestamp - _lastReceivedPacketTimestamp) - samplesCountPerChannel;
                if (lostInterval > 0)
                {
                    lossCount += lostInterval / samplesCountPerChannel;
                }

                _lastReceivedPacketTimestamp = snapshot.PacketTimestamp;
            }

            if (lossCount != 0)
            {
                Debug.WriteLine($"{nameof(DetectDataLoss)}: by timestamp, " +
                                $"loss count {lossCount}/{aSnapshots.Count} packets");
            }
        }

        /// <summary>
        /// Fetches impedance data from the buffers.
        /// </summary>
        /// <returns>result impedance data per channel ID, or null.</returns>
        private IReadOnlyDictionary<int, double> UpdateDataForImpedance(
            IReadOnlyCollection<short[,]> aDataBuffers)
        {
            var result = new Dictionary<int, double>();

            // the impedance is contained in the generic chart channels;
            // the impedance is just single latest sample per each channel for now,
            // thus no matter how many buffers we get, just take the last one.

            var spdSamples = aDataBuffers.LastOrDefault();
            if (spdSamples != null)
            {
                int channelsCount = spdSamples.GetLength(0);
                int samplesCountPerChannel = spdSamples.GetLength(1);

                foreach (var channelId in Enumerable.Range(1, channelsCount))
                {
                    int channelIndex = channelId - 1;

                    // take latest sample, no conversion is needed here
                    result[channelId] = spdSamples[channelIndex, samplesCountPerChannel - 1];
                }
            }

            return result;
        }

		private void UpdateDataForSPS(IReadOnlyCollection<long> aSpsData)
		{
            if (aSpsData.Count == 0)
            {
                return;
            }

			var average = aSpsData.Average();

			short spsValue = (short) (_lastBiopotInfo.SamplesPerChannelNumber / (average / 1000f));

			_chSPSData.TryDequeue(out _);
			_chSPSData.Enqueue(spsValue);
		}

        private void UpdateDataForBioImpedance(IReadOnlyCollection<short[]> aBioImpedanceBuffers)
        {
            foreach (var buffer in aBioImpedanceBuffers)
            {
                foreach (var entry in _bioImpedanceData)
                {
                    var channelData = Array.ConvertAll(buffer, MapSampleValueToMicroVolts);

                    // TODO ask why we just duplicate data to each channel for default 4 channels
                    AppendDataToChannelBuffer(entry.Value, channelData);
                }
            }
        }

        private void UpdateDataForAccelerometer(IReadOnlyCollection<short[]> aAccelerometerBuffers)
        {
            foreach (var buffer in aAccelerometerBuffers)
            {
                var channelsNumber = _lastBiopotInfo.AccelerometerChannelNumber;
                foreach (var entry in _accelerometerData)
                {
                    var channelId = entry.Key;
                    var channelIndex = channelId - 1;
                    uint sPC = _chartService.DeviceInfo.SamplesPerChannelNumber;
                    double[] channelBuffer = new double[sPC];
                    uint firstDuplicatesSize = sPC / 2 + sPC % 2;
                    uint secondDulicatesSize = sPC / 2;
                    int j = 0;
                    for (j = 0; j < firstDuplicatesSize; j++)
                    {
                        channelBuffer[j] = MapSampleValueToGravity(buffer[entry.Key-1]);
                    }
                    for(;j< firstDuplicatesSize + secondDulicatesSize; j++)
                    {
                        channelBuffer[j] = MapSampleValueToGravity(buffer[entry.Key-1 + 3]);
                    }
                    AppendDataToChannelBuffer(entry.Value, channelBuffer);
                    /*
                    var channelBuffer = buffer
                        .Where((sample, index) => 0 == (index - channelIndex) % channelsNumber)
                        .Select(MapSampleValueToGravity)
                        .ToArray();
                        */

                }
            }
        }

        /// <summary>
        /// Updates data of ally channels, using new buffers.
        /// </summary>
        /// <param name="aDataBuffers">The data buffers</param>
        /// <returns>count of new samples in a channel, equal for all channels.</returns>
        private int UpdateData(IReadOnlyCollection<short[,]> aDataBuffers)
        {
            Debug.WriteLine($"Count of buffers: {aDataBuffers.Count}");

            // make a copy of signals to avoid race condition during processing all channels
            var signalFilters = _signalFilters.ToArray();

            var flattenedData = FlattenDataBuffers(aDataBuffers);

            Parallel.ForEach(flattenedData, aPair =>
            {
                int channelId = aPair.Key;
                if (_channelsData.TryGetValue(channelId, out var channelData))
                {
                    lock (channelData)
                    {
                        SetNewDataLocked(signalFilters, aPair.Value, channelData);
                    }
                }
            });

            // count of new samples in a channel, equal for all channels.
            return flattenedData.Select(x => x.Value.Count).FirstOrDefault();
        }

        /// <summary>
        /// Flattens the buffers of channel data into continuous buffers per each channel.
        /// </summary>
        /// <param name="aDataBuffers">The buffers to flatten</param>
        /// <returns>dictionary of samples per each channel ID.</returns>
        private static IReadOnlyDictionary<int, IList<short>> FlattenDataBuffers(
            IReadOnlyCollection<short[,]> aDataBuffers)
        {
            var dictionary = new Dictionary<int, IList<short>>();

            foreach (var spdSamples in aDataBuffers)
            {
                int channelsCount = spdSamples.GetLength(0);
                var samplesCountPerChannel = spdSamples.GetLength(1);

                foreach (var channelId in Enumerable.Range(1, channelsCount))
                {
                    if (!dictionary.TryGetValue(channelId, out var channelData))
                    {
                        channelData = new List<short>();
                        dictionary[channelId] = channelData;
                    }

                    for (int sampleIndex = 0; sampleIndex < samplesCountPerChannel; sampleIndex++)
                    {
                        var sampleValue = spdSamples[channelId - 1, sampleIndex];
                        channelData.Add(sampleValue);
                    }
                }
            }

            return dictionary;
        }

        private static void SetNewDataLocked(IReadOnlyList<ISignalFilter> aSignalFilters,
            IList<short> aNewChannelData, IList<double[]> aChannelData)
        {
            // for 1 filter we have +1 channel data (unfiltered and filtered)
            if (aSignalFilters.Count + 1 != aChannelData.Count)
            {
                throw new ArgumentException(
                    $@"Mismatch channel data count: {aSignalFilters.Count} vs {aChannelData.Count}",
                    nameof(aChannelData));
            }

            int newSamplesCount = aNewChannelData.Count;

            // shift the unfiltered channel data to insert new samples
            double[] originalChannelData = aChannelData[0];
            ShiftChannelBufferToLeft(originalChannelData, newSamplesCount);

            // append new samples to the unfiltered channel data
            for (int channelIndex = originalChannelData.Length - newSamplesCount, sampleIndex = 0;
                sampleIndex < newSamplesCount;
                channelIndex++, sampleIndex++)
            {
                var sampleValue = aNewChannelData[sampleIndex];
                originalChannelData[channelIndex] = MapSampleValueToMicroVolts(sampleValue);
            }

            // apply every filter to the channel data
            for (int filterIndex = 0; filterIndex < aSignalFilters.Count; filterIndex++)
            {
                ISignalFilter signalFilter = aSignalFilters[filterIndex];

                // shift output array for filtered data of the given filter
                double[] filteredChannelData = aChannelData[filterIndex + 1];
                ShiftChannelBufferToLeft(filteredChannelData, newSamplesCount);

                // apply filter using unfiltered (or output from previous filter) and new output filtered data
                double[] unfilteredChannelData = aChannelData[filterIndex];
                signalFilter.ApplyFilter(unfilteredChannelData, filteredChannelData,
                    (filteredChannelData.Length - newSamplesCount),
                    newSamplesCount);
            }
        }

        /// <summary>
        /// Shifts a channel buffer to the left by given number of items.
        /// </summary>
        /// <param name="aBuffer">The buffer to shift.</param>
        /// <param name="aLength">The number of items to shift by.</param>
        private static void ShiftChannelBufferToLeft(double[] aBuffer, int aLength)
        {
            if (aBuffer.Length >= aLength)
            {
                // we shift part of the array
                Array.Copy(aBuffer, aLength,
                    aBuffer, 0,
                    aBuffer.Length - aLength);
            }
            else
            {
                // we shift the entire array, thus it's enough to just clear it
                Array.Clear(aBuffer, 0, aBuffer.Length);
            }
        }

        /// <summary>
        /// Appends data to the target buffer.
        /// </summary>
        /// <param name="aTargetBuffer">The target buffer to append data to.</param>
        /// <param name="aData">The data to append.</param>
        private static void AppendDataToChannelBuffer(double[] aTargetBuffer, double[] aData)
        {
            ShiftChannelBufferToLeft(aTargetBuffer, aData.Length);

            int targetOffset = aTargetBuffer.Length - aData.Length;

            Array.Copy(aData, 0,
                aTargetBuffer, targetOffset, aData.Length);
        }

        /// <inheritdoc />
        public event EventHandler DataLoaded;

        /// <inheritdoc />
        public event EventHandler<ImpedanceDataEventArgs> ImpedanceDataLoaded;

        /// <inheritdoc />
        async Task<AOResult> IChartManagerService.StartAsync(CaptureMode aMode)
        {
            if (!(aMode == CaptureMode.CaptureEegEmg || aMode == CaptureMode.CaptureImpedance))
            {
                throw new ArgumentException(
                    $@"Expected EEG or Impedance mode: {aMode}", nameof(aMode));
            }
            aMode = CaptureMode.CaptureEegEmg;
            using (await _captureModeLock.LockAsync())
            {
                if (_activeCaptureMode == CaptureMode.CaptureEegEmg)
                {
                    // nothing to do, already desired mode
                    var result = new AOResult();
                    result.SetSuccess();
                    return result;
                }

                using (Performance.StartNew())
                {
                    return await StartLockedAsync(aMode);
                }
            }
        }

        /// <inheritdoc />
        async Task IChartManagerService.StopAsync()
        {
            using (await _captureModeLock.LockAsync())
            {
                if (_activeCaptureMode != CaptureMode.CaptureNone)
                {
                    // first of all, disable refresh timer
                    _timer.Enabled = false;

                    await _biopotChartsService.PauseAsync();


                    await _chartService.UnSubscribeChartsDataChangeAsync();

                    _activeCaptureMode = CaptureMode.CaptureNone;
                }
            }
        }

        /// <summary>
        /// Start the given capture mode, while <see cref="_captureModeLock"/>  is held.
        /// </summary>
        /// <param name="aMode">The capture mode to apply.</param>
        /// <returns>true, if started successfully; otherwise, false.</returns>
        private async Task<AOResult> StartLockedAsync(CaptureMode aMode)
        {
            var result = new AOResult();
           // Performance.Start(out var reference);

                // ensure, the chart is stopped
              //  Performance.Start(reference, "StopChart");
                var asyncResult = await _biopotChartsService.PauseAsync();
              //  Performance.Stop(reference, "StopChart");
                if (!asyncResult.IsSuccess)
                {
              //      Performance.Stop(reference);

            //        Debug.WriteLine($"{nameof(IChartManagerService.StartAsync)}: " +
             //                       $"failed to stop with '{asyncResult.Message};");
                    return asyncResult;
                }
                await Task.Delay(10);
                // switch to desired mode (EEG or impedance)
                //J.M Impedance is changed to characteristic 3
                /*
                Performance.Start(reference, "SetImpedanceEnabled");
                asyncResult = await _characteristic3Service.SetImpedanceEnabledAsync(
                    aMode == CaptureMode.CaptureImpedance);
                Performance.Stop(reference, "SetImpedanceEnabled");
                if (!asyncResult.IsSuccess)
                {
                    Performance.Stop(reference);

                    Debug.WriteLine($"{nameof(IChartManagerService.StartAsync)}: " +
                                   $"failed to change impedance with '{asyncResult.Message};");
                    return asyncResult;
                 }
                 */

                // subscribe (or re-subscribe, if device has changed) to the charts
            //    Performance.Start(reference, "SubscribeChartsData");
                var succeeded = await _chartService.SubscribeChartsDataChangeAsync();
             //   Performance.Stop(reference, "SubscribeChartsData");
                if (!succeeded)
                {
              //      Performance.Stop(reference);

              //      Debug.WriteLine($"{nameof(IChartManagerService.StartAsync)}: failed to subscribe to charts");
                    result.SetFailure($"{(int)SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
                    return result;
                }

            // re-start the charts
         //   Performance.Start(reference, "StartChart");
            asyncResult = await _biopotChartsService.StartAsync();
         //   Performance.Stop(reference, "StartChart");
            if (!asyncResult.IsSuccess)
            {
         //       Performance.Stop(reference);

         //       Debug.WriteLine($"{nameof(IChartManagerService.StartAsync)}: " +
        //                        $"failed to start with '{asyncResult.Message};");
                return asyncResult;
            }
            _activeCaptureMode = aMode;

            // enable timer refresh, once started receiving data
            _timer.Enabled = true;

          //  Performance.Stop(reference);

            result.SetSuccess();
            return result;
        }

        /// <inheritdoc />
        void IChartManagerService.SetSignalFilter(SignalFilterType aFilterType, int aSamplingRate)
		{
			_signalFilters = _filterService.GetFilters(aFilterType, aSamplingRate);
			CurrentSignalFilterType = aFilterType;
            ResetChannelData();
		}

		/// <inheritdoc />
		public SignalFilterType CurrentSignalFilterType { get; private set; }

        /// <inheritdoc />
        IReadOnlyCollection<double> IChartManagerService.GetActualData(EDeviceType aDeviceType, int aChannelId)
        {
            if (aDeviceType == EDeviceType.EEGorEMG)
            {
                // primary, return montage, if exists
                if (_montageData.TryGetValue(aChannelId, out var montageHolder))
                {
                    return montageHolder.Buffer;
                }

                // otherwise, return channel data
                if (_channelsData.TryGetValue(aChannelId, out var channelData))
                {
                    lock (channelData)
                    {
                        var chartData = channelData.Last();

                        return chartData;
                    }
                }
            }

            if (aDeviceType == EDeviceType.Accelerometer)
            {
                if (_accelerometerData.TryGetValue(aChannelId, out var values))
                {
                    return values;
                }
            }
            else if (aDeviceType == EDeviceType.BioImpedance)
            {
                if (_bioImpedanceData.TryGetValue(aChannelId, out var values))
                {
                    return values;
                }
            }
            else if (aDeviceType == EDeviceType.SPSValue)
            {
                return _chSPSData;
            }

            // empty collection by default
            return new double[PointPerGraph];
        }

		/// <inheritdoc />
		IEnumerable<EDeviceType> IChartManagerService.GetDeviceTypes()
		{
			return _providedServices;
		}

        /// <inheritdoc />
        IEnumerable<int> IChartManagerService.GetIds(EDeviceType aDeviceType)
        {
            int channelsCount;
            switch (aDeviceType)
            {
                case EDeviceType.EEGorEMG:
                    channelsCount = (int) _lastBiopotInfo.ChannelsNumber;
                    break;

                case EDeviceType.BioImpedance:
                    channelsCount = (int) _lastBiopotInfo.BioImpedanceChannelNumber;
                    break;

                case EDeviceType.Accelerometer:
                    channelsCount = (int) _lastBiopotInfo.AccelerometerChannelNumber;
                    break;

                case EDeviceType.SPSValue:
                    channelsCount = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(aDeviceType), aDeviceType, null);
            }

            return Enumerable.Range(1, channelsCount).ToList();
        }

        #region Montage Public

        /// <inheritdoc />
        void IChartManagerService.RegisterMontageChannel(MontageChannelPair aMontagePair)
        {
            if (_montageData.TryGetValue(aMontagePair.ChannelId, out var montageHolder))
            {
                // there is a montage already registered for this channel.
                if (montageHolder.MontagePair == aMontagePair)
                {
                    // same montage channels, ignore the request
                    return;
                }
            }

            montageHolder = new MontageDataHolder(aMontagePair)
            {
                // use empty array to mark, that montage must be calculated for this montage pair
                Buffer = Array.Empty<double>()
            };

            // finally, we register the montage in any case (absent or mismatch)
            _montageData[aMontagePair.ChannelId] = montageHolder;
        }

        /// <inheritdoc />
        void IChartManagerService.UnregisterMontageChannel(int aChannelId)
        {
            _montageData.TryRemove(aChannelId, out _);
        }

        /// <inheritdoc />
        IReadOnlyCollection<MontageChannelPair> IChartManagerService.RegisteredMontageChannels =>
            _montageData.Values.Select(x => x.MontagePair).ToList();

        #endregion

        #region Montage Helpers

        /// <summary>
        /// The holder class for montage pair and buffer.
        /// </summary>
        private class MontageDataHolder
        {
            /// <summary>
            /// Gets the montage pair.
            /// </summary>
            public MontageChannelPair MontagePair { get; }

            /// <summary>
            /// Gets/sets the montage data buffer.
            /// </summary>
            public double[] Buffer { get; set; }

            /// <summary>
            /// Creates an instance of the class.
            /// </summary>
            /// <param name="aMontagePair">The montage channels pair.</param>
            public MontageDataHolder(MontageChannelPair aMontagePair)
            {
                MontagePair = aMontagePair;
            }
        }

        /// <summary>
        /// Updates montage data for any registered channels, that was calculated on a previous pass.
        /// </summary>
        private void UpdateExistingMontageData(int aNewSamplesCount)
        {
            if (aNewSamplesCount <= 0)
            {
                // nothing to update, quit
                return;
            }

            // take just updated with new samples
            var montages = _montageData
                .Select(x => x.Value)
                .Where(x => x.Buffer.Length != 0);

            Parallel.ForEach(Partitioner.Create(montages), aDataHolder =>
            {
                double[] buffer = aDataHolder.Buffer;
                MontageChannelPair montagePair = aDataHolder.MontagePair;

                // shift the target montage buffer
                ShiftChannelBufferToLeft(buffer, aNewSamplesCount);

                int startOffset = Math.Max(buffer.Length - aNewSamplesCount, 0);
                CalculateMontage(montagePair, buffer, startOffset);
            });
        }

        /// <summary>
        /// Calculates montage data for any registered channels for what there is no montage data yet.
        /// </summary>
        private void UpdateNewMontageChannels()
        {
            // take montages to be calculated entirely
            var montages = _montageData
                .Select(x => x.Value)
                .Where(x => x.Buffer.Length == 0);

            // recalculate montages
            Parallel.ForEach(Partitioner.Create(montages), aHolder =>
            {
                double[] buffer = new double[PointPerGraph];
                MontageChannelPair montagePair = aHolder.MontagePair;
                if (CalculateMontage(montagePair, buffer, 0))
                {
                    // save the buffer into the holder directly
                    aHolder.Buffer = buffer;
                }
            });
        }

        /// <summary>
        /// Calculates montage data for the given montage pair and starting from the given offset.
        /// </summary>
        /// <param name="aMontage">The montage pair.</param>
        /// <param name="aMontageOut">The result montage data to write data to.</param>
        /// <param name="aStartOffset">The start offset within channel buffers to calculate montage from.</param>
        /// <returns>true, if successfully calculated; otherwise, false.</returns>
        private bool CalculateMontage(MontageChannelPair aMontage, double[] aMontageOut, int aStartOffset)
        {
            if (!_channelsData.TryGetValue(aMontage.ChannelId, out var channelsData))
            {
                return false;
            }

            if (!_channelsData.TryGetValue(aMontage.ReferenceChannelId, out var referencesData))
            {
                return false;
            }

            IReadOnlyList<double> channelData, referenceData;
            lock (channelsData)
            {
                channelData = channelsData.Last();
            }

            lock (referencesData)
            {
                referenceData = referencesData.Last();
            }

            if (aMontageOut.Length != channelData.Count ||
                channelData.Count != referenceData.Count)
            {
                Debug.WriteLine($"{nameof(CalculateMontage)}: mismatch channel buffers: " +
                                $"{aMontageOut.Length}/{channelData.Count}/{referenceData.Count}");
                return false;
            }

            if (aStartOffset < 0 || aStartOffset >= channelData.Count)
            {
                Debug.WriteLine($"{nameof(CalculateMontage)}: start offset is wrong: " +
                                $"{aStartOffset}/{channelData.Count}");
                return false;
            }

            CalculateMontage(channelData, referenceData, aMontageOut, aStartOffset);
            return true;
        }

        /// <summary>
        /// Calculates montages for the given lists.
        /// </summary>
        /// <param name="aChannelData">The channel data to subtract values from.</param>
        /// <param name="aReferenceChannelData">The reference channel data to subtract value.</param>
        /// <param name="aMontageOut">The result montage data.</param>
        /// <param name="aStartOffset">The start position offset to calculate montage from, inclusively.</param>
        private void CalculateMontage(IReadOnlyList<double> aChannelData,
            IReadOnlyList<double> aReferenceChannelData, IList<double> aMontageOut, int aStartOffset)
        {
            Parallel.ForEach(Partitioner.Create(aStartOffset, aChannelData.Count), range =>
            {
                for (var sampleIndex = range.Item1; sampleIndex < range.Item2; sampleIndex++)
                {
                    aMontageOut[sampleIndex] = aChannelData[sampleIndex] - aReferenceChannelData[sampleIndex];
                }
            });
        }

        #endregion
    }
}
