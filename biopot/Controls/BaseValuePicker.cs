using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prism.Commands;
using Xamarin.Forms;

namespace biopot.Controls
{
    /// <summary>
    /// A step interpolator to support variable step for <see cref="BaseValuePicker"/>.
    /// </summary>
    public interface IPickerStepInterpolator
    {
        /// <summary>
        /// Gets the step to apply to the picker.
        /// </summary>
        /// <param name="aPickerValue">Current value of the picker.</param>
        /// <param name="aIncrease">Flag indicates direction of the step - increase or decrease.</param>
        /// <returns>step to use.</returns>
        uint GetStep(int aPickerValue, bool aIncrease);
    }

    /// <summary>
    /// The base value picker with common functionality.
    /// </summary>
    public abstract class BaseValuePicker : ContentView
    {
        private bool iIsIncreaseAllowed;
        private bool iIsDecreaseAllowed;
        private string iFormattedValue;

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        protected BaseValuePicker()
        {
            ValueFallingCommand = new DelegateCommand(DecreasePickerValueByStep);
            ValueFallingCommand.ObservesCanExecute(() => IsDecreaseAllowed);

            ValueRaisingCommand = new DelegateCommand(IncreasePickerValueByStep);
            ValueRaisingCommand.ObservesCanExecute(() => IsIncreaseAllowed);
        }

        #region Bindable Properties

        /// <summary>
        /// The binding property for <see cref="MaxValue"/>.
        /// </summary>
        public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(nameof(MaxValue),
            typeof(int), typeof(BaseValuePicker), 100, BindingMode.TwoWay,
            propertyChanged: (aBindable, aValue, aNewValue) =>
            {
                var picker = (BaseValuePicker) aBindable;
                picker.UpdateFormattedValue();
            });

        /// <summary>
        /// The binding property for <see cref="MinValue"/>.
        /// </summary>
        public static readonly BindableProperty MinValueProperty = BindableProperty.Create(nameof(MinValue),
            typeof(int), typeof(BaseValuePicker), 0, BindingMode.TwoWay,
            propertyChanged: (aBindable, aValue, aNewValue) =>
            {
                var picker = (BaseValuePicker) aBindable;
                picker.UpdateFormattedValue();
            });

        /// <summary>
        /// The binding property for <see cref="PickerStep"/>.
        /// </summary>
        public static readonly BindableProperty PickerStepProperty = BindableProperty.Create(nameof(PickerStep),
            typeof(IPickerStepInterpolator), typeof(BaseValuePicker),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValueCreator: aBindable => new StaticPickerStep(1),
            coerceValue: (aBindable, aValue) =>
            {
                if (!(aValue is IPickerStepInterpolator value))
                {
                    value = new StaticPickerStep(1);
                }

                return value;
            });

        /// <summary>
        /// The binding property for <see cref="PickerValue"/>.
        /// </summary>
        public static readonly BindableProperty PickerValueProperty = BindableProperty.Create(nameof(PickerValue),
            typeof(int), typeof(BaseValuePicker), default(int), BindingMode.TwoWay,
            coerceValue: (aBindable, aValue) =>
            {
                var valuePicker = (BaseValuePicker) aBindable;
                if (aValue is int value)
                {
                    if (value < valuePicker.MinValue)
                    {
                        value = valuePicker.MinValue;
                    }
                    else if (value > valuePicker.MaxValue)
                    {
                        value = valuePicker.MaxValue;
                    }
                }
                else
                {
                    value = valuePicker.MinValue;
                }

                return value;
            }, propertyChanged: (aBindable, aValue, aNewValue) =>
            {
                var picker = (BaseValuePicker) aBindable;
                picker.UpdateFormattedValue();
                picker.UpdateIncreaseDecreaseAvailability();
            });

        /// <summary>
        /// The binding property for <see cref="PickerValueFormatter"/>.
        /// </summary>
        public static readonly BindableProperty PickerValueFormatterProperty = BindableProperty.Create(
            nameof(PickerValueFormatter), typeof(string), typeof(BaseValuePicker),
            "{0}", BindingMode.TwoWay, coerceValue: (aBindable, aValue) =>
            {
                if (aValue is string value && !string.IsNullOrEmpty(value))
                {
                    return aValue;
                }

                return "{0}";
            }, propertyChanged: (aBindable, aValue, aNewValue) =>
            {
                var picker = (BaseValuePicker) aBindable;
                picker.UpdateFormattedValue();
            });

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets the picker value formatter.
        /// </summary>
        public string PickerValueFormatter
        {
            get => (string) GetValue(PickerValueFormatterProperty);
            set => SetValue(PickerValueFormatterProperty, value);
        }

        /// <summary>
        /// Gets/sets the max value of the picker.
        /// </summary>
        public int MaxValue
        {
            get => (int) GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Gets/sets the min value of the picker.
        /// </summary>
        public int MinValue
        {
            get => (int) GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Gets/sets the step interpolator of the picker.
        /// </summary>
        public IPickerStepInterpolator PickerStep
        {
            get => (IPickerStepInterpolator) GetValue(PickerStepProperty);
            set => SetValue(PickerStepProperty, value);
        }

        /// <summary>
        /// Gets/sets the picker value.
        /// </summary>
        public int PickerValue
        {
            get => (int) GetValue(PickerValueProperty);
            set => SetValue(PickerValueProperty, value);
        }

        /// <summary>
        /// Gets the formatted value.
        /// </summary>
        public string FormattedValue
        {
            get => iFormattedValue;
            private set
            {
                if (iFormattedValue != value)
                {
                    iFormattedValue = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the flag indicating the 'plus' is allowed.
        /// </summary>
        public bool IsIncreaseAllowed
        {
            get => iIsIncreaseAllowed;
            private set
            {
                if (iIsIncreaseAllowed != value)
                {
                    iIsIncreaseAllowed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the flag indicating the 'minus' is allowed.
        /// </summary>
        public bool IsDecreaseAllowed
        {
            get => iIsDecreaseAllowed;
            private set
            {
                if (iIsDecreaseAllowed != value)
                {
                    iIsDecreaseAllowed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the command to execute to fall the picker value by <see cref="PickerStep"/>.
        /// </summary>
        public DelegateCommand ValueFallingCommand { get; }

        /// <summary>
        /// Gets the command to execute to raise the picker value by <see cref="PickerStep"/>.
        /// </summary>
        public DelegateCommand ValueRaisingCommand { get; }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Decreases the picker value by <see cref="PickerStep"/>.
        /// </summary>
        private void DecreasePickerValueByStep()
        {
            Debug.WriteLine($"{nameof(DecreasePickerValueByStep)}");
            PickerValue -= (int) PickerStep.GetStep(PickerValue, false);
        }

        /// <summary>
        /// Increases the picker value by <see cref="PickerStep"/>.
        /// </summary>
        private void IncreasePickerValueByStep()
        {
            Debug.WriteLine($"{nameof(IncreasePickerValueByStep)}");
            PickerValue += (int) PickerStep.GetStep(PickerValue, true);
        }

        /// <summary>
        /// Updates availability of increase/decrease actions.
        /// </summary>
        private void UpdateIncreaseDecreaseAvailability()
        {
            IsIncreaseAllowed = PickerValue < MaxValue;
            IsDecreaseAllowed = PickerValue > MinValue;
        }

        /// <summary>
        /// Updates the formatted value with current picker's value.
        /// </summary>
        private void UpdateFormattedValue()
        {
            FormattedValue = string.Format(PickerValueFormatter, PickerValue);
        }

        #endregion
    }

    /// <summary>
    /// Simple picker step with static value.
    /// </summary>
    public sealed class StaticPickerStep : IPickerStepInterpolator
    {
        public static StaticPickerStep One { get; } = new StaticPickerStep(1);

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="aPickerStep">The picker step.</param>
        public StaticPickerStep(uint aPickerStep)
        {
            PickerStep = aPickerStep;
        }

        /// <summary>
        /// Gets/sets the picker step to use.
        /// </summary>
        public uint PickerStep { get; }

        /// <inheritdoc />
        uint IPickerStepInterpolator.GetStep(int aPickerValue, bool aIncrease)
        {
            return PickerStep;
        }
    }

    /// <summary>
    /// The step interpolator, which uses the dictionary's key
    /// to find lowest bound of range to apply corresponding value.
    /// </summary>
    public sealed class RangePickerStep : SortedList<int, uint>, IPickerStepInterpolator
    {
        /// <inheritdoc />
        uint IPickerStepInterpolator.GetStep(int aPickerValue, bool aIncrease)
        {
            // since the keys are ordered ascending of lowest boundary of ranges,
            // then we can iterate and find first matching range to take the step from.

            var foundStep = this
                .Select(x => new {LowestBound = x.Key, Step = x.Value})
                .TakeWhile(x => x.LowestBound <= aPickerValue)
                .Select(x => x.Step)
                .DefaultIfEmpty(0u)
                .Last();

            Debug.WriteLine($"{nameof(RangePickerStep)}: " +
                            $"value={aPickerValue}, step={foundStep}, increase={aIncrease}");

            return foundStep;
        }
    }
}