using System;
using biopot.Resources.Strings;
using biopot.ViewModels;
using biopot.ViewModels;
using Prism.Navigation;

namespace biopot.ViewModels
{
	public class SessionViewModel : BaseViewModel
	{
		#region -- Public properties --

		private string _SavingTarget = Strings.InternalMemory;
		public string SavingTarget
		{
			get { return _SavingTarget; }
			set
			{
				//TODO hack for bloc USBCable and WirelessStick
				string val = string.Empty;
				if (value == Strings.USBCable || value == Strings.WirelessStick)
				{
					val = Strings.InternalMemory;
				}
				else
				{
					val = value;
				}
				_SavingTarget = val;
			}
		}

		private string _FileName = Strings.DefaultFileName;
		public string FileName
		{
			get { return _FileName; }
			set
			{
				if (value != null && value.Length > Constants.MAXIMUM_FILE_NAME_LENGTH)
					value = value.Substring(0, Constants.MAXIMUM_FILE_NAME_LENGTH);
				_FileName = value;
				RaisePropertyChanged(nameof(FileName));
				BuildExampleName();
			}
		}

		private string _FolderName = Strings.DefaultFolderName;
		public string FolderName
		{
			get { return _FolderName; }
			set
			{
				if (value != null && value.Length > Constants.MAXIMUM_FOLDER_NAME_LENGTH)
					value = value.Substring(0, Constants.MAXIMUM_FOLDER_NAME_LENGTH);
				_FolderName = value;
				RaisePropertyChanged(nameof(FolderName));
			}
		}

		private string _ErrorMessage = "";
		public string ErrorMessage
		{
			get { return _ErrorMessage; }
			set { SetProperty(ref _ErrorMessage, value); }
		}

		private bool _IsDateInName = true;
		public bool IsDateInName
		{
			get { return _IsDateInName; }
			set { SetProperty(ref _IsDateInName, value); BuildExampleName(); }
		}

		private bool _IsTimeInName = true;
		public bool IsTimeInName
		{
			get { return _IsTimeInName; }
			set { SetProperty(ref _IsTimeInName, value); BuildExampleName(); }
		}

		private bool _IsSequentialNumberInName = true;
		public bool IsSequentialNumberInName
		{
			get { return _IsSequentialNumberInName; }
			set { SetProperty(ref _IsSequentialNumberInName, value); BuildExampleName(); }
		}

		private string _ExampleName;
		public string ExampleName
		{
			get { return _ExampleName; }
			set { SetProperty(ref _ExampleName, value); }
		}

		#endregion

		#region -- Private helpers --

		private void BuildExampleName()
		{
			ExampleName = "";
			ExampleName += String.Format("{0}_", FileName.Length > 10 ? FileName.Substring(0, 10) + "…" : FileName);
			ExampleName += IsDateInName ? "20180624_" : "";
			ExampleName += IsTimeInName ? "22.30.41_" : "";
			ExampleName += IsSequentialNumberInName ? "0001" : "";

			if (ExampleName == "")
				ExampleName = "BioPot_20180624_22:30.41_0001";

			if (ExampleName[0] == '_')
				ExampleName = ExampleName.Remove(0, 1);

			if (ExampleName[ExampleName.Length - 1] == '_')
				ExampleName = ExampleName.Remove(ExampleName.Length - 1);
		}

		#endregion
	}
}
