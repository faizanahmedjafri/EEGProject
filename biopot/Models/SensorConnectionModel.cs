using System;
using biopot.ViewModels;
using Xamarin.Forms;
using Prism.Mvvm;
using System.Runtime.CompilerServices;

namespace biopot.Models
{
	public class SensorConnectionModel : BindableBase
	{
		public int SensorId { get; set; }
		//public int SignalValue { get; set; }
		public bool IsActive { get; set; }
		public ChartViewModel Chart { get; set; }

		private int _SignalValue;
		public int SignalValue
		{
			get { return _SignalValue; }
			set { SetProperty(ref _SignalValue, value); }
		}
	}
}
