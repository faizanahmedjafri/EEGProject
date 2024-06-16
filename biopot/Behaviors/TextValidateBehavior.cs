using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace biopot.Behaviors
{
	public class TextValidateBehavior : Behavior<Entry>
	{
		private const string TEXT_PATTERN = "^[^\\\\/?%*:|\"<>.!@]+$";
		private const string EMAIL_PATTERN = "^[^\\\\/?%*:|\"<>!]+$";
		private Regex _regex;

		private bool _IsMailEntry;
		public bool IsMailEntry
		{
			get => _IsMailEntry;
			set
			{
				_IsMailEntry = value;
			}
		}

		private bool _IsNeedTextValidator;
		public bool IsNeedTextValidator
		{
			get => _IsNeedTextValidator;
			set
			{
				_IsNeedTextValidator = value;
			}
		}

		protected override void OnAttachedTo(Entry entry)
		{
			entry.TextChanged += OnEntryTextChanged;
			entry.Unfocused += OnEntryFocuseChanged;
			base.OnAttachedTo(entry);
		}

		protected override void OnDetachingFrom(Entry entry)
		{
			entry.TextChanged -= OnEntryTextChanged;
			entry.Unfocused -= OnEntryFocuseChanged;
			base.OnDetachingFrom(entry);
		}

		private void OnEntryFocuseChanged(object sender, FocusEventArgs e)
		{
			var entry = sender as Entry;

			if (_regex == null)
			{
				string pattern = IsMailEntry ? EMAIL_PATTERN : TEXT_PATTERN;
				_regex = new Regex(pattern);
			}

			var text = entry.Text;
			bool isValid = _regex.IsMatch(text ?? "");

			if (!isValid)
			{
				entry.Text = "";
			}
		}

		private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
		{
			var entry = sender as Entry;
			if (_regex == null)
			{
				string pattern = IsMailEntry ? EMAIL_PATTERN : TEXT_PATTERN;
				_regex = new Regex(pattern);
			}

			var text = entry.Text;
			if (string.IsNullOrWhiteSpace(text))
				return;

			bool isValid = _regex.IsMatch(text);

			if (isValid || !IsNeedTextValidator)
			{
				entry.Text = text;
			}
			else
			{
				entry.Text = text.Remove(text.Length - 1);
			}
		}
	}
}
