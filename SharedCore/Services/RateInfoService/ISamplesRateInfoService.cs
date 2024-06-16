using System;
using System.Threading.Tasks;
using SharedCore.Models;

namespace SharedCore.Services.RateInfoService
{
	public interface ISamplesRateInfoService
	{
		Task<AOResult<string>> GetInfoAsync();

	    /// <summary>
	    /// Gets hardware low pass filter from the remote device.
	    /// </summary>
	    /// <returns>the filter index - [0-16].</returns>
	    Task<AOResult<int>> GetHardwareLowPassFilterAsync();

	    /// <summary>
	    /// Sets new hardware low pass filter on the remote device.
	    /// </summary>
	    /// <param name="aFilterIndex">The filter index - [0-16].</param>
        Task<AOResult> SetHardwareLowPassFilterAsync(int aFilterIndex);

	    /// <summary>
	    /// Gets hardware high pass filter from the remote device.
	    /// </summary>
	    /// <returns>the filter index - [0-24].</returns>
        Task<AOResult<int>> GetHardwareHighPassFilterAsync();

	    /// <summary>
	    /// Sets new hardware high pass filter on the remote device.
	    /// </summary>
	    /// <param name="aFilterIndex">The filter index - [0-24].</param>
        Task<AOResult> SetHardwareHighPassFilterAsync(int aFilterIndex);

        /// <summary>
        /// Gets cutoff filter from the remote device.
        /// </summary>
        /// <returns>the filter index - [0-15].</returns>
        Task<AOResult<int>> GetSoftwareLowPassFilterAsync();

		/// <summary>
		/// Sets new cutoff filter on the remote device.
		/// </summary>
		/// <param name="aFilterIndex">The filter index - [0-15].</param>
		Task<AOResult> SetSoftwareLowPassFilterAsync(int aFilterIndex);

		/// <summary>
		/// Gets current sampling rate.
		/// </summary>
		/// <returns>sampling rate.</returns>
		Task<AOResult<int>> GetSamplingRateAsync();

		/// <summary>
		/// Sets new sampling rate.
		/// </summary>
		/// <param name="aSamplingRate">New sampling rate.</param>
		Task<AOResult> SetSamplingRateAsync(int aSamplingRate);

        /// <summary>
        /// Sets new data on the remote device.
        /// </summary>
        /// <returns></returns>
	    Task<AOResult> SetDataAsync(string aData);

        /// <summary>
        /// Invoked when sampling rate of a device changes.
        /// </summary>
        event EventHandler<int> SamplingRateChanged;

	    /// <summary>
	    /// Validates data that is going to be saved to characteristic.
	    /// </summary>
	    /// <param name="aData"> Data to save.</param>
	    /// <returns> true - data is valid, otherwise - false. </returns>
	    bool IsValidDataToWrite(string aData);
    }
}
