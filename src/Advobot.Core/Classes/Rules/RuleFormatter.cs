using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
			{ RuleFormat.Bold, MarkDownFormat.Bold | MarkDownFormat.Italics },
		};
		private static Dictionary<RuleFormat, MarkDownFormat> _DefaultRuleFormats = new Dictionary<RuleFormat, MarkDownFormat>
		{
			{ default, default },
			{ RuleFormat.Numbers, default },
			{ RuleFormat.Dashes, default },
			{ RuleFormat.Bullets, default },
			{ RuleFormat.Bold, MarkDownFormat.Bold },
		};
			 
		public RuleFormat Format;
		public MarkDownFormat TitleMarkDownFormat;
		public MarkDownFormat RuleMarkDownFormat;
		public RuleFormatOption Options;
		public char CharAfterNumbers;

		[CustomArgumentConstructor]
		public RuleFormatter(
			[CustomArgument] RuleFormat format = default,
			[CustomArgument] MarkDownFormat titleFormat = default,
			[CustomArgument] MarkDownFormat ruleFormat = default,
			[CustomArgument] char charAfterNumbers = '.',
			[CustomArgument(10)] params RuleFormatOption[] formatOptions)
		{
			Format = format == default ? RuleFormat.Numbers : format;
			TitleMarkDownFormat = titleFormat;
			RuleMarkDownFormat = ruleFormat;
			CharAfterNumbers = charAfterNumbers;
			formatOptions.ToList().ForEach(x => Options |= x);
		}

		public string FormatName(string name, int index)
		{
			var n = "";
			switch (Format)
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
					n = name.FormatTitle();
					break;
				}
			}

			n = n.Trim(' ');
			if (Options.HasFlag(RuleFormatOption.ExtraLines))
			{
				n = n + "\n";
			}
			return AddMarkDown(TitleMarkDownFormat == default ? _DefaultTitleFormats[Format] : TitleMarkDownFormat, n);
		}
		public string FormatRule(string rule, int index, int rulesInCategory)
		{
			var r = "";
			switch (Format)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bold:
				{
					if (Options.HasFlag(RuleFormatOption.NumbersSameLength))
					{
						r = $"`{(index + 1).ToString().PadLeft(rulesInCategory.GetLength(), '0')}";
					}
					else
					{
						r = $"`{index + 1}`";
					}
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

			r = $"{r}{rule}";
			r = CharAfterNumbers != default
				? AddCharAfterNumbers(r, CharAfterNumbers)
				: r;
			r = r.Trim(' ');
			if (Options.HasFlag(RuleFormatOption.ExtraLines))
			{
				r = r + "\n";
			}
			return AddMarkDown(RuleMarkDownFormat == default ? _DefaultRuleFormats[Format] : RuleMarkDownFormat, r);
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
		private string AddMarkDown(MarkDownFormat formattingOptions, string text)
		{
			foreach (MarkDownFormat md in Enum.GetValues(typeof(MarkDownFormat)))
			{
				if ((formattingOptions & md) != 0)
				{
					switch (md)
					{
						case MarkDownFormat.Bold:
						{
							text = $"**{text}**";
							break;
						}
						case MarkDownFormat.Italics:
						{
							text = $"*{text}*";
							break;
						}
						case MarkDownFormat.Code:
						{
							text = $"`{text.EscapeBackTicks()}`";
							break;
						}
					}
				}
			}
			return text;
		}

		/// <summary>
		/// Sends the rules to the specified channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task<IReadOnlyList<IUserMessage>> SendRulesAsync(IEnumerable<RuleCategory> categories, IMessageChannel channel)
		{
			var messages = new List<IUserMessage>();

			var formattedCategories = categories.Select((c, i) => c.ToString(this, i)).ToImmutableList();
			var formattedRules = String.Join("\n", formattedCategories);
			//If all of the rules can be sent in one message, do that.
			if (!String.IsNullOrWhiteSpace(formattedRules) && formattedRules.Length <= 2000)
			{
				messages.Add(await MessageUtils.SendMessageAsync(channel, formattedRules).CAF());
				return messages.AsReadOnly();
			}

			//If not, go by category
			foreach (var category in formattedCategories)
			{
				messages.AddRange(await SendCategoryAsync(category, channel));
			}
			return messages.AsReadOnly();
		}
		/// <summary>
		/// Sends a category to the specified channel.
		/// </summary>
		/// <param name="formattedCategory"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		public async Task<IReadOnlyCollection<IUserMessage>> SendCategoryAsync(string formattedCategory, IMessageChannel channel)
		{
			var messages = new List<IUserMessage>();
			//Null category gets ignored
			if (String.IsNullOrWhiteSpace(formattedCategory))
			{
				return messages;
			}
			//Short enough categories just get sent on their own
			else if (formattedCategory.Length <= 2000)
			{
				messages.Add(await MessageUtils.SendMessageAsync(channel, formattedCategory).CAF());
				return messages;
			}

			var sb = new StringBuilder();
			foreach (var part in formattedCategory.Split('\n'))
			{
				//If the current stored text + the new part is too big, send the current stored text
				//Then start building new stored text to send
				if (sb.Length + part.Length >= 2000)
				{
					messages.Add(await MessageUtils.SendMessageAsync(channel, sb.ToString()).CAF());
					sb.Clear();
				}
				sb.Append(part);
			}
			//Send the last remaining text
			if (sb.Length > 0)
			{
				messages.Add(await MessageUtils.SendMessageAsync(channel, sb.ToString()).CAF());
			}
			return messages;
		}
	}
}
