using System;
using System.IO;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using biopot.Droid.Services;
using SharedCore.Enums;
using SharedCore.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(FolderService))]
namespace biopot.Droid.Services
{
	public class FolderService : IFolderService
	{
		private const string PROVIDER_PATH = "com.meffort.biopot.fileprovider";

		private const string DEFAULT_FOLDER = "biopot";
		private Context _context;
		private ESaveDataWays _currentWay;
		private readonly string[] Permissions =
		{
		  Manifest.Permission.WriteExternalStorage,
		  Manifest.Permission.ReadExternalStorage
		};

		public FolderService(Context context)
		{
			_context = context;
		}

		#region --IFolderService--

		public string GetFolderPath(ESaveDataWays way, string innerFolder)
		{
			_currentWay = way;

			string path = GetStoragePath(way);

			path = PreparePath(path, innerFolder);

			return path;
		}

		public double GetAvailableSpaceMb(ESaveDataWays way)
		{
			string path = GetStoragePath(way);

			var st = new StatFs(path);

			return st.AvailableBytes / 1024 / 1024;
		}


		public bool HasPermissions()
		{
			bool result = CheckPermissions();
			if (!result)
			{
				AskPermissions();
				result = CheckPermissions();
			}

			return result;
		}

		#endregion

		#region --Private helpers--

		private bool CheckPermissions()
		{
			return ContextCompat.CheckSelfPermission(_context, Manifest.Permission.ReadExternalStorage) == Permission.Granted
								&& ContextCompat.CheckSelfPermission(_context, Manifest.Permission.WriteExternalStorage) == Permission.Granted;
		}

		private void AskPermissions()
		{
			((MainActivity)_context).RequestPermissions(Permissions, 0);
		}

		private string GetStoragePath(ESaveDataWays way)
		{
			string path;

			switch (way)
			{
				case ESaveDataWays.Device:
					path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
					break;
				case ESaveDataWays.SD:
					var dirs = _context.GetExternalFilesDirs(null);
					path = GetExternalStorage(dirs);
					break;
				case ESaveDataWays.Usb:
					throw new NotImplementedException();
					break;
				default:
					throw new ArgumentException();
			}
			return path;
		}

		private string GetExternalStorage(Java.IO.File[] files)
		{
			string path = string.Empty;

			if (files != null && files.Length > 0)
			{
				foreach (var file in files)
				{
					if (!file.AbsolutePath.Contains("emulated"))
					{
						path = GetRootDir(file);
						break;
					}
				}
			}

			return path;
		}

		private string GetRootDir(Java.IO.File file)
		{
			string path = string.Empty;

			var arr = file.AbsolutePath.Split('/');
			if (arr.Length > 2)
			{
				path += $"{arr[0]}/{arr[1]}/{arr[2]}/";
			}

			return path;
		}

		private string PreparePath(string path, string innerFolder)
		{

			if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Lollipop)
			{
				path = Path.Combine(path, innerFolder);
			}
			else
			{
				path = (_currentWay == ESaveDataWays.Device) ?
					Path.Combine(path, innerFolder) :
					FileProvider.GetUriForFile(_context, PROVIDER_PATH, new Java.IO.File(path)).Path + "/Android/data/com.meffort.biopot/files/" + innerFolder;
			}

			CreateDefaultFolderIfNeed(path);

			return path;
		}

		private void CreateDefaultFolderIfNeed(string path)
		{
			try
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
			catch (Exception ex)
			{

			}
		}

		#endregion
	}
}