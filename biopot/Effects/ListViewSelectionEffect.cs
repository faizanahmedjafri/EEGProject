using System;
using System.Linq;
using Xamarin.Forms;

namespace biopot.Effects
{
    /// <summary>
    /// The class represents an effect to disable selection of items of a <see cref="ListView"/>.
    /// </summary>
    /// <remarks>The same functionality could be achived as a behavior.</remarks>
    /// <remarks>https://developer.xamarin.com/guides/xamarin-forms/user-interface/listview/interactivity/#Disabling_Selection</remarks>
    public sealed class ListViewSelectionEffect : PlatformEffect<object, object>
    {
        public static readonly BindableProperty DisableHighlightProperty =
            BindableProperty.CreateAttached("DisableHighlight",
                typeof(bool), typeof(ListViewSelectionEffect), false,
                propertyChanged: (b, o, n) => AttachEffectIfNeeded(b));

        /// <inheritdoc />
        protected override void OnAttached()
        {
            if (!(Element is ListView))
            {
                throw new InvalidCastException($"{Element} must be {nameof(ListView)}");
            }

            ((ListView) Element).ItemSelected += OnListItemSelectedToDisable;
        }

        /// <inheritdoc />
        protected override void OnDetached()
        {
            ((ListView) Element).ItemSelected -= OnListItemSelectedToDisable;
        }

        /// <summary>
        /// Handles item selection and unselects it.
        /// The method is static to avoid capturing unneeded instances and contexts.
        /// </summary>
        private static void OnListItemSelectedToDisable(
            object aSender, SelectedItemChangedEventArgs aArgs)
        {
            var listView = (ListView) aSender;
            if (GetDisableHighlight(listView))
            {
                listView.SelectedItem = null;
            }
        }

        /// <summary>
        /// Retrieves value of the attached <see cref="DisableHighlightProperty"/>.
        /// </summary>
        /// <param name="aBindable">The bindable object.</param>
        /// <returns>true to disable selection; otherwise false.</returns>
        public static bool GetDisableHighlight(BindableObject aBindable)
        {
            return (bool) aBindable.GetValue(DisableHighlightProperty);
        }

        /// <summary>
        /// Sets value of the attached <see cref="DisableHighlightProperty"/>.
        /// </summary>
        /// <param name="aBindable">The bindable object.</param>
        /// <param name="aValue">The value to set.</param>
        public static void SetDisableHighlight(BindableObject aBindable, bool aValue)
        {
            aBindable.SetValue(DisableHighlightProperty, aValue);
        }

        /// <summary>
        /// Handles change of the attached property.
        /// </summary>
        private static void AttachEffectIfNeeded(BindableObject aBindable)
        {
            var view = aBindable as ListView;
            if (view == null)
            {
                throw new InvalidCastException($"{aBindable} must be {nameof(ListView)}");
            }

            var effect = view.Effects.FirstOrDefault(x => x is ListViewSelectionEffect);
            if (effect == null)
            {
                view.Effects.Add(new ListViewSelectionEffect());
            }
        }
    }
}