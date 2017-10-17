using Advobot.Actions.Formatting;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Classes.Rules
{
	public class RuleFormatter
	{
		private Dictionary<RuleFormat, MarkDownFormat> _DefaultTitleFormats = new Dictionary<RuleFormat, MarkDownFormat>
		{
			{ RuleFormat.Numbers, MarkDownFormat.Bold },
			{ RuleFormat.Dashes, MarkDownFormat.Code },
			{ RuleFormat.Bullets, MarkDownFormat.Bold },
			{ RuleFormat.Bold, MarkDownFormat.Bold | MarkDownFormat.Italics },
		};
		private Dictionary<RuleFormat, MarkDownFormat> _DefaultRuleFormats = new Dictionary<RuleFormat, MarkDownFormat>
		{
			{ RuleFormat.Numbers, default },
			{ RuleFormat.Dashes, default },
			{ RuleFormat.Bullets, default },
			{ RuleFormat.Bold, MarkDownFormat.Bold },
		};

		private RuleFormat _Format;
		private MarkDownFormat _TitleFormat;
		private MarkDownFormat _RuleFormat;
		private bool _ExtraSpaces;
		private bool _NumbersSameLength;
		private bool _ExtraLines;
		private char _CharAfterNumbers;

		[CustomArgumentConstructor]
		public RuleFormatter(
			[CustomArgument] RuleFormat format = default,
			[CustomArgument] MarkDownFormat titleFormat = default,
			[CustomArgument] MarkDownFormat ruleFormat = default,
			[CustomArgument(10)] params AdditionalFormatOptions[] moreOptions)
		{
			_Format = format;
			_ExtraSpaces = moreOptions.Contains(AdditionalFormatOptions.ExtraSpaces);
			_NumbersSameLength = moreOptions.Contains(AdditionalFormatOptions.NumbersSameLength);
			_ExtraLines = moreOptions.Contains(AdditionalFormatOptions.ExtraLines);

			_CharAfterNumbers = moreOptions.Contains(AdditionalFormatOptions.PeriodsAfterNumbers)
				? '.'
				: _CharAfterNumbers;
			_CharAfterNumbers = moreOptions.Contains(AdditionalFormatOptions.ParanthesesAfterNumbers)
				? ')'
				: _CharAfterNumbers;
		}

		public string FormatRules(Rules rules)
		{
			var sb = new StringBuilder();
			var categories = rules.Categories;
			for (int c = 0; c < categories.Count; ++c)
			{
				sb.AppendLineFeed(FormatName(c, categories[c].Name));
				if (_ExtraLines)
				{
					sb.AppendLineFeed();
				}
				sb.AppendLineFeed(FormatRuleCategory(categories[c]));
			}
			return sb.ToString();
		}
		public string FormatRuleCategory(RuleCategory category)
		{
			var sb = new StringBuilder();
			var rules = category.Rules;
			for (int r = 0; r < rules.Count; ++r)
			{
				sb.AppendLineFeed(FormatRule(r, rules[r], rules.Count));
				if (_ExtraLines)
				{
					sb.AppendLineFeed();
				}
			}
			return sb.ToString();
		}

		private string FormatName(int index, string name)
		{
			var n = "";
			switch (_Format)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bullets:
				case RuleFormat.Bold:
				{
					n = $"{name.FormatTitle()}";
					break;
				}
				case RuleFormat.Dashes:
				{
					n = $"{index + 1} - {name.FormatTitle()}";
					break;
				}
				default:
				{
					return name;
				}
			}

			return AddFormattingOptions(_TitleFormat == default ? _DefaultTitleFormats[_Format] : _TitleFormat, n);
		}
		private string FormatRule(int index, string rule, int totalRuleCountInCategory)
		{
			var r = "";
			switch (_Format)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bold:
				{
					r = _NumbersSameLength
						? $"`{(index + 1).ToString().PadLeft(totalRuleCountInCategory.ToString().Length, '0')}"
						: $"`{index + 1}`";
					break;
				}
				case RuleFormat.Dashes:
				{
					r = $"-";
					break;
				}
				case RuleFormat.Bullets:
				{
					r = $"•";
					break;
				}
				default:
				{
					return rule;
				}
			}

			r = _CharAfterNumbers != default
				? AddCharAfterNumbers(r, _CharAfterNumbers)
				: r;
			r = _ExtraSpaces
				? $"{r} {rule}"
				: $"{r}{rule}";
			return AddFormattingOptions(_RuleFormat == default ? _DefaultRuleFormats[_Format] : _TitleFormat, r);
		}

		private string AddCharAfterNumbers(string text, char charToAdd)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < text.Length; ++i)
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
		private string AddFormattingOptions(MarkDownFormat formattingOptions, string text)
		{
			var t = text.EscapeAllMarkdown();
			foreach (MarkDownFormat md in Enum.GetValues(typeof(MarkDownFormat)))
			{
				if ((formattingOptions & md) != 0)
				{
					t = AddMarkDown(md, t);
				}
			}
			return t;
		}
		private string AddMarkDown(MarkDownFormat md, string text)
		{
			switch (md)
			{
				case MarkDownFormat.Bold:
				{
					return $"**{text}**";
				}
				case MarkDownFormat.Italics:
				{
					return $"*{text}*";
				}
				case MarkDownFormat.Code:
				{
					return $"`{text}`";
				}
				default:
				{
					return text;
				}
			}
		}
	}
}
