﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Android.Graphics;
using Xamarin.Forms;
using AApplication = Android.App.Application;

namespace biopot.Droid.Extensions
{
	public static class FontExtensions
	{
		private static readonly Dictionary<Tuple<string, FontAttributes>, Typeface> _typefaces = new Dictionary<Tuple<string, FontAttributes>, Typeface>();

		// We don't create and cache a Regex object here because we may not ever need it, and creating Regexes is surprisingly expensive (especially on older hardware)
		// Instead, we'll use the static Regex.IsMatch below, which will create and cache the regex internally as needed. It's the equivalent of Lazy<Regex> with less code.
		// See https://msdn.microsoft.com/en-us/library/sdx2bds0(v=vs.110).aspx#Anchor_2
		const string _loadFromAssetsRegex = @"\w+\.((ttf)|(otf))\#\w*";

		static Typeface _defaultTypeface;

		public static float ToScaledPixel(this Font self)
		{
			if (self.IsDefault)
				return 14;

			if (self.UseNamedSize)
			{
				switch (self.NamedSize)
				{
					case NamedSize.Micro:
						return 10;

					case NamedSize.Small:
						return 12;

					case NamedSize.Default:
					case NamedSize.Medium:
						return 14;

					case NamedSize.Large:
						return 18;
				}
			}

			return (float)self.FontSize;
		}

		public static Typeface ToTypeface(this Font self)
		{
			if (self.IsDefault)
				return _defaultTypeface ?? (_defaultTypeface = Typeface.Default);

			var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
			Typeface result;
			if (_typefaces.TryGetValue(key, out result))
				return result;

			if (self.FontFamily == null)
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(Typeface.Default, style);
			}
			else if (Regex.IsMatch(self.FontFamily, _loadFromAssetsRegex))
			{
				result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(self.FontFamily));
			}
			else
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(self.FontFamily, style);
			}
			return (_typefaces[key] = result);
		}

		internal static bool IsDefault(this Entry self)
		{
			return self.FontFamily == null && self.FontAttributes == FontAttributes.None;
		}

		internal static Typeface ToTypeface(this Entry self)
		{
			if (self.IsDefault())
				return _defaultTypeface ?? (_defaultTypeface = Typeface.Default);

			var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
			Typeface result;
			if (_typefaces.TryGetValue(key, out result))
				return result;

			if (self.FontFamily == null)
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(Typeface.Default, style);
			}
			else if (Regex.IsMatch(self.FontFamily, _loadFromAssetsRegex))
			{
				result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(self.FontFamily));
			}
			else
			{
				var style = ToTypefaceStyle(self.FontAttributes);
				result = Typeface.Create(self.FontFamily, style);
			}
			return (_typefaces[key] = result);
		}

		public static TypefaceStyle ToTypefaceStyle(FontAttributes attrs)
		{
			var style = TypefaceStyle.Normal;
			if ((attrs & (FontAttributes.Bold | FontAttributes.Italic)) == (FontAttributes.Bold | FontAttributes.Italic))
				style = TypefaceStyle.BoldItalic;
			else if ((attrs & FontAttributes.Bold) != 0)
				style = TypefaceStyle.Bold;
			else if ((attrs & FontAttributes.Italic) != 0)
				style = TypefaceStyle.Italic;
			return style;
		}

		static string FontNameToFontFile(string fontFamily)
		{
			int hashtagIndex = fontFamily.IndexOf('#');
			if (hashtagIndex >= 0)
				return fontFamily.Substring(0, hashtagIndex);

			throw new InvalidOperationException($"Can't parse the {nameof(fontFamily)} {fontFamily}");
		}
	}
}
