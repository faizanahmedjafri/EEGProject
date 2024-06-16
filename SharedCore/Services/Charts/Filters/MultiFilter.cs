using System.Collections.Generic;

namespace SharedCore.Services.Charts.Filters
{
	/// <summary>
	/// The filter to apply multiple filters one by one.
	/// </summary>
	internal sealed class MultiFilter : ISignalFilter
	{
		private readonly IReadOnlyList<ISignalFilter> iFilters;

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="aFilters">The ordered filters to apply.</param>
		public MultiFilter(IReadOnlyList<ISignalFilter> aFilters)
		{
			iFilters = aFilters;
		}

		/// <inheritdoc />
		public void ApplyFilter(double[] aSignalIn, double[] aSignalOut, int aOffset, int aLength)
		{
			for (int i = 0; i < iFilters.Count; i++)
			{
				var signalFilter = iFilters[i];
				signalFilter.ApplyFilter(aSignalIn, aSignalOut, aOffset, aLength);

				if (i < iFilters.Count - 1)
				{
					// make copy of just filtered signal to become input signal for next filter,
					// unless this is the last filter to apply.
					aSignalIn = (double[]) aSignalOut.Clone();
				}
			}
		}
	}
}