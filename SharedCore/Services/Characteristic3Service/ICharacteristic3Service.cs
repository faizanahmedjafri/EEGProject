using System.Threading.Tasks;
using SharedCore.Models;

namespace SharedCore.Services.Characteristic3Service
{
    public interface ICharacteristic3Service
    {
        /// <summary>
        /// Gets all data from Characteristic 3 from the remove device.
        /// </summary>
        Task<AOResult<string>> GetInfoAsync();

        /// <summary>
        /// Sets new data to the Characteristic 3 on the remote device.
        /// </summary>
        /// <param name="aData"> The new data. </param>
        Task<AOResult> SetDataAsync(string aData);

        /// <summary>
        /// Enables/disables the impedance flag.
        /// </summary>
        /// <param name="aEnabled">true, to enable impedance data; otherwise, false.</param>
        /// <returns>result of the operation.</returns>
        Task<AOResult> SetImpedanceEnabledAsync(bool aEnabled);

        /// <summary>
        /// Validates data that is going to be saved to characteristic.
        /// </summary>
        /// <param name="aData"> Data to save.</param>
        /// <returns> true - data is valid, otherwise - false. </returns>
        bool IsValidDataToWrite(string aData);
    }
}
