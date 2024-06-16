using System;
using System.Collections.Generic;
using Xamarin.Forms;
using biopot.Resources.Strings;

namespace biopot.Controls
{
	public partial class SavingTargetPicker : ContentView
	{
		public SavingTargetPicker()
		{
			InitializeComponent();
		}

		private string _TitleText;
		public string TitleText
		{
			get { return _TitleText; }
			set { _TitleText = value; OnPropertyChanged(nameof(TitleText)); }
		}

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create
			   (nameof(SelectedItem), typeof(string), typeof(SavingTargetPicker), default(string), BindingMode.TwoWay);
		public string SelectedItem
		{
			get { return (string)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly BindableProperty TargetsListProperty = BindableProperty.Create
			(nameof(TargetsList), typeof(IList<string>), typeof(SavingTargetPicker), default(IList<string>), propertyChanged: OnTargetsListChanged);

		public IList<string> TargetsList
		{
			get { return (IList<string>)GetValue(TargetsListProperty); }
			set { SetValue(TargetsListProperty, value); }
		}

		private static void OnTargetsListChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var _this = (SavingTargetPicker)bindable;
		}

		private void OnSelectedIndexChanged(object sender, System.EventArgs e)
		{
			var picker = (Picker)sender;
			if (picker.SelectedItem != null)
			{
				label.Text = picker.SelectedItem.ToString();
			}
		}

	}
}
