using System;
using biopot.ViewModels;
using Xamarin.Forms;
namespace biopot.ViewModels
{
	public class UserDetailsViewModel : BaseViewModel
	{
		#region -- Public properties --

		private string _Name = "Dr, Anonymous";
		public string Name
		{
			get { return _Name; }
			set { SetProperty(ref _Name, value); }
		}

		private string _EmpId = "0000";
		public string EmpId
		{
			get { return _EmpId; }
			set { SetProperty(ref _EmpId, value); }
		}

		private string _Role = "Doc";
		public string Role
		{
			get { return _Role; }
			set { SetProperty(ref _Role, value); }
		}

		private string _Email = "No Data";
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
