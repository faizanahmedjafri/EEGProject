using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using SharedCore.Enums;
using SharedCore.Models;

namespace SharedCore.Services.TemperatureDataService
{
    public class BatteryAndTemperatureDataService : ITemperatureDataService
    {
        private readonly Guid _serviceGuid = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
        private readonly Guid _characterGuid = new Guid("0000fff6-0000-1000-8000-00805f9b34fb");
        private const int TryGetServiceCounter = 2000;

        private enum Characteristic
        {
            SyncModeValueIndex = 8,
            EraseModeValueIndex = 11,
            BatteryValueIndex = 12,
            TemperatureValueIndex = 13,
            MinLength
        }

        private ICharacteristic _batteryCharacter;
        private int iTemperatureActualValue;
        private double iSynchRatioValueActualValue;
        private int iBatteryActualValue;
        private bool _iIsEraseModeActive;
        private bool _iIsSyncModeActive;

        #region Public Properties

        /// <inheritdoc />
        public int BatteryActualValue
        {
            get => iBatteryActualValue;
            private set
            {
                if (iBatteryActualValue != value)
                {
                    iBatteryActualValue = value;
                    OnBatteryLevelChanged?.Invoke(this, iBatteryActualValue);

                    Debug.WriteLine($" ---- Battery Value --- {BatteryActualValue}");
                }
            }
        }

        /// <inheritdoc />
        public int TemperatureActualValue
        {
            get => iTemperatureActualValue;
            private set
            {
                if (iTemperatureActualValue != value)
                {
                    iTemperatureActualValue = value;
                    OnTemperatureChanged?.Invoke(this, iTemperatureActualValue);

                    Debug.WriteLine($" ---- Temperature value --- {TemperatureActualValue}");
                }
            }
        }

        /// <inheritdoc />
        public double SynchRatioActualValue
        {
            get => iSynchRatioValueActualValue;
            private set
            {
                if (Math.Abs(iSynchRatioValueActualValue - value) > 0.0001)
                {
                    iSynchRatioValueActualValue = value;

                    Debug.WriteLine($" ---- Synch ratio value --- {SynchRatioActualValue}");
                }
            }
        }

        /// <inheritdoc />
        public bool IsEraseModeActive
        {
            get => _iIsEraseModeActive;
            private set
            {
                if (_iIsEraseModeActive != value)
                {
                    _iIsEraseModeActive = value;
                    OnEraseModeChanged?.Invoke(this, _iIsEraseModeActive);

                    Debug.WriteLine($" ---- Is in erase mode --- {IsEraseModeActive}");
                }
            }
        }


        /// <inheritdoc />
        public bool IsSyncModeActive
        {
            get => _iIsSyncModeActive;
            private set
            {
                if (_iIsSyncModeActive != value)
                {
                    _iIsSyncModeActive = value;
                    OnSyncModeChanged?.Invoke(this, _iIsSyncModeActive);

                    Debug.WriteLine($" ---- Is in sync mode --- {IsSyncModeActive}");
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler<int> OnTemperatureChanged;

        /// <inheritdoc />
        public event EventHandler<int> OnBatteryLevelChanged;

        /// <inheritdoc />
        public event EventHandler<bool> OnEraseModeChanged;

        /// <inheritdoc />
        public event EventHandler<bool> OnSyncModeChanged;

        #endregion

        /// <inheritdoc />
        public async Task<AOResult<string>> GetInfoAsync(IDevice aDevice)
        {
            var result = new AOResult<string>();
            if (aDevice != null && aDevice.State == DeviceState.Connected)
            {
                try
                {
                    var service = await aDevice.GetServiceAsync(_serviceGuid);
                    var characteristic = await service.GetCharacteristicAsync(_characterGuid);

                    var data = await characteristic.ReadAsync();
                    var val = BitConverter.ToString(data);

                    result.SetSuccess(val);
                }
                catch (CharacteristicReadException)
                {
                    result.SetFailure($"{(int) SignalRetrievalErrors.CharacteristicReadException}");
                }
            }
            else
            {
                result.SetFailure($"{(int) SignalRetrievalErrors.DeviceNotConnected}");
            }

            return result;
        }

        /// <inheritdoc/>
        async Task<AOResult> ITemperatureDataService.SubscribeChangesAsync(IDevice aDevice)
        {
            var result = new AOResult();

            if (Equals(_batteryCharacter?.Service.Device, aDevice))
            {
                // already subscribe to the device, nothing to do
                result.SetSuccess();
                return result;
            }

            await (this as ITemperatureDataService).UnsubscribeChangeAsync();

            try
            {
                IService batteryService = null;
                int counter = 0;

                while (batteryService == null && counter != TryGetServiceCounter)
                {
                    //try to get characteristics many times
                    batteryService = await aDevice.GetServiceAsync(_serviceGuid);
                    counter++;
                }

                if (batteryService == null)
                {
                    result.SetFailure($"{(int)SignalRetrievalErrors.MissingBleService}");
                    return result;
                }

                _batteryCharacter = await batteryService.GetCharacteristicAsync(_characterGuid);
                _batteryCharacter.ValueUpdated += OnCharacteristicValueUpdated;

                await _batteryCharacter.ReadAsync();
                HandleCharacteristicValue(_batteryCharacter.Value);

                await _batteryCharacter.StartUpdatesAsync();

                result.SetSuccess();
                return result;
            }

            catch (CharacteristicReadException e)
            {
                Debug.WriteLine($"{nameof(ITemperatureDataService.SubscribeChangesAsync)}:{e}");

                result.SetFailure($"{(int) SignalRetrievalErrors.CharacteristicReadException}");
                return result;
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine($"{nameof(ITemperatureDataService.SubscribeChangesAsync)}:{e}");

                result.SetFailure($"{(int) SignalRetrievalErrors.NotSupportedCharacteristicOperation}");
                return result;
            }
        }

        /// <inheritdoc/>
        async Task ITemperatureDataService.UnsubscribeChangeAsync()
        {
            if (_batteryCharacter != null)
            {
                _batteryCharacter.ValueUpdated -= OnCharacteristicValueUpdated;

                try
                {
                    await _batteryCharacter.StopUpdatesAsync();
                }
                catch (Exception e)
                {
                    // just ignore since nothing can be done with this exception
                    Debug.WriteLine($"{nameof(ITemperatureDataService.UnsubscribeChangeAsync)}:{e}");
                }

                _batteryCharacter = null;
            }
        }

        /// <summary>
        /// Handles characteristic value changes.
        /// </summary>
        /// <param name="aSender"> The event sender. </param>
        /// <param name="aArgs"> The event arguments. </param>
        private void OnCharacteristicValueUpdated(object aSender, CharacteristicUpdatedEventArgs aArgs)
        {
            var characteristics = aArgs.Characteristic;
            var value = characteristics.Value;
            HandleCharacteristicValue(value);
        }

        /// <summary>
        /// Handles new value of the characteristic.
        /// </summary>
        /// <param name="aValue">New characteristic value.</param>
        private void HandleCharacteristicValue(byte[] aValue)
        {
            if (aValue.Length >= (int) Characteristic.MinLength)
            {
                TemperatureActualValue = (int)(sbyte)aValue[(int)Characteristic.TemperatureValueIndex];
                BatteryActualValue = aValue[(int) Characteristic.BatteryValueIndex];
                SynchRatioActualValue = BitConverter.ToDouble(aValue, 0);
                IsEraseModeActive = aValue[(int)Characteristic.EraseModeValueIndex] == 0;
                IsSyncModeActive = aValue[(int)Characteristic.SyncModeValueIndex] == 1;
            }
            else
            {
                TemperatureActualValue = 0;
                BatteryActualValue = 0;
                IsEraseModeActive = true; 
                IsSyncModeActive = false;
                
                // default Sync Ratio value
                SynchRatioActualValue = 1;
            }
        }
    }
}
