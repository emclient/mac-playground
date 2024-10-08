using System.Collections.Generic;
using System.Globalization;

#if MAC || IOS
using CoreFoundation;
using Foundation;
#endif

namespace System.Windows.Forms
{
#if MAC || IOS
	static class DateTimeUtility
	{
		static object localeDidChangeNotification = NSLocale.Notifications.ObserveCurrentLocaleDidChange(OnLocaleDidChange);
		static DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		static DateTimeFormatInfo cdtf = null;
		static object cdtfLock = new object();
		static Dictionary<string, NSDateFormatter> formatters = null;

		public static event EventHandler LocaleDidChange;

		static void OnLocaleDidChange(object sender, EventArgs args)
		{
			preferredCulture = null;
			preferredLocale = null;
			baselineFormatter = null;
			formatters = null;
			cdtf = null;

			CultureInfo.CurrentCulture = PreferredCulture;

			if (LocaleDidChange != null)
				LocaleDidChange(sender, new EventArgs());
		}

		public static bool IsUsing12HourFormat
		{
			get
			{
				var formatter = Formatters["t"];
				var s = formatter.ToString(new NSDate());
				return s.IndexOf(formatter.AMSymbol) >= 0 || s.IndexOf(formatter.PMSymbol) >= 0;
			}
		}

		static Dictionary<string, NSDateFormatter> Formatters
		{
			get
			{
				if (formatters == null)
					formatters = CreateFormatters();
				return formatters;
			}

		}

		static NSLocale preferredLocale = null;
		public static NSLocale PreferredLocale
		{
			get
			{
				if (preferredLocale == null)
				{
					var defaults = NSUserDefaults.StandardUserDefaults;
					var languages = defaults["AppleLanguages"] as NSArray;
					var preferred = CFString.FromHandle(languages.ValueAt(0));
					preferredLocale = NSLocale.FromLocaleIdentifier(preferred);
				}
				return preferredLocale;
			}
		}

		static CultureInfo preferredCulture = null;
		public static CultureInfo PreferredCulture
		{
			// The tests for neutral culture are here to prevent crash when getting the current region
			get
			{
				if (preferredCulture == null)
				{
					var locale = NSLocale.AutoUpdatingCurrentLocale;
					try
					{
						var c = CultureInfo.GetCultureInfo(locale.Identifier);
						if (!c.IsNeutralCulture)
							return preferredCulture = c;
					}
					catch { }

					var languageCode = locale.LanguageCode ?? "en";
					if (locale.CountryCode != null)
					{
						try
						{
							var c = CultureInfo.GetCultureInfo($"{languageCode}-{locale.CountryCode}");
							if (!c.IsNeutralCulture)
								return preferredCulture = c;
						}
						catch { }
					}

					if (locale.ScriptCode != null)
					{
						try
						{
							var c = CultureInfo.GetCultureInfo($"{languageCode}-{locale.ScriptCode}");
							if (!c.IsNeutralCulture)
								return preferredCulture = c;
						}
						catch { }
					}

					try
					{
						var c = CultureInfo.CreateSpecificCulture(languageCode);
						if (!c.IsNeutralCulture)
							return preferredCulture = c;
					}
					catch { }

					try
					{
						var c = CultureInfo.GetCultureInfo($"{languageCode}-US");
						if (!c.IsNeutralCulture)
							return preferredCulture = c;
					}
					catch { }

					try
					{
						var c = CultureInfo.CreateSpecificCulture($"{languageCode}-US");
						if (!c.IsNeutralCulture)
							return preferredCulture = c;
					}
					catch { }

					try
					{
						var c = CultureInfo.CurrentCulture;
						if (!c.IsNeutralCulture)
							return preferredCulture = c;
					}
					catch { }

					preferredCulture = CultureInfo.GetCultureInfo("en-US");
				}
				return preferredCulture;
			}
		}

		static NSDateFormatter baselineFormatter;
		static NSDateFormatter BaselineFormatter
		{
			get
			{
				if (baselineFormatter == null)
				{
					baselineFormatter = new NSDateFormatter();
					baselineFormatter.Locale = new NSLocale(NSLocale.CurrentLocale.Identifier);
				}
				return baselineFormatter;
			}
		}

		static Dictionary<string, NSDateFormatter> CreateFormatters()
		{
			var locale = NSLocale.AutoUpdatingCurrentLocale;

			Dictionary<string, NSDateFormatter> d = new Dictionary<string, NSDateFormatter>();

			NSDateFormatter f = new NSDateFormatter();

			//case "d": // 6 / 15 / 2008
			f.DateStyle = NSDateFormatterStyle.Short;
			f.TimeStyle = NSDateFormatterStyle.None;
			d.Add("d", NewDateFormatter(f, locale));

			//case "D": // Sunday, June 15, 2000
			f.DateStyle = NSDateFormatterStyle.Full;
			f.TimeStyle = NSDateFormatterStyle.None;
			d.Add("D", NewDateFormatter(f, locale));

			//case "f": //Sunday, June 15, 2008 9:15 PM
			f.DateStyle = NSDateFormatterStyle.Full;
			f.TimeStyle = NSDateFormatterStyle.Short;
			d.Add("f", NewDateFormatter(f, locale));

			//case "F": //Sunday, June 15, 2008 9:15:07 PM
			f.DateStyle = NSDateFormatterStyle.Full;
			f.TimeStyle = NSDateFormatterStyle.Medium;
			d.Add("F", NewDateFormatter(f, locale));

			//case "g": //6/15/2008 9:15 PM
			f.DateStyle = NSDateFormatterStyle.Short;
			f.TimeStyle = NSDateFormatterStyle.Short;
			d.Add("g", NewDateFormatter(f, locale));

			//case "G": //6/15/2008 9:15:07 PM
			f.DateStyle = NSDateFormatterStyle.Short;
			f.TimeStyle = NSDateFormatterStyle.Medium;
			d.Add("G", NewDateFormatter(f, locale));

			//case "m": //June 15
			//case "o": //2008-06-15T21:15:07.0000000
			//case "R": //Sun, 15 Jun 2008 21:15:07 GMT
			//case "s": //2008-06-15T21:15:00
			//case "u": //2008-06-15 21:15:07Z
			//case "U": //Monday, June 16, 2008 4:15:07 AM
			//case "y": //June, 2008

			//case "t": //9:15 PM
			f.DateStyle = NSDateFormatterStyle.None;
			f.TimeStyle = NSDateFormatterStyle.Short;
			d.Add("t", NewDateFormatter(f, locale));

			//case "T": //9:15:07 PM
			f.DateStyle = NSDateFormatterStyle.None;
			f.TimeStyle = NSDateFormatterStyle.Medium;
			d.Add("T", NewDateFormatter(f, locale));

			return d;
		}

		static NSDateFormatter NewDateFormatter(NSDateFormatter current, NSLocale locale)
		{
			var dateStyle = current.DateStyle;
			var timeStyle = current.TimeStyle;

			var baseline = BaselineFormatter;

			var merged = new NSDateFormatter();
			merged.Locale = locale;

			merged.DateStyle = baseline.DateStyle = current.DateStyle = dateStyle;
			merged.TimeStyle = baseline.TimeStyle = current.TimeStyle = NSDateFormatterStyle.None;
			var mergedDateFormat = merged.DateFormat;
			var baselineDateFormat = baseline.DateFormat;
			var currentDateFormat = current.DateFormat;

			merged.DateStyle = baseline.DateStyle = current.DateStyle = NSDateFormatterStyle.None;
			merged.TimeStyle = baseline.TimeStyle = current.TimeStyle = timeStyle;
			var mergedTimeFormat = merged.DateFormat;
			var baselineTimeFormat = baseline.DateFormat;
			var currentTimeFormat = current.DateFormat;

			merged.DateStyle = baseline.DateStyle = current.DateStyle = dateStyle;
			merged.TimeStyle = baseline.TimeStyle = current.TimeStyle = timeStyle;

			if (!current.AMSymbol.Equals(baseline.AMSymbol))
				merged.AMSymbol = current.AMSymbol;
			if (!current.PMSymbol.Equals(baseline.PMSymbol))
				merged.PMSymbol = current.PMSymbol;

			if (!current.DateFormat.Equals(baseline.DateFormat) && !String.IsNullOrEmpty(mergedDateFormat))
			{
				var format = merged.DateFormat;
				if (baselineDateFormat != currentDateFormat)
					format = format.Replace(mergedDateFormat, currentDateFormat);
				if (baselineTimeFormat != currentTimeFormat)
					format = format.Replace(mergedTimeFormat, currentTimeFormat);
				merged.DateFormat = format;
			}

			return merged;
		}

		public static DateTimeFormatInfo CurrentFormat
		{
			get
			{
				lock (cdtfLock)
				{
					if (cdtf == null)
						cdtf = CreateDateTimeFormat();
					return cdtf;
				}
			}
		}

		static DateTimeFormatInfo CreateDateTimeFormat()
		{
			var dtfi = new DateTimeFormatInfo();

			var cul = PreferredCulture;
			dtfi.CalendarWeekRule = cul.DateTimeFormat.CalendarWeekRule;

			var f = Formatters["f"];
			
			dtfi.TrySetCalendar(cul.Calendar, cul.OptionalCalendars);
			dtfi.DateSeparator = GetDateSeparator();
			dtfi.TimeSeparator = GetTimeSeparator();
			dtfi.DayNames = f.StandaloneWeekdaySymbols;
			dtfi.MonthGenitiveNames = Append(f.StandaloneMonthSymbols, String.Empty, 13);
			dtfi.MonthNames = Append(f.MonthSymbols, String.Empty, 13);
			dtfi.AbbreviatedDayNames = f.ShortStandaloneWeekdaySymbols;
			dtfi.AbbreviatedMonthNames = Append(f.ShortStandaloneMonthSymbols, String.Empty, 13);
			dtfi.AbbreviatedMonthGenitiveNames = Append(f.ShortMonthSymbols, String.Empty, 13);
			dtfi.AMDesignator = f.AMSymbol;
			dtfi.PMDesignator = f.PMSymbol;
			dtfi.FirstDayOfWeek = cul.DateTimeFormat.FirstDayOfWeek;
			dtfi.FullDateTimePattern = DateTimePatternFromNative(Formatters["F"].DateFormat);
			dtfi.LongDatePattern = DateTimePatternFromNative(Formatters["D"].DateFormat);
			dtfi.LongTimePattern = DateTimePatternFromNative(Formatters["T"].DateFormat);
			dtfi.MonthDayPattern = cul.DateTimeFormat.MonthDayPattern; //FIXME
			dtfi.ShortDatePattern = DateTimePatternFromNative(Formatters["d"].DateFormat);
			dtfi.ShortestDayNames = f.ShortStandaloneWeekdaySymbols; // f.VeryShortWeekdaySymbols; // For Win 'compatibility'
			dtfi.ShortTimePattern = DateTimePatternFromNative(Formatters["t"].DateFormat);
			dtfi.YearMonthPattern = cul.DateTimeFormat.YearMonthPattern;

			return dtfi;
		}

		static bool TrySetCalendar(this DateTimeFormatInfo info, Calendar current, Calendar[] all)
		{
			if (info.TrySetCalendar(current))
				return true;

			foreach (var calendar in all)
				if (info.TrySetCalendar(calendar))
					return true;

			return false;
		}

		static bool TrySetCalendar(this DateTimeFormatInfo info, Calendar calendar)
		{
			try
			{
				info.Calendar = calendar;
				return true;
			}
			catch
			{
				return false;
			}
		}

		static string[] Append(string[] array, string value, int maxLength = -1)
		{
			if (maxLength >= 0 && array.Length >= maxLength)
				return array;

			var z = new string[1 + array.Length];
			array.CopyTo(z, 0);
			z[z.Length - 1] = value;
			return z;
		}

		static string DateTimePatternFromNative(string p)
		{
			p = ReplaceNotQuoted(p, "EEEE", "dddd");
			p = ReplaceNotQuoted(p, "E", "ddd");
			p = ReplaceNotQuoted(p, "a", "tt");
			p = ReplaceNotQuoted(p, 'y', "yyyy");
			p = ReplaceNotQuoted(p, "GGGGG", "gg");
			p = ReplaceNotQuoted(p, "G", "g");
			return p;
		}

		static string DateTimePatternToNative(string p)
		{
			p = ReplaceNotQuoted(p, "dddd", "EEEE");
			p = ReplaceNotQuoted(p, "ddd", "E");
			p = ReplaceNotQuoted(p, "tt", "aa");
			p = ReplaceNotQuoted(p, "t", "a");
			p = ReplaceNotQuoted(p, "yyyy", "y");
			p = ReplaceNotQuoted(p, "gg", "GGGGG");
			p = ReplaceNotQuoted(p, "g", "G");

			p = ReplaceNotQuoted(p, "mm", "*");
			p = ReplaceNotQuoted(p, "m", "MMM");
			p = ReplaceNotQuoted(p, "*", "mm");

			return p;
		}

		public static string ReplaceNotQuoted(string s, char what, string with)
		{
			return ReplaceNotQuoted(s, what.ToString(), with, true);
		}

		public static string ReplaceNotQuoted(string s, string what, string with, bool singleChar = false)
		{
			var q = false;
			var l = s.Length;
			for (int i = 0; i < s.Length; ++i)
			{
				var c = s[i];
				if (c == '\'')
				{
					q = !q;
					continue;
				}

				int pos = s.IndexOf(what, i);
				if (!q && pos == i && (!singleChar || IsSingular(s, pos)))
				{
					s = s.Substring(0, i) + with + s.Substring(i + what.Length);
					i += with.Length - 1;
					l = s.Length;
				}
			}
			return s;
		}

		public static string ReplaceAllNotQuoted(string s, string what, string with, bool singleChar = false)
		{
			while (true)
			{
				string next = DateTimeUtility.ReplaceNotQuoted(s, what, with);
				if (s == next)
					break;
				s = next;
			}
			return s;
		}

		static bool IsSingular(string s, int pos)
		{
			if (pos > 0 && s.Length > pos && s[pos-1] == s[pos])
				return false;
			if (s.Length > pos + 1 && s[pos+1] == s[pos])
				return false;
			return true;
		}

		static string GetDateSeparator()
		{
			return GetSeparator(Formatters["d"].DateFormat, DateTimeFormatInfo.InvariantInfo.DateSeparator);
		}

		static string GetTimeSeparator()
		{
			return GetSeparator(Formatters["t"].DateFormat, DateTimeFormatInfo.InvariantInfo.TimeSeparator);
		}

		static string GetSeparator(string pattern, string @default)
		{
			int l = -1, r = -1;
			for (int i = 0; i < pattern.Length; i++)
			{
				var c = pattern[i];
				if (l < 0)
				{
					if (!Char.IsLetter(c))
						l = i;
				}
				else if (r < 0)
				{
					if (Char.IsLetter(c))
					{
						r = i;
						return pattern.Substring(l, r - l);
					}
				}
			}
			return @default;
		}

		public static string ToString(DateTime dateTime)
		{
			return ToString(dateTime, null);
		}

		public static string ToString(DateTime dateTime, string? format)
		{
			if (String.IsNullOrEmpty(format))
				format = "G";

			if (Formatters.TryGetValue(format, out NSDateFormatter formatter))
				return formatter.ToString((NSDate)dateTime.UnspecifiedTo(DateTimeKind.Local));

			return ToString(dateTime, format, PreferredCulture);
		}

		public static string ToString(DateTime dateTime, string? format, CultureInfo culture)
		{
			if (culture.Calendar.MinSupportedDateTime <= dateTime && dateTime <= culture.Calendar.MaxSupportedDateTime)
				return dateTime.ToString(format, culture);

			return dateTime.ToString(format, CultureInfo.InvariantCulture);
		}

		internal static string ToSafeString(this DateTime dateTime)
		{
			return ToString(dateTime);
		}

		internal static string ToSafeString(this DateTime dateTime, string? format)
		{
			return ToString(dateTime, format);
		}

		public static string ToSafeString(this DateTime dateTime, string? format, CultureInfo culture)
		{
			return ToString(dateTime, format, culture);
		}

		public static DateTime UnspecifiedTo(this DateTime dateTime, DateTimeKind kind)
		{
			return dateTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dateTime, kind) : dateTime;
		}
	}
#else
	static class DateTimeUtility
	{
		public static CultureInfo PreferredCulture
		{
			get { return CultureInfo.CurrentCulture; }
		}

		public static DateTimeFormatInfo CurrentFormat
		{
			get { return PreferredCulture.DateTimeFormat; }
		}

		public static string ToString(DateTime dateTime)
		{
			return ToString(dateTime, null);
		}

		public static string ToString(DateTime dateTime, string? format)
		{
			return ToString(dateTime, format, CultureInfo.CurrentCulture);
		}

		public static string ToString(DateTime dateTime, string? format, CultureInfo culture)
		{
			if (culture.Calendar.MinSupportedDateTime <= dateTime && dateTime <= culture.Calendar.MaxSupportedDateTime)
				return dateTime.ToString(format, culture);
			return dateTime.ToString(format, CultureInfo.InvariantCulture);
		}

		internal static string ToSafeString(this DateTime dateTime)
		{
			return ToString(dateTime);
		}

		internal static string ToSafeString(this DateTime dateTime, string? format)
		{
			return ToString(dateTime, format);
		}

		public static string ToSafeString(this DateTime dateTime, string? format, CultureInfo culture)
		{
			return ToString(dateTime, format, culture);
		}
	}
#endif
}

