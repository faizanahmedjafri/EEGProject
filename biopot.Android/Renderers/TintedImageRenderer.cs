using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using biopot.Controls;
using biopot.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(TintedImage), typeof(TintedImageRenderer))]
namespace biopot.Droid.Renderers
{
    /// <summary>
    /// Represents renderer for custom tinting image control
    /// </summary>
    public class TintedImageRenderer: ImageRenderer
    {
        public TintedImageRenderer(Context context) : base(context)
        {
        }

        /// <inheritdoc />
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            SetTint();
        }

        /// <inheritdoc />
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == TintedImage.TintColorProperty.PropertyName)
                SetTint();
        }

        /// <summary>
        /// Set tinting to image
        /// </summary>
        private void SetTint()
        {
            if (Control == null || Element == null)
                return;

            if (!((TintedImage)Element).TintColor.Equals(Xamarin.Forms.Color.Transparent))
            {
                //Apply tint color
                var colorFilter = new PorterDuffColorFilter(((TintedImage)Element).TintColor.ToAndroid(),
                    PorterDuff.Mode.SrcIn);
                Control.SetColorFilter(colorFilter);
            }
            else
            {
                //Turn off tinting
                if (Control.ColorFilter != null)
                    Control.ClearColorFilter();
            }
        }
    }
}