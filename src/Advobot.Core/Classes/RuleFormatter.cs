using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Rules
{
	/// <summary>
	/// Formats rules to look nice.
	/// </summary>
	public class RuleFormatter
	{
		private static Dictionary<RuleFormat, MarkDownFormat> _DefaultTitleFormats = new Dictionary<RuleFormat, MarkDownFormat>
		{
			{ default, MarkDownFormat.Bold },
			{ RuleFormat.Numbers, MarkDownFormat.Bold },
			{ RuleFormat.Dashes, MarkDownFormat.Code },
			{ RuleFormat.Bullets, MarkDownFormat.Bold },
			{ RuleFormat.Bold, MarkDownFormat.Bold | MarkDownFormat.Italics }
		};
		private static Dictionary<RuleFormat, MarkDownFormat> _DefaultRuleFormats = new Dictionary<RuleFormat, MarkDownFormat>
		{
			{ default, default },
			{ RuleFormat.Numbers, default },
			{ RuleFormat.Dashes, default },
			{ RuleFormat.Bullets, default },
			{ RuleFormat.Bold, MarkDownFormat.Bold }
		};

		private RuleFormat _Format;
		private MarkDownFormat _TitleMarkDownFormat;
		private MarkDownFormat _RuleMarkDownFormat;
		private RuleFormatOption _Options;
		private char _CharAfterNumbers;

		public RuleFormatter() : this(default) { }
		[NamedArgumentConstructor]
		private RuleFormatter(
			[NamedArgument] RuleFormat format = default,
			[NamedArgument] MarkDownFormat titleFormat = default,
			[NamedArgument] MarkDownFormat ruleFormat = default,
			[NamedArgument] char charAfterNumbers = '.',
			[NamedArgument(10)] params RuleFormatOption[] formatOptions)
		{
			_Format = format == default ? RuleFormat.Numbers : format;
			_TitleMarkDownFormat = titleFormat;
			_RuleMarkDownFormat = ruleFormat;
			_CharAfterNumbers = charAfterNumbers;
			formatOptions.ToList().ForEach(x => _Options |= x);
		}

		public string FormatName(string name)
		{
			var n = name.FormatTitle().Trim(' ');
			if (_Options.HasFlag(RuleFormatOption.ExtraLines))
			{
				n = n + "\n";
			}
			return AddMarkDown(_TitleMarkDownFormat == default ? _DefaultTitleFormats[_Format] : _TitleMarkDownFormat, n);
		}
		public string FormatRule(string rule, int index, int rulesInCategory)
		{
			string r;
			switch (_Format)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bold:
					if (_Options.HasFlag(RuleFormatOption.NumbersSameLength))
					{
						r = $"`{(index + 1).ToString().PadLeft(rulesInCategory.ToString().Length, '0')}";
					}
					else
					{
						r = $"`{index + 1}`";
					}
					break;
				case RuleFormat.Dashes:
					r = $"-";
					break;
				case RuleFormat.Bullets:
					r = $"•";
					break;
				default:
					r = "";
					break;
			}

			r = $"{r}{rule}";
			r = _CharAfterNumbers != default
				? AddCharAfterNumbers(r, _CharAfterNumbers)
				: r;
			r = r.Trim(' ');
			if (_Options.HasFlag(RuleFormatOption.ExtraLines))
			{
				r = r + "\n";
			}
			return AddMarkDown(_RuleMarkDownFormat == default ? _DefaultRuleFormats[_Format] : _RuleMarkDownFormat, r);
		}
		private string AddCharAfterNumbers(string text, char charToAdd)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < text.Length; ++i)
			{
				var c = text[i];
				sb.Append(c);

				//If the last character in a string then add a period since it's the end
				//If the next character after is not a number add a period too
				if (Char.IsNumber(c) && (i + 1 == text.Length || !Char.IsNumber(text[i + 1])))
				{
					sb.Append(charToAdd);
				}
			}
			return sb.ToString();
		}
		private string AddMarkDown(MarkDownFormat formattingOptions, string text)
		{
			foreach (MarkDownFormat md in Enum.GetValues(typeof(MarkDownFormat)))
			{
				if ((formattingOptions & md) != 0)
				{
					switch (md)
					{
						case MarkDownFormat.Bold:
							text = $"**{text}**";
							break;
						case MarkDownFormat.Italics:
							text = $"*{text}*";
							break;
						case MarkDownFormat.Code:
							text = $"`{text.EscapeBackTicks()}`";
							break;
					}
				}
			}
			return text;
		}
	}
}
