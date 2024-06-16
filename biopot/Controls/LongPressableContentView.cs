using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using biopot.Extensions;
using Xamarin.Forms;

namespace biopot.Controls
{
    /// <summary>
    /// The clickable content view, which triggers additional command,
    /// when user long presses it for defined period of time.
    /// </summary>
    public class LongPressableContentView : ClickableContentView
    {
        private CancellationTokenSource _pressingCancellationTokenSource;

        #region -- Public properties --

        /// <summary>
        /// The bindable property for <see cref="OnPressing"/>.
        /// </summary>
        public static readonly BindableProperty OnPressingProperty = BindableProperty.Create(
            nameof(OnPressing), typeof(ICommand), typeof(LongPressableContentView),
            default(ICommand));

        /// <summary>
        /// The bindable property for <see cref="LongPressDelay"/>.
        /// </summary>
        public static readonly BindableProperty LongPressDelayProperty = BindableProperty.Create(
            nameof(LongPressDelay), typeof(TimeSpan), typeof(LongPressableContentView),
            TimeSpan.FromMilliseconds(800), BindingMode.TwoWay);

        /// <summary>
        /// The bindable property for <see cref="LongPressTriggerInterval"/>.
        /// </summary>
        public static readonly BindableProperty LongPressTriggerIntervalProperty = BindableProperty.Create(
            nameof(LongPressTriggerInterval), typeof(TimeSpan), typeof(LongPressableContentView),
            TimeSpan.FromMilliseconds(800), BindingMode.TwoWay);

        /// <summary>
        /// Gets/sets the command to execute, when long press trigger with defined rate.
        /// </summary>
        public ICommand OnPressing
        {
            get => (ICommand) GetValue(OnPressingProperty);
            set => SetValue(OnPressingProperty, value);
        }

        /// <summary>
        /// Gets/sets the delay before the long press starts counting.
        /// </summary>
        public TimeSpan LongPressDelay
        {
            get => (TimeSpan) GetValue(LongPressDelayProperty);
            set => SetValue(LongPressDelayProperty, value);
        }

        /// <summary>
        /// Gets/sets the interval between sequential trigger of
        /// <see cref="OnPressing"/> after the <see cref="LongPressDelay"/> has elapsed.
        /// </summary>
        public TimeSpan LongPressTriggerInterval
        {
            get => (TimeSpan) GetValue(LongPressTriggerIntervalProperty);
            set => SetValue(LongPressTriggerIntervalProperty, value);
        }

        #endregion

        #region -- Overrides --

        /// <inheritdoc />
        public override bool TouchesBegan(IEnumerable<NGraphics.Point> aPoints)
        {
            var result = base.TouchesBegan(aPoints);
            StartLongPressingAsync();
            return result;
        }

        /// <inheritdoc />
        public override bool TouchesEnded(IEnumerable<NGraphics.Point> aPoints)
        {
            StopLongPressing();
            return base.TouchesEnded(aPoints);
        }

        /// <inheritdoc />
        public override bool TouchesCancelled(IEnumerable<NGraphics.Point> aPoints)
        {
            StopLongPressing();
            return base.TouchesCancelled(aPoints);
        }

        #endregion

        #region -- Private helpers --

        /// <summary>
        /// Stops any long pressing timer.
        /// </summary>
        private void StopLongPressing()
        {
            _pressingCancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Starts the long pressing timer.
        /// </summary>
        private async void StartLongPressingAsync()
        {
            StopLongPressing();

            using (_pressingCancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = _pressingCancellationTokenSource.Token;
                try
                {
                    // initial delay
                    await Task.Delay(LongPressDelay, cancellationToken);

                    // trigger and wait, repeatedly
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        OnPressing?.ExecuteIfCan();
                        await Task.Delay(LongPressTriggerInterval, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // nothing to do, ignored
                }
            }

            // The view is always executed in the main UI thread, thus it's thread-safe
            _pressingCancellationTokenSource = null;
        }

        #endregion
    }
}