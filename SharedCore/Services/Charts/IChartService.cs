using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SharedCore.Services.Charts;

namespace SharedCore.Services
{
	public interface IChartService
	{
        /// <summary>
        /// Takes pending (unprocessed) chart data from the service.
        /// </summary>
        /// <returns>ordered collection of snapshots, or empty collection.</returns>
        IReadOnlyList<IChartDataSnapshot> TryTakePendingData();

		Task<bool> SubscribeChartsDataChangeAsync();
		Task UnSubscribeChartsDataChangeAsync();

		/// <summary>
		/// Information of a device to draw a chart.
		/// It can change, when connected device changes.
		/// </summary>
		BiopotGenericInfo DeviceInfo { get; }

        /// <summary>
        /// Invoked when the data information changes.
        /// </summary>
        event EventHandler<BiopotGenericInfo> DeviceInfoUpdated;

        /// <summary>
        /// Gets current sampling rate.
        /// </summary>
        int DeviceSamplingRate { get; }

        /// <summary>
        /// Invoked when the sampling rate changes.
        /// </summary>
        event EventHandler<int> DeviceSamplingRateChanged;
    }

    /// <summary>
    /// The class represents a snapshot of the chart data, receiving at once.
    /// </summary>
    public interface IChartDataSnapshot : IDisposable
    {
        /// <summary>
        /// Normal data samples per each channel.
        /// </summary>
        short[,] DataSamples { get; }

        /// <summary>
        /// Accelerometer samples, or empty array, if no data available.
        /// </summary>
        short[] AccelerometerSamples { get; }

        /// <summary>
        /// Bio-impedance samples, or empty array, if no data available.
        /// </summary>
        short[] BioImpedanceSamples { get; }

        /// <summary>
        /// The raw data buffer.
        /// </summary>
        byte[] RawBuffer { get; }

        /// <summary>
        /// The timestamp value from the remote device.
        /// </summary>
        int PacketTimestamp { get; }

        /// <summary>
        /// Gets/sets elapsed time since last snapshot.
        /// </summary>
        TimeSpan ElapsedTime { get; }
    }
}
