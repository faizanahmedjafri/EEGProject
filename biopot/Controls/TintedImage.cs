using Xamarin.Forms;

namespace biopot.Controls
{
    /// <summary>
    /// Represent image view which can display icons with tint effect.
    /// </summary>
    public class TintedImage : Image
    {
        /// <summary>
        /// Tint effect color bindable property.
        /// </summary>
        public static readonly BindableProperty TintColorProperty =
            BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(TintedImage), Color.Black);

        /// <summary>
        /// Tint color property.
        /// </summary>
        public Color TintColor
        {
            get { return (Color) GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }
    }
}
