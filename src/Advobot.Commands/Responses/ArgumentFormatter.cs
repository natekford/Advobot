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
			if (format.Contains('t') || UseTitleCase) { arg = arg.FormatTitle(); }
			if (format.Contains('c') || UseColon) { arg += ":"; }
			if (format.Contains('c') || UseCode) { arg = $"`{arg}`"; }
			if (format.Contains('b') || UseBold) { arg = $"**{arg}**"; }
			if (format.Contains('i') || UseItalics) { arg = $"_{arg}_"; }
			if (format.Contains('u') || UseUnderline) { arg = $"__{arg}__"; }
			if (format.Contains('s') || UseStrikethrough) { arg = $"~~{arg}~~"; }
			if (format.Contains('B') || UseBigCode) { arg = $"```\n{arg}\n```"; }
			return arg;
		}
	}

	/*
	public partial class ResponsesOf
	{
		public sealed class ReadOnlyAdvobotSettingsModuleBase : CommandResponses
		{
			public static ReadOnlyAdvobotSettingsModuleBase For(AdvobotCommandContext context)
				=> new ReadOnlyAdvobotSettingsModuleBase { Context = context, };
			public AdvobotResult Names(string settingName, IEnumerable<string> settings)
			{
				return Success().WithEmbed(new EmbedWrapper
				{
					Title = settingName,
					Description = settings,
				});
			}
		}
	}*/
}
