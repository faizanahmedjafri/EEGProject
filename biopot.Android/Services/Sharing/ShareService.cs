using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Support.V4.Content;
using biopot.Resources.Strings;
using biopot.Services.Sharing;

namespace biopot.Droid.Services.Sharing
{
    public class ShareService : IShareService
    {
        private const string PROVIDER_PATH = "com.meffort.biopot.fileprovider";

        #region -- IShareService implementation --

        public async Task ShareFileAsync(IEnumerable<string> paths)
        {
            if (paths.Count() != 0)
            {
                Intent intent;

                //Implementation for one file
                if (paths.Count() == 1)
                {
                    intent = new Intent(Intent.ActionSend);
                    intent.SetType("text/*");

                    IParcelable parcelable = null;
                    parcelable = GetPhileUri(paths.FirstOrDefault());

                    intent.PutExtra(Intent.ExtraStream, parcelable);
                }
                else
                {
                    intent = new Intent(Intent.ActionSendMultiple);
                    intent.SetType("text/*");

                    List<IParcelable> parcelables = new List<IParcelable>();

                    foreach (var path in paths)
                    {
                        parcelables.Add(GetPhileUri(path));
                    }

                    intent.PutParcelableArrayListExtra(Intent.ExtraStream, parcelables);
                }
                StartActivity(intent);
            }
            else
            {
                return;
            }
        }

        #endregion

        #region -- Private helpers --

        private void StartActivity(Intent intent)
        {
            var intentChooser = Intent.CreateChooser(intent, Strings.ShareVia);
            intentChooser.SetFlags(ActivityFlags.GrantReadUriPermission);

            ((Activity)Xamarin.Forms.Forms.Context).StartActivityForResult(intentChooser, 0);
        }

        private Uri GetPhileUri(string path)
        {
            Uri uriFilePath = null;

            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
            {
                uriFilePath = Uri.Parse("content://" + path);
            }
            else
            {
                uriFilePath = FileProvider.GetUriForFile((Activity)Xamarin.Forms.Forms.Context, PROVIDER_PATH, new Java.IO.File(path));
            }

            return uriFilePath;
        }

        #endregion
    }
}
