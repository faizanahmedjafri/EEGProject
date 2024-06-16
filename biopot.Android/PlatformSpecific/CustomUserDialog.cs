using System;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Support.V7.App;

namespace biopot.Droid.PlatformSpecific
{
    public class CustomUserDialog : UserDialogsImpl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getTopActivity">Method which provide top most activity </param>
        public CustomUserDialog(Func<Activity> getTopActivity)
            : base(getTopActivity)
        {
        }

        /// <summary>
        /// The AppCompat activity instance.
        /// </summary>
        private AppCompatActivity AppCompatActivity => (AppCompatActivity)TopActivityFunc();

        /// <inheritdoc />
        public override async Task AlertAsync(AlertConfig config,
            CancellationToken? cancelToken = default(CancellationToken?))
        {
            int resourceId = AppCompatActivity.Resources.GetIdentifier("AppCompatDialogDarkStyle", "style", 
                AppCompatActivity.PackageName);
            if (resourceId > 0)
            {
                config.AndroidStyleId = resourceId;
            }

            await base.AlertAsync(config, cancelToken);
        }
    }
}