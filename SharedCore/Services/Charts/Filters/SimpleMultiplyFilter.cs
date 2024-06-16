using System;

namespace SharedCore.Services.Charts.Filters
{
	/// <summary>
	/// Simple filter to multiple every signal sample with a multiplier.
	/// </summary>
	internal class SimpleMultiplyFilter : ISignalFilter
	{
		private readonly float iMultiplier;

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		public SimpleMultiplyFilter(float aMultiplier)
		{
			iMultiplier = aMultiplier;
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

			for (var i = aOffset; i < (aOffset + aLength); i++)
			{
				aSignalOut[i] = (aSignalIn[i] * iMultiplier);
			}
		}
	}
}