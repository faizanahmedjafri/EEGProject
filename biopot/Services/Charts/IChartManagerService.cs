using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using biopot.Enums;
using biopot.Models;
using SharedCore.Models;
using SharedCore.Services.Charts;

namespace biopot.Services.Charts
{
	public interface IChartManagerService
	{
		IEnumerable<int> GetIds(EDeviceType deviceType);
		IEnumerable<EDeviceType> GetDeviceTypes();
		IReadOnlyCollection<double> GetActualData(EDeviceType deviceType, int id);
        event EventHandler DataLoaded;
		event EventHandler<ImpedanceDataEventArgs> ImpedanceDataLoaded;

        #region Start/Stop Mode

        /// <summary>
        /// Starts capturing data in the given mode.
        /// It does nothing, if it's the given mode is already active.
        /// It switches to new mode, if the given mode is changed.
        /// </summary>
        /// <param name="aMode">The mode to activate.</param>
        Task<AOResult> StartAsync(CaptureMode aMode);

        /// <summary>
        /// Stops any data receiving.
        /// </summary>
        Task StopAsync();

        #endregion

        #region Signal Filter

		/// <summary>
		/// Enables notch filter for given frequency and signal sampling rate.
		/// </summary>
		/// <param name="aFilterType">The filter frequency.</param>
		/// <param name="aSamplingRate">The signal sampling rate.</param>
		void SetSignalFilter(SignalFilterType aFilterType, int aSamplingRate);

		/// <summary>
		/// Gets currently applied signal filter type.
		/// </summary>
		SignalFilterType CurrentSignalFilterType { get; }

		#endregion

        #region Montage

        /// <summary>
        /// Enables montage channel to be returned by <see cref="GetActualData"/>
        /// for the channels in <paramref name="aMontagePair"/>.
        /// 
        /// The montage data is calculated as:
        /// <code>montage=[channelId]-[referenceChannelId]</code>
        /// </summary>
        /// <param name="aMontagePair">The montage channel IDs to create montage for.</param>
        void RegisterMontageChannel(MontageChannelPair aMontagePair);

        /// <summary>
        /// Disables montage data for the given channels.
        /// After this, <see cref="GetActualData"/> will return normal channel data, referenced to the ground (zero).
        /// It does nothing, if no montage data was registered for the channel.
        /// </summary>
        /// <param name="aChannelId"></param>
        void UnregisterMontageChannel(int aChannelId);

        /// <summary>
        /// Gets currently registered montage channels.
        /// </summary>
        IReadOnlyCollection<MontageChannelPair> RegisteredMontageChannels { get; }

        #endregion
    }

    /// <summary>
    /// The capture mode controls, which data is captured.
    /// The actual data share the same communication channel,
    /// thus they conflict and only one of them should be active at the same time.
    /// </summary>
    public enum CaptureMode
    {
        /// <summary>
        /// Captures no data.
        /// </summary>
        CaptureNone = -1,

        /// <summary>
        /// Captures normal data with EEG/EMG, accelerometer and bio-impedance.
        /// </summary>
        CaptureEegEmg = 1,

        /// <summary>
        /// Captures impedance data instead of EEG/EMG and others.
        /// </summary>
        CaptureImpedance
    }

    /// <summary>
    /// The pair identifies montage reference between 2 channels.
    /// </summary>
    public struct MontageChannelPair : IEquatable<MontageChannelPair>
    {
        /// <summary>
        /// Gets the channel ID for montage.
        /// </summary>
        public int ChannelId { get; }

        /// <summary>
        /// Gets the reference channel ID for montage.
        /// </summary>
        public int ReferenceChannelId { get; }

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="aChannelId">The channel ID for montage.</param>
        /// <param name="aReferenceChannelId">The reference channel ID for montage.</param>
        public MontageChannelPair(int aChannelId, int aReferenceChannelId)
        {
            ChannelId = aChannelId;
            ReferenceChannelId = aReferenceChannelId;
        }

        /// <inheritdoc />
        public bool Equals(MontageChannelPair aOther)
        {
            return ChannelId == aOther.ChannelId
                   && ReferenceChannelId == aOther.ReferenceChannelId;
        }

        /// <inheritdoc />
        public override bool Equals(object aOther)
        {
            if (ReferenceEquals(null, aOther))
            {
                return false;
            }

            return aOther is MontageChannelPair other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (ChannelId * 397) ^ ReferenceChannelId;
            }
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(MontageChannelPair aLeft, MontageChannelPair aRight)
        {
            return aLeft.Equals(aRight);
        }

        /// <summary>
        /// Non-equality operator.
        /// </summary>
        /// <param name="aLeft"></param>
        /// <param name="aRight"></param>
        /// <returns></returns>
        public static bool operator !=(MontageChannelPair aLeft, MontageChannelPair aRight)
        {
            return !aLeft.Equals(aRight);
        }
    }
}