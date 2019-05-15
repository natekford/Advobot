using System;
using System.Collections;
using System.Linq;
using System.Text;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Commands.Responses
{
	public static class RuntimeFormatUtils
	{
		public static RuntimeFormattedObject NoFormatting(this object value)
			=> RuntimeFormattedObject.None(value);
		public static RuntimeFormattedObject WithFormat(this object value, string format)
			=> RuntimeFormattedObject.Create(value, format);
	}

	public struct RuntimeFormattedObject
	{
		public object Value { get; }
		public string Format { get; }

		private RuntimeFormattedObject(object value, string? format)
		{
			Value = value;
			Format = format ?? "NONE";
		}

		public override string ToString()
			=> Value.ToString();

		public static RuntimeFormattedObject None(object value)
			=> new RuntimeFormattedObject(value, null);
		public static RuntimeFormattedObject Create(object value, string format)
			=> new RuntimeFormattedObject(value, format);
	}

	//TODO: make into service that accepts client meaning whenever it encounters a ulong it can parse it?
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
			if (arg is RuntimeFormattedObject rtf)
			{
				if (format != null)
				{
					throw new InvalidOperationException($"{nameof(format)} should not be supplied if {nameof(RuntimeFormattedObject)} is being used.");
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
