using System;
using System.Collections;
using System.Linq;
using System.Text;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;

namespace Advobot.Commands.Responses
{
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
			var overrideFormat = format.StartsWith("F=");
			var options = overrideFormat ? format.Substring(2).Split('|') : Enumerable.Empty<string>();

			//TODO: make this ignore the default format when supplied with override
			if (options.Contains("title") || UseTitleCase) { arg = arg.FormatTitle(); }
			if (options.Contains(":")     || UseColon) { arg += ":"; }
			if (options.Contains("`")     || UseCode) { arg = $"`{arg}`"; }
			if (options.Contains("**")    || UseBold) { arg = $"**{arg}**"; }
			if (options.Contains("_")     || UseItalics) { arg = $"_{arg}_"; }
			if (options.Contains("__")    || UseUnderline) { arg = $"__{arg}__"; }
			if (options.Contains("~~")    || UseStrikethrough) { arg = $"~~{arg}~~"; }
			if (options.Contains("```")   || UseBigCode) { arg = $"```\n{arg}\n```"; }
			return arg;
		}
	}
}
