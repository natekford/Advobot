using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Commands.Responses
{
	public static class RuntimeFormatUtils
	{
		public static RuntimeFormat None(this object value)
			=> RuntimeFormat.None(value);
		public static RuntimeFormat Create(this object value, string format)
			=> RuntimeFormat.Create(value, format);
	}

	public sealed class RuntimeFormat
	{
		public object Value { get; }
		public string Format { get; }

		private RuntimeFormat(object value, string? format)
		{
			Value = value;
			Format = format ?? "NONE";
		}

		public static RuntimeFormat None(object value)
			=> new RuntimeFormat(value, null);
		public static RuntimeFormat Create(object value, string format)
			=> new RuntimeFormat(value, format);
	}

	public class ArgumentFormatter : IFormatProvider, ICustomFormatter
	{
		public string Joiner { get; set; } = ", ";
		public bool UseTitleCase { get; set; }
		public bool UseColon { get; set; }
		public bool UseCode { get; set; }
		public bool UseBigCode { get; set; }
		public bool UseBold { get; set; }
		public bool UseItalics { get; set; }
		public bool UseUnderline { get; set; }
		public bool UseStrikethrough { get; set; }

		public object? GetFormat(Type formatType)
			=> formatType == typeof(ICustomFormatter) ? this : null;
		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (formatProvider != this)
			{
				throw new ArgumentException(nameof(formatProvider));
			}
			return Format(format, arg);
		}
		private string Format(string format, object arg)
		{
			if (arg is null)
			{
				return Format(format, "Nothing");
			}
			if (arg is RuntimeFormat rtf)
			{
				if (format != null)
				{
					throw new InvalidOperationException($"{nameof(format)} should not be supplied if {nameof(RuntimeFormat)} is being used.");
				}
				return Format(rtf.Format ?? "", rtf.Value);
			}
			if (arg is string str)
			{
				return Format(format, str);
			}
			if (arg is IEnumerable enumerable)
			{
				var sb = new StringBuilder();
				foreach (var item in enumerable)
				{
					if (sb.Length > 0)
					{
						sb.Append(Joiner);
					}
					sb.Append(Format(format, item));
				}
				return sb.Length > 0 ? sb.ToString() : Format(format, "None");
			}
			if (arg is ISnowflakeEntity snowflake)
			{
				return Format(format, snowflake.Format());
			}
			return Format(format, arg.ToString());
		}
		private string Format(string format, string arg)
		{
			var options = (format ?? "").Split('|').Select(x => x.Trim());
			var ignoreDefaults = options.Any();

			if (options.Contains("title") || (!ignoreDefaults && UseTitleCase)) { arg = arg.FormatTitle(); }
			if (options.Contains(":")     || (!ignoreDefaults && UseColon)) { arg += ":"; }
			if (options.Contains("`")     || (!ignoreDefaults && UseCode)) { arg = $"`{arg}`"; }
			if (options.Contains("**")    || (!ignoreDefaults && UseBold)) { arg = $"**{arg}**"; }
			if (options.Contains("_")     || (!ignoreDefaults && UseItalics)) { arg = $"_{arg}_"; }
			if (options.Contains("__")    || (!ignoreDefaults && UseUnderline)) { arg = $"__{arg}__"; }
			if (options.Contains("~~")    || (!ignoreDefaults && UseStrikethrough)) { arg = $"~~{arg}~~"; }
			if (options.Contains("```")   || (!ignoreDefaults && UseBigCode)) { arg = $"```\n{arg}\n```"; }
			return arg;
		}
	}
}
