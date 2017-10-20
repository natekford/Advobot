using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Classes.Rules
{
	//TODO: finish rule implementation
	public class RuleFormatter
	{
		private static Dictionary<RuleFormat, MarkDownFormat> _DefaultTitleFormats = new Dictionary<RuleFormat, MarkDownFormat>
		{
			{ RuleFormat.Numbers, MarkDownFormat.Bold },
			{ RuleFormat.Dashes, MarkDownFormat.Code },
			{ RuleFormat.Bullets, MarkDownFormat.Bold },
			{ RuleFormat.Bold, MarkDownFormat.Bold | MarkDownFormat.Italics },
		};
		private static Dictionary<RuleFormat, MarkDownFormat> _DefaultRuleFormats = new Dictionary<RuleFormat, MarkDownFormat>
		{
			{ RuleFormat.Numbers, default },
			{ RuleFormat.Dashes, default },
			{ RuleFormat.Bullets, default },
			{ RuleFormat.Bold, MarkDownFormat.Bold },
		};

		private string _Rules;
		public string Rules => _Rules;
		private List<string> _Categories = new List<string>();
		public ImmutableList<string> Categories => _Categories.ToImmutableList();
			 
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
			[CustomArgument] char charAfterNumbers = '.',
			[CustomArgument(10)] params FormatOptions[] formatOptions)
		{
			_Format = format;
			_TitleFormat = titleFormat;
			_RuleFormat = ruleFormat;
			_CharAfterNumbers = charAfterNumbers;

			_ExtraSpaces = formatOptions.Contains(FormatOptions.ExtraSpaces);
			_NumbersSameLength = formatOptions.Contains(FormatOptions.NumbersSameLength);
			_ExtraLines = formatOptions.Contains(FormatOptions.ExtraLines);
		}

		public void SetRulesAndCategories(RuleHolder rules)
		{
			_Rules = rules.ToString(this);
			_Categories.AddRange(rules.Categories.Select((x, index) => x.ToString(this, index)));
		}
		public void SetCategory(RuleCategory category, int index)
		{
			_Categories.Add(category.ToString(this, index));
		}

		public string FormatName(RuleCategory category, int index)
		{
			var n = "";
			switch (_Format)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bullets:
				case RuleFormat.Bold:
				{
					n = $"{category.Name.FormatTitle()}";
					break;
				}
				case RuleFormat.Dashes:
				{
					n = $"{index + 1} - {category.Name.FormatTitle()}";
					break;
				}
				default:
				{
					n = category.Name.FormatTitle();
					break;
				}
			}

			n = _ExtraLines
				? $"{n}\n"
				: $"{n}";
			return AddFormattingOptions(_TitleFormat == default ? _DefaultTitleFormats[_Format] : _TitleFormat, n);
		}
		public string FormatRule(Rule rule, int index, int rulesInCategory)
		{
			var r = "";
			switch (_Format)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bold:
				{
					r = _NumbersSameLength
						? $"`{(index + 1).ToString().PadLeft(rulesInCategory.GetLengthOfNumber(), '0')}"
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
					r = "";
					break;
				}
			}

			r = _CharAfterNumbers != default
				? AddCharAfterNumbers(r, _CharAfterNumbers)
				: r;
			r = _ExtraLines
				? $"{r}\n"
				: $"{r}";
			r = _ExtraSpaces
				? $"{r} {rule.Text}"
				: $"{r}{rule.Text}";
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
					return $"`{text.EscapeBackTicks()}`";
				}
				default:
				{
					return text;
				}
			}
		}

		public async Task<IReadOnlyList<IUserMessage>> SendAsync(IMessageChannel channel)
		{
			var messages = new List<IUserMessage>();
			//If all of the rules can be sent in one message, do that.
			if (_Rules != null && _Rules.Length <= 2000)
			{
				messages.Add(await MessageActions.SendMessageAsync(channel, _Rules));
				return messages.AsReadOnly();
			}

			//If not, go by category
			foreach (var category in _Categories)
			{
				if (category == null)
				{
					continue;
				}
				else if (category.Length <= 2000)
				{
					messages.Add(await MessageActions.SendMessageAsync(channel, category));
					continue;
				}

				var sb = new StringBuilder();
				foreach (var part in category.Split('\n'))
				{
					if (sb.Length + part.Length <= 2000)
					{
						messages.Add(await MessageActions.SendMessageAsync(channel, sb.ToString()));
						sb.Clear();
					}
					sb.Append(part);
				}
				if (sb.Length > 0)
				{
					messages.Add(await MessageActions.SendMessageAsync(channel, sb.ToString()));
				}
			}
			return messages.AsReadOnly();
		}
	}
}
