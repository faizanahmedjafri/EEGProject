using Android.App;
using Android.Content.PM;
using Android.OS;
using Acr.UserDialogs;
using Prism;
using Prism.Ioc;
using NControl.Droid;
using NControl.Controls.Droid;
using Android;
using Android.Support.V4.App;
using biopot.Droid.Services;
using Android.Content;
using biopot.Services.Sharing;
using biopot.Droid.Services.Sharing;
using SharedCore.Services;
using Android.Views;
using biopot.Droid.PlatformSpecific;
using biopot.Services;
using Prism.Events;

namespace biopot.Droid
{
    //H.H. : change application name here , SearchKey: APPNAME/APP_NAME/ApplicationName.
    [Activity(Label = "BIOPOT3", Icon = "@mipmap/ic_launch", Theme = "@style/MyTheme.Splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		private readonly string[] Permissions =
		{
		  Manifest.Permission.Bluetooth,
		  Manifest.Permission.BluetoothAdmin,
		  Manifest.Permission.BluetoothPrivileged,
		  Manifest.Permission.AccessCoarseLocation,
		  Manifest.Permission.AccessFineLocation,
		  Manifest.Permission.WriteExternalStorage,
		  Manifest.Permission.ReadExternalStorage,
          Manifest.Permission.Camera,
		};

		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;
			this.SetTheme(Resource.Style.MyTheme_Empty);

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);
		    UserDialogs.Instance = new CustomUserDialog(() => this);
            NControlViewRenderer.Init();
			NControls.Init();
		    ZXing.Net.Mobile.Forms.Android.Platform.Init();

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			SetPoliciesForSharing();

			LoadApplication(new App(new AndroidInitializer(this)));
			CheckPermissions();
		}

	    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
	    {
            ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // raise event that permissions have changed
            App.Resolve<IEventAggregator>().GetEvent<SystemPermissionsChangedEvent>().Publish();
        }

	    private void SetPoliciesForSharing()
		{
			StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
			StrictMode.SetVmPolicy(builder.Build());
		}

		private void CheckPermissions()
		{
			bool minimumPermissionsGranted = true;

			if (!(ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted
				&& ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted
				&& ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Bluetooth) != Permission.Granted
				&& ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothAdmin) != Permission.Granted
				&& ActivityCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothPrivileged) != Permission.Granted
				&& ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted
				&& ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted
				&& ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Permission.Granted))
			{
				minimumPermissionsGranted = false;
			}

			// If one of the minimum permissions aren't granted, we request them from the user
			if (!minimumPermissionsGranted)
				RequestPermissions(Permissions, 0);
		}
	}

	public class AndroidInitializer : IPlatformInitializer
	{
		private Context _context;

		public AndroidInitializer(Context context)
		{
			_context = context;
		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.RegisterInstance<IFolderService>(new FolderService(_context));
			containerRegistry.RegisterInstance<IShareService>(new ShareService());
			containerRegistry.RegisterInstance<ISoundService>(new SoundService());
            containerRegistry.RegisterInstance<IPermissionsRequester>(new DroidPermissionsRequester(_context));
		}
	}
}

