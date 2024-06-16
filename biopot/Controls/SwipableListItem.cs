using Xamarin.Forms;

namespace biopot.Controls
{
    public class SwipableListItem : StackLayout
    {
        public SwipableListItem()
        {
            Orientation = StackOrientation.Horizontal;
            HorizontalOptions = LayoutOptions.FillAndExpand;
        }

        public View InitialView
        {
            get => (View) GetValue(InitialViewProperty);
            set => SetValue(InitialViewProperty, value);
        }

        public static readonly BindableProperty InitialViewProperty = BindableProperty.Create(
            nameof(InitialView), typeof(View), typeof(SwipableListItem), null, BindingMode.TwoWay,
            propertyChanged: OnInitialViewChanged);

        public View SwippedView
        {
            get => (View) GetValue(SwippedViewProperty);
            set => SetValue(SwippedViewProperty, value);
        }

        public static readonly BindableProperty SwippedViewProperty = BindableProperty.Create(
            nameof(SwippedView), typeof(View), typeof(SwipableListItem), null, BindingMode.TwoWay,
            propertyChanged: OnSwippedViewChanged);

        public bool IsSwipable
        {
            get => (bool)GetValue(IsSwipableProperty);
            set => SetValue(IsSwipableProperty, value);
        }

        public static readonly BindableProperty IsSwipableProperty = BindableProperty.Create(
            nameof(IsSwipable), typeof(bool), typeof(SwipableListItem), true);

        /// <summary>
        /// Shows the initial view before swipe.
        /// </summary>
        public void ShowInitialView()
        {
            if (!IsSwipable)
            {
                return;
            }

            InitialView.IsVisible = true;
            SwippedView.IsVisible = false;
        }

        /// <summary>
        /// Shows the view after swipe.
        /// </summary>
        public void ShowSwippedView()
        {
            if (!IsSwipable)
            {
                return;
            }

            InitialView.IsVisible = false;
            SwippedView.IsVisible = true;
        }

        /// <summary>
        /// Handles changed initial view.
        /// </summary>
        /// <param name="aBindable"> The bindable object. </param>
        /// <param name="aOldValue"> The old value. </param>
        /// <param name="aNewValue"> The new value. </param>
        private static void OnInitialViewChanged(BindableObject aBindable, object aOldValue, object aNewValue)
        {
            var control = (SwipableListItem) aBindable;
            var newView = (View) aNewValue;
            var oldView = (View) aOldValue;

            if (oldView != null)
            {
                control.Children.Remove(oldView);
            }

            if (newView != null)
            {
                newView.IsVisible = true;
                control.Children.Add(newView);
                control.InvalidateLayout();
            }
        }

        /// <summary>
        /// Handles changed swipped view.
        /// </summary>
        /// <param name="aBindable"> The bindable object. </param>
        /// <param name="aOldValue"> The old value. </param>
        /// <param name="aNewValue"> The new value. </param>
        private static void OnSwippedViewChanged(BindableObject aBindable, object aOldValue, object aNewValue)
        {
            var control = (SwipableListItem) aBindable;
            var newView = (View) aNewValue;
            var oldView = (View) aOldValue;

            if (oldView != null)
            {
                control.Children.Remove(oldView);
            }

            if (newView != null)
            {
                newView.IsVisible = false;

                control.Children.Add(newView);
                control.InvalidateLayout();
            }
        }
    }
}
