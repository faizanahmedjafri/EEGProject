using System;

namespace SharedCore.Services.Charts.Filters
{
	/// <summary>
	/// The notch filter implementation to cutoff a frequency band from a signal.
	/// </summary>
	internal class NotchFilter : ISignalFilter
	{
		private readonly NotchFilterParams iFilterParams;

		/// <summary>
		/// Count of samples to take before current sample.
		/// </summary>
		private int PreviousSamplesCount => AccountedSamplesCount - 1;

		/// <summary>
		/// Count of samples to take for calculation of current sample.
		/// </summary>
		private int AccountedSamplesCount => 2 * iFilterParams.FilterRank + 1;

		/// <summary>
		/// Creates an instance of the filter.
		/// </summary>
		/// <param name="aFilterParams">Filter params.</param>
		public NotchFilter(NotchFilterParams aFilterParams)
		{
			iFilterParams = aFilterParams;
		}

		/// <inheritdoc />
		public void ApplyFilter(double[] aSignalIn, double[] aSignalOut, int aOffset, int aLength)
		{
			if (aSignalIn.Length != aSignalOut.Length)
			{
				throw new ArgumentException(
					"Mismatch length of signal buffers", nameof(aSignalOut));
			}

			if (aOffset + aLength > aSignalIn.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(aOffset),
					$"Offset({aOffset})/length({aLength}) point beyond array boundaries");
			}

			// correct minimal offset to avoid out of bounds errors.
			if (aOffset < PreviousSamplesCount)
			{
				var adjustCount = PreviousSamplesCount - aOffset;
				aOffset += adjustCount;
				aLength -= adjustCount;
			}

			DoApplyFilter(aSignalIn, aSignalOut, aOffset, aLength);
		}

		/// <summary>
		/// Applies filter with all validated and corrected input data.
		/// </summary>
		private void DoApplyFilter(double[] aSignalIn, double[] aSignalOut, int aOffset, int aLength)
		{
			for (int indexSignal = aOffset; indexSignal < aOffset + aLength; indexSignal++)
			{
				aSignalOut[indexSignal] = GetComponentYn(aSignalIn, aSignalOut, indexSignal);
			}
		}

		/// <summary>
		/// Calculates final result of a sample.
		/// </summary>
		/// <param name="aSignalIn">The unfiltered signal.</param>
		/// <param name="aSignalOut">The filtered signal.</param>
		/// <param name="aOffset">The offset of current sample being filtered.</param>
		/// <returns>final calculated Yn result.</returns>
		private double GetComponentYn(double[] aSignalIn, double[] aSignalOut, int aOffset)
		{
			double sumXa = GetComponentXnAn(aSignalIn, aOffset);
			double sumYb = GetComponentYnBn(aSignalOut, aOffset);
			double coefficientBn = iFilterParams.CoefficientsB[AccountedSamplesCount - 1];

			return (sumXa - sumYb) / coefficientBn;
		}

		/// <summary>
		/// Calculates sum of multiplications of (Xn * An).
		/// </summary>
		/// <param name="aSignalIn">The unfiltered signal.</param>
		/// <param name="aOffset">The offset of current sample being filtered.</param>
		/// <returns>component value.</returns>
		private double GetComponentXnAn(double[] aSignalIn, int aOffset)
		{
			int previousOffset = aOffset - (AccountedSamplesCount - 1);

			double sum = 0;
			for (int i = 0; i < AccountedSamplesCount; i++)
			{
				var sampleIn = aSignalIn[previousOffset + i];
				var coefficientA = iFilterParams.CoefficientsA[i];

				sum += coefficientA * sampleIn;
			}

			return sum;
		}

		/// <summary>
		/// Calculates sum of multiplications of (Yn * Bn).
		/// </summary>
		/// <param name="aSignalOut">The filtered signal.</param>
		/// <param name="aOffset">The offset of current sample being filtered.</param>
		/// <returns>component value.</returns>
		private double GetComponentYnBn(double[] aSignalOut, int aOffset)
		{
			int previousOffset = aOffset - (AccountedSamplesCount - 1);

			double sum = 0;
			for (int i = 0; i < (AccountedSamplesCount - 1); i++)
			{
				var sampleIn = aSignalOut[previousOffset + i];
				var coefficientB = iFilterParams.CoefficientsB[i];

				sum += coefficientB * sampleIn;
			}

			return sum;
		}
	}
}