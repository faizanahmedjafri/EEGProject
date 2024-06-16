namespace SharedCore.Services.Charts
{
	/// <summary>
	/// The parser of biopot signal packet, received from a BLE characteristic.
	/// </summary>
	public class BiopotSignalParser
	{
        private static readonly short[,] EmptyDataSamples = new short[0, 0];
        private static readonly short[] EmptyAccelerometerSamples = new short[0];

		private const uint BioImpedanceSamplesCount = 2;
		private const uint AccelerometerSamplesCount = 6;

		private readonly BiopotGenericInfo _biopotParams;
		private readonly uint _expectedPacketSize;

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="aBiopotParams">The biopot device parameters to use for parsing.</param>
		public BiopotSignalParser(BiopotGenericInfo aBiopotParams)
		{
			_biopotParams = aBiopotParams;
			_expectedPacketSize = GetExpectedPacketSize(aBiopotParams);
		}

        /// <summary>
        /// Gets the timestamp field value.
        /// </summary>
        public int Timestamp { get; private set; }

		/// <summary>
		/// Gets SPD/EEG/EMG data, or empty array.
		/// </summary>
		public short[,] SpdData { get; private set; }

		/// <summary>
		/// Gets bio-impedance data, or empty array.
		/// </summary>
		public short[] BioImpedanceData { get; private set; }

		/// <summary>
		/// Gets accelerometer data, or empty array.
		/// </summary>
		public short[] AccelerometerData { get; private set; }

		/// <summary>
		/// Parses a BLE packet using the biopot device params.
		/// </summary>
		/// <param name="aPacket">The packet to parse.</param>
		/// <returns>true, if successfully parsed; otherwise, false.</returns>
		public bool TryParse(byte[] aPacket)
		{
			ResetData();

			if (aPacket == null || aPacket.Length == 0)
			{
				// empty packet, ignore it.
				return false;
			}

			if (aPacket.Length < _expectedPacketSize)
			{
				// wrong packet length, ignore it.
				return false;
			}

			short[,] spd = new short[
				_biopotParams.ChannelsNumber,
				_biopotParams.SamplesPerChannelNumber
			];

			int packetOffset = 0;

            // timestamp
            Timestamp = aPacket[packetOffset + 3] << 24
                        | aPacket[packetOffset + 2] << 16
                        | aPacket[packetOffset + 1] << 8
                        | aPacket[packetOffset + 0] << 0;
            packetOffset += 4;

			// SPD
			{
				uint spdLength = 2 * _biopotParams.ChannelsNumber * _biopotParams.SamplesPerChannelNumber;
				for (int offset = 0; offset < spdLength / 2; packetOffset += 2, offset++)
				{
					short sample = (short) (aPacket[packetOffset] | aPacket[packetOffset + 1] << 8);
					var sampleId = offset / _biopotParams.ChannelsNumber;
					var channelId = offset % _biopotParams.ChannelsNumber;
					spd[channelId, sampleId] = sample;
				}

				SpdData = spd;
			}

            // accelerometer
            if (_biopotParams.IsAccelerometerPresent)
            {
                short[] accelerometer = new short[AccelerometerSamplesCount];
                for (int offset = 0; offset < AccelerometerSamplesCount; packetOffset += 2, offset++)
                {
                    short sample = (short) (aPacket[packetOffset] | aPacket[packetOffset + 1] << 8);
                    accelerometer[offset] = sample;
                }

                AccelerometerData = accelerometer;
            }

            // bio-impedance
            if (_biopotParams.IsBioImpedancePresent)
            {
                short[] bioImpedance = new short[BioImpedanceSamplesCount];
                for (int offset = 0; offset < BioImpedanceSamplesCount; packetOffset += 2, offset++)
                {
                    short sample = (short) (aPacket[packetOffset] | aPacket[packetOffset + 1] << 8);
                    bioImpedance[offset] = sample;
                }

                BioImpedanceData = bioImpedance;
            }

            return true;
		}

		/// <summary>
		/// Resets data to default values.
		/// </summary>
		private void ResetData()
        {
            Timestamp = 0;
			SpdData = EmptyDataSamples;
			BioImpedanceData = AccelerometerData = EmptyAccelerometerSamples;
		}

		/// <summary>
		/// Calculates expected packet size, based on biopot params.
		/// </summary>
		/// <param name="aBiopotParams">Biopot device params</param>
		/// <returns>expected packet size.</returns>
		private uint GetExpectedPacketSize(BiopotGenericInfo aBiopotParams)
		{
			uint size = 0;

			// timestamp
			size += 4;
			// channels
			size += 2 * (aBiopotParams.SamplesPerChannelNumber * aBiopotParams.ChannelsNumber);

			// accelerometer
			if (aBiopotParams.IsAccelerometerPresent)
			{
				size += 2 * AccelerometerSamplesCount;
			}

			// bio-impedance
			if (aBiopotParams.IsBioImpedancePresent)
			{
				size += 2 * BioImpedanceSamplesCount;
			}

			return size;
		}
	}
}