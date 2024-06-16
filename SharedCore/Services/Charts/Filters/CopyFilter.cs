using System;

namespace SharedCore.Services.Charts.Filters
{
	/// <summary>
	/// The filter just copies signal data from input to output buffers without any manipulations.
	/// </summary>
	internal class CopyFilter : ISignalFilter
	{
		/// <inheritdoc />
		public void ApplyFilter(double[] aSignalIn, double[] aSignalOut, int aOffset, int aLength)
		{
			Array.Copy(aSignalIn, aOffset,
				aSignalOut, aOffset, aLength);
		}
	}
}