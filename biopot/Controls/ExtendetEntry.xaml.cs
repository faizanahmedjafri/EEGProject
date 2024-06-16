using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.ComponentModel.Design;
using biopot.Helpers;
using biopot.ViewModels;
using biopot.Resources.Strings;

namespace biopot.Controls
{
	public partial class ExtendetEntry : ContentView
	{

		public ExtendetEntry()
		{
			InitializeComponent();

			string savedEntryText = string.Empty;

			entry.Focused += (sender, e) =>
			{
				line.BackgroundColor = StyleManager.GetAppResource<Color>("entrySeparatorColor");

			    if (IsCleanOnFocus)
			    {
			        savedEntryText = entry.Text;
			        entry.Text = string.Empty; //clear field after tap on it    
                }
			};
			entry.Unfocused += (sender, e) =>
			{
				line.BackgroundColor = StyleManager.GetAppResource<Color>("entrySeparatorColorDefault");
				if (IsCleanOnFocus && string.IsNullOrEmpty(entry.Text))
				{
					entry.Text = savedEntryText;
				}
			};
		}

		private bool _IsMailEntry;
		public bool IsMailEntry
		{
			get => _IsMailEntry;
			set
			{
				_IsMailEntry = value;
				textValidateBehavior.IsMailEntry = _IsMailEntry;
			}
		}

		private bool _IsNeedTextValidator = true;
		public bool IsNeedTextValidator
		{
			get => _IsNeedTextValidator;
			set
			{
				_IsNeedTextValidator = value;
				textValidateBehavior.IsNeedTextValidator = _IsNeedTextValidator;
			}
		}

	    public static readonly BindableProperty IsCleanOnFocusProperty = BindableProperty.Create
	        (nameof(IsCleanOnFocus), typeof(bool), typeof(ExtendetEntry), true, BindingMode.TwoWay);

        public bool IsCleanOnFocus
        {
	        get => (bool)GetValue(IsCleanOnFocusProperty);
            set => SetValue(IsCleanOnFocusProperty, value);
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create
		   (nameof(Text), typeof(string), typeof(ExtendetEntry), default(string), BindingMode.TwoWay);
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create
			(nameof(Placeholder), typeof(string), typeof(ExtendetEntry), default(string));
		public string Placeholder
		{
			get { return (string)GetValue(PlaceholderProperty); }
			set { SetValue(PlaceholderProperty, value); }
		}

		public double FontSize
		{
			get { return this.entry.FontSize; }
			set { this.entry.FontSize = value; }
		}
	}
}
