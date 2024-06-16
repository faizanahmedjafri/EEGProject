using System;
using System.Threading.Tasks;
using SharedCore.Models;

namespace SharedCore.Services.Charts
{
	public interface IBiopotInfoChartsService
	{
		Task<AOResult<string>> GetInfoAsync();

		/// <summary>
		/// Gets device information from a connected device.
		/// </summary>
		/// <returns>generic information about a biopot device.</returns>
		Task<AOResult<BiopotGenericInfo>> GetDeviceInfoAsync();

        /// <summary>
        /// Sets new data on the remote device.
        /// </summary>
	    Task<AOResult> SetDataAsync(string aData);

	    /// <summary>
	    /// Validates data that is going to be saved to characteristic.
	    /// </summary>
	    /// <param name="aData"> Data to save.</param>
	    /// <returns> true - data is valid, otherwise - false. </returns>
	    bool IsValidDataToWrite(string aData);
    }

	/// <summary>
	/// Represents generic information of a biopot device, like count of channels,
	/// provided bio-impedance and accelerometer data etc.
	/// </summary>
	public class BiopotGenericInfo : IEquatable<BiopotGenericInfo>
    {
        public const uint DefaultBioImpedanceChannelCount = 4;
        public const uint DefaultAccelerometerChannelCount = 3;

        /// <summary>
        /// Device name.
        /// </summary>
        public string DeviceName { get; set; } = "Unknown";

        /// <summary>
        /// Number of EEG channels.
        /// </summary>
        public uint ChannelsNumber { get; set; } = 0;

		/// <summary>
		/// Number of samples per channel in single BLE chart data packet.
		/// </summary>
		public uint SamplesPerChannelNumber { get; set; } = 7;

		/// <summary>
		/// Indicates, whether this device provides bio-impedance data.
		/// </summary>
		public bool IsBioImpedancePresent { get; set; } = true;

        /// <summary>
        /// Number of channels for bio-impedance.
        /// By default, it's <see cref="DefaultBioImpedanceChannelCount"/>.
        /// </summary>
	    public uint BioImpedanceChannelNumber { get; set; } = DefaultBioImpedanceChannelCount;

        /// <summary>
        /// Indicates, whether this device provides accelerometer data.
        /// </summary>
        public bool IsAccelerometerPresent { get; set; } = true;

        /// <summary>
        /// Indicates the format of the accelerometer data .
        /// </summary>
        public uint AccelerometerMode { get; set; } = 2;


        /// <summary>
        /// Number of channels for accelerometer.
        /// By default, it's <see cref="DefaultAccelerometerChannelCount"/>.
        /// </summary>
        public uint AccelerometerChannelNumber { get; set; } = DefaultAccelerometerChannelCount;

		/// <inheritdoc />
		public bool Equals(BiopotGenericInfo aOther)
		{
			if (ReferenceEquals(null, aOther))
			{
				return false;
			}

			return ChannelsNumber == aOther.ChannelsNumber
			       && SamplesPerChannelNumber == aOther.SamplesPerChannelNumber
			       && IsBioImpedancePresent == aOther.IsBioImpedancePresent
			       && IsAccelerometerPresent == aOther.IsAccelerometerPresent
                   && AccelerometerMode == aOther.AccelerometerMode
			       && DeviceName == aOther.DeviceName
			       && BioImpedanceChannelNumber == aOther.BioImpedanceChannelNumber
			       && AccelerometerChannelNumber == aOther.AccelerometerChannelNumber
                ;
		}

		/// <inheritdoc />
		public override bool Equals(object aOther)
		{
			if (ReferenceEquals(null, aOther))
			{
				return false;
			}

			return aOther is BiopotGenericInfo other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Tuple.Create(
				DeviceName,
				ChannelsNumber,
				SamplesPerChannelNumber,
				IsBioImpedancePresent,
				BioImpedanceChannelNumber,
				IsAccelerometerPresent,
                AccelerometerMode,
                AccelerometerChannelNumber
			).GetHashCode();
		}
	}
}