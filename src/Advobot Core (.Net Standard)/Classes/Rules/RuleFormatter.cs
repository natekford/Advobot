using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
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

		public StringBuilder RuleBuilder;
		public List<StringBuilder> CategoryBuilders;
			 
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
			RuleBuilder = new StringBuilder();
			CategoryBuilders = new List<StringBuilder>();

			_Format = format;
			_TitleFormat = titleFormat;
			_RuleFormat = ruleFormat;
			_CharAfterNumbers = charAfterNumbers;

			_ExtraSpaces = formatOptions.Contains(FormatOptions.ExtraSpaces);
			_NumbersSameLength = formatOptions.Contains(FormatOptions.NumbersSameLength);
			_ExtraLines = formatOptions.Contains(FormatOptions.ExtraLines);
		}

		public StringBuilder FormatRules(RuleHolder rules)
		{
			var categories = rules.Categories;
			for (int c = 0; c < categories.Count; ++c)
			{
				CategoryBuilders.Add(FormatRuleCategory(c, categories[c]));
			}
			foreach (var category in CategoryBuilders)
			{
				RuleBuilder.AppendLineFeed(category.ToString());
			}
			return RuleBuilder;
		}
		public StringBuilder FormatRuleCategory(int index, RuleCategory category)
		{
			var sb = new StringBuilder();
			sb.AppendLineFeed(FormatName(index, category.Name));

			var rules = category.Rules;
			for (int r = 0; r < rules.Count; ++r)
			{
				if (_ExtraLines)
				{
					sb.AppendLineFeed();
				}
				sb.AppendLineFeed(FormatRule(r, rules[r], rules.Count));
			}
			return sb;
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
					n = name;
					break;
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
						? $"`{(index + 1).ToString().PadLeft(totalRuleCountInCategory.GetLengthOfNumber(), '0')}"
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
					r = rule;
					break;
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
			if (RuleBuilder.Length <= 2000)
			{
				messages.Add(await MessageActions.SendMessageAsync(channel, RuleBuilder.ToString()));
				return messages.AsReadOnly();
			}

			foreach (var category in CategoryBuilders)
			{
				if (category.Length <= 2000)
				{
					messages.Add(await MessageActions.SendMessageAsync(channel, category.ToString()));
					continue;
				}

				var sb = new StringBuilder();
				foreach (var part in category.ToString().Split('\n'))
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
