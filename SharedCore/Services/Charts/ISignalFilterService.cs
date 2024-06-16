using System.Collections.Generic;

namespace SharedCore.Services.Charts
{
	/// <summary>
	/// A service to create signal filters.
	/// </summary>
	public interface ISignalFilterService
	{
		/// <summary>
		/// Gets a filter.
		/// </summary>
		/// <returns>active filters for chosen frequency filter.</returns>
		IReadOnlyList<ISignalFilter> GetFilters(SignalFilterType aFilterType, int aSamplingRate);
	}

	/// <summary>
	/// The frequency bands that the notch filter can operate on.
	/// </summary>
	public enum SignalFilterType
	{
		/// <summary>
		/// No filter, pass signal as is.
		/// </summary>
		NoFilter,

		/// <summary>
		/// Notch filter to filter out 50Hz/100Hz frequency.
		/// </summary>
		NotchFilter50Hz,

		/// <summary>
		/// Notch filter to filter out 60Hz/120Hz frequency.
		/// </summary>
		NotchFilter60Hz,
	}

	/// <summary>
	/// An interface for a signal filter.
	/// </summary>
	public interface ISignalFilter
	{
		/// <summary>
		/// Applies a filter to signal data.
		/// </summary>
		/// <param name="aSignalIn">The signal buffer, containing unfiltered data to use.</param>
		/// <param name="aSignalOut">The signal buffer to save filtered data to.</param>
		/// <param name="aOffset">The offset of 1st sample to start applying filter from. It's applicable for both buffers.</param>
		/// <param name="aLength">The number of samples to apply the filter. It's applicable for both buffers.</param>
		void ApplyFilter(double[] aSignalIn, double[] aSignalOut, int aOffset, int aLength);
	}
}