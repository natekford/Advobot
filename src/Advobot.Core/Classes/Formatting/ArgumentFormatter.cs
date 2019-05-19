using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Classes.Formatting
{
	//TODO: make into service that accepts client meaning whenever it encounters a ulong it can parse it?
	/// <summary>
	/// Formats arguments with markdown.
	/// </summary>
	public class ArgumentFormatter : IFormatProvider, ICustomFormatter
	{
		/// <summary>
		/// What to join <see cref="IEnumerable{T}"/> with.
		/// </summary>
		public string Joiner { get; set; } = ", ";
		/// <summary>
		/// Whether to use title case by defaul.t
		/// </summary>
		public bool UseTitleCase { get; set; }
		/// <summary>
		/// Whether to use colons by default.
		/// </summary>
		public bool UseColon { get; set; }
		/// <summary>
		/// Whether to put into markdown code by default.
		/// </summary>
		public bool UseCode { get; set; }
		/// <summary>
		/// Whether to put into markdown big code by default.
		/// </summary>
		public bool UseBigCode { get; set; }
		/// <summary>
		/// Whether to put into markdown bold by default.
		/// </summary>
		public bool UseBold { get; set; }
		/// <summary>
		/// Whether to put into markdown italics by default.
		/// </summary>
		public bool UseItalics { get; set; }
		/// <summary>
		/// Whether to put into markdown underline by default.
		/// </summary>
		public bool UseUnderline { get; set; }
		/// <summary>
		/// Whether to put into markdown strikethrough by default.
		/// </summary>
		public bool UseStrikethrough { get; set; }

		/// <inheritdoc />
		public object? GetFormat(Type formatType)
			=> formatType == typeof(ICustomFormatter) ? this : null;
		/// <inheritdoc />
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
			if (options.Contains(":") || (!ignoreDefaults && UseColon)) { arg += ":"; }
			if (options.Contains("`") || (!ignoreDefaults && UseCode)) { arg = $"`{arg}`"; }
			if (options.Contains("**") || (!ignoreDefaults && UseBold)) { arg = $"**{arg}**"; }
			if (options.Contains("_") || (!ignoreDefaults && UseItalics)) { arg = $"_{arg}_"; }
			if (options.Contains("__") || (!ignoreDefaults && UseUnderline)) { arg = $"__{arg}__"; }
			if (options.Contains("~~") || (!ignoreDefaults && UseStrikethrough)) { arg = $"~~{arg}~~"; }
			if (options.Contains("```") || (!ignoreDefaults && UseBigCode)) { arg = $"```\n{arg}\n```"; }
			return arg;
		}
	}
}
