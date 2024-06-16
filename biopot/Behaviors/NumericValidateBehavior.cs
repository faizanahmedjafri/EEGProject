using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace biopot.Behaviors
{
	public class NumericValidateBehavior : Behavior<Entry>
	{
		private Regex _regex = new Regex(@"^\d+$");
		private string _lastValidText = default(string);

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

			var text = entry.Text;
			bool isValid = _regex.IsMatch(text);

			if (!isValid)
			{
				entry.Text = _lastValidText;
			}

		}

		private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
		{
			var entry = sender as Entry;

			var text = entry.Text;
			bool isValid = _regex.IsMatch(text);

			if (string.IsNullOrWhiteSpace(text))
				return;

			if (isValid)
			{
				entry.Text = text;
				_lastValidText = text;
			}
			else
			{
				entry.Text = text.Remove(text.Length - 1);
			}
		}
	}
}
