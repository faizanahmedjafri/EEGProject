using System.Threading.Tasks;
using SharedCore.Enums;
using SharedCore.Models;

namespace SharedCore.Services.Charts
{
	public interface IBiobotDataForChartsService
	{
		Task<AOResult<EDataForChartsState>> GetDataStateAsync();
		Task<AOResult> StartAsync();
		Task<AOResult> PauseAsync();
		Task<AOResult> StopAsync();

        /// <summary>
        /// Sets new data on the remote device.
        /// </summary>
        /// <param name="aData"> The new data. </param>
	    Task<AOResult> SetDataAsync(string aData);

	    /// <summary>
	    /// Validates data that is going to be saved to characteristic.
	    /// </summary>
	    /// <param name="aData"> Data to save.</param>
	    /// <returns> true - data is valid, otherwise - false. </returns>
	    bool IsValidDataToWrite(string aData);
    }
}
