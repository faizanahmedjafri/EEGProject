using System;
using biopot.ViewModels;

namespace biopot.ViewModels
{
	public class OtherDetailsViewModel : BaseViewModel
	{
		#region -- Public properties --

		private string _Name;
		public string Name
		{
			get { return _Name; }
			set { SetProperty(ref _Name, value); }
		}

		private string _EmpId;
		public string EmpId
		{
			get { return _EmpId; }
			set { SetProperty(ref _EmpId, value); }
		}

		private string _Role;
		public string Role
		{
			get { return _Role; }
			set { SetProperty(ref _Role, value); }
		}

		private string _Email;
		public string Email
		{
			get { return _Email; }
			set { SetProperty(ref _Email, value); }
		}

		private string _ErrorMessage = "";
		public string ErrorMessage
		{
			get { return _ErrorMessage; }
			set { SetProperty(ref _ErrorMessage, value); }
		}

		#endregion

	}
}
