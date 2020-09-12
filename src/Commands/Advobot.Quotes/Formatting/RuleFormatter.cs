using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using Advobot.Quotes.ReadOnlyModels;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Quotes.Formatting
{
	public static class RuleFormatterUtils
	{
		public static StringBuilder AddCharAfterNumbers(this StringBuilder sb, char charToAdd)
		{
			for (var i = 0; i < sb.Length; ++i)
			{
				// Not a number, no need to check
				if (!char.IsNumber(sb[i]))
				{
					continue;
				}

				// End of the string
				if (i == sb.Length - 1)
				{
					sb.Append(charToAdd);
					break;
				}

				var c = sb[i + 1];
				if (!char.IsNumber(c) && c != charToAdd)
				{
					sb.Insert(i, charToAdd);
				}
			}
			return sb;
		}

		public static StringBuilder AddMarkDown(this StringBuilder sb, IEnumerable<MarkDownFormat> markdown)
		{
			foreach (var md in markdown)
			{
				var f = md switch
				{
					MarkDownFormat.Bold => "**",
					MarkDownFormat.Italics => "*",
					MarkDownFormat.Code => "`",
					MarkDownFormat.StrikeThrough => "~~",
					_ => "",
				};
				sb.Insert(0, f).Append(f);
			}
			return sb;
		}

		public static int DigitCount(this int n)
		{
			if (n >= 0)
			{
#pragma warning disable IDE0011 // Add braces
				if (n < 10) return 1;
				if (n < 100) return 2;
				if (n < 1000) return 3;
				if (n < 10000) return 4;
				if (n < 100000) return 5;
				if (n < 1000000) return 6;
				if (n < 10000000) return 7;
				if (n < 100000000) return 8;
				if (n < 1000000000) return 9;
				return 10;
			}
			else
			{
				if (n > -10) return 2;
				if (n > -100) return 3;
				if (n > -1000) return 4;
				if (n > -10000) return 5;
				if (n > -100000) return 6;
				if (n > -1000000) return 7;
				if (n > -10000000) return 8;
				if (n > -100000000) return 9;
				if (n > -1000000000) return 10;
#pragma warning restore IDE0011 // Add braces
				return 11;
			}
		}

		public static StringBuilder TrimEnd(this StringBuilder sb)
		{
			if (sb.Length == 0)
			{
				return sb;
			}

			var i = sb.Length - 1;
			for (; i >= 0; i--)
			{
				if (!char.IsWhiteSpace(sb[i]))
				{
					break;
				}
			}

			if (i < sb.Length - 1)
			{
				sb.Length = i + 1;
			}

			return sb;
		}
	}

	/// <summary>
	/// Formats rules to look nice.
	/// </summary>
	[NamedArgumentType]
	public sealed class RuleFormatter
	{
		private static readonly ImmutableDictionary<RuleFormat, ISet<MarkDownFormat>> _DefaultRuleFormats =
			new Dictionary<RuleFormat, ISet<MarkDownFormat>>
			{
				{ default, new HashSet<MarkDownFormat>() },
				{ RuleFormat.Numbers, new HashSet<MarkDownFormat>() },
				{ RuleFormat.Dashes, new HashSet<MarkDownFormat>() },
				{ RuleFormat.Bullets, new HashSet<MarkDownFormat>() },
				{ RuleFormat.Bold, new HashSet<MarkDownFormat>() { MarkDownFormat.Bold } }
			}.ToImmutableDictionary();
		private static readonly ImmutableDictionary<RuleFormat, ISet<MarkDownFormat>> _DefaultTitleFormats =
			new Dictionary<RuleFormat, ISet<MarkDownFormat>>
			{
				{ default, new HashSet<MarkDownFormat>() { MarkDownFormat.Bold } },
				{ RuleFormat.Numbers, new HashSet<MarkDownFormat>() { MarkDownFormat.Bold } },
				{ RuleFormat.Dashes, new HashSet<MarkDownFormat>() { MarkDownFormat.Code } },
				{ RuleFormat.Bullets, new HashSet<MarkDownFormat>() { MarkDownFormat.Bold } },
				{ RuleFormat.Bold, new HashSet<MarkDownFormat>() { MarkDownFormat.Bold | MarkDownFormat.Italics } }
			}.ToImmutableDictionary();

		/// <summary>
		/// The character to put after numbers in the lists.
		/// </summary>
		public char CharAfterNumbers { get; set; } = '.';
		/// <summary>
		/// Additional formatting options.
		/// </summary>
		public ISet<RuleFormatOption> Options { get; set; } = new HashSet<RuleFormatOption>();
		/// <summary>
		/// The main format to use for rules.
		/// </summary>
		public RuleFormat RuleFormat { get; set; } = RuleFormat.Numbers;
		/// <summary>
		/// Markdown supplied for rules.
		/// </summary>
		public ISet<MarkDownFormat> RuleMarkDownFormat { get; set; } = new HashSet<MarkDownFormat>();
		/// <summary>
		/// Markdown supplied for titles.
		/// </summary>
		public ISet<MarkDownFormat> TitleMarkDownFormat { get; set; } = new HashSet<MarkDownFormat>();

		public void AppendCategory(StringBuilder sb, IReadOnlyRuleCategory category, IReadOnlyList<IReadOnlyRule> rules)
		{
			FormatName(sb, category.Name);
			for (var i = 0; i < rules.Count; ++i)
			{
				FormatRule(sb, rules[i].Value, i, rules.Count).AppendLineFeed();
			}
			sb.AppendLineFeed();
		}

		private StringBuilder FormatName(StringBuilder sb, string name)
		{
			sb.Append(name.FormatTitle()).TrimEnd();
			if (Options.Contains(RuleFormatOption.ExtraLines))
			{
				sb.AppendLineFeed();
			}
			sb.AddMarkDown(TitleMarkDownFormat.Count > 0 ? TitleMarkDownFormat : _DefaultTitleFormats[RuleFormat]);
			return sb;
		}

		private StringBuilder FormatRule(StringBuilder sb, string rule, int index, int rulesInCategory)
		{
			static StringBuilder PotentiallyPad(StringBuilder sb, ISet<RuleFormatOption> options, int index, int rulesInCategory)
			{
				sb.Append('`');

				var position = index + 1;
				if (options.Contains(RuleFormatOption.NumbersSameLength))
				{
					var curLength = position.DigitCount();
					var padLength = rulesInCategory.DigitCount();
					if (curLength != padLength)
					{
						sb.Append('0', padLength - curLength);
					}
					sb.Append(position);
				}

				return sb.Append(position).Append('`');
			}

			switch (RuleFormat)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bold:
					PotentiallyPad(sb, Options, index, rulesInCategory);
					break;

				case RuleFormat.Dashes:
					sb.Append('-');
					break;

				case RuleFormat.Bullets:
					sb.Append('•');
					break;
			}

			sb.Append(rule);
			if (CharAfterNumbers != default)
			{
				sb.AddCharAfterNumbers(CharAfterNumbers);
			}
			sb.TrimEnd();
			if (Options.Contains(RuleFormatOption.ExtraLines))
			{
				sb.AppendLineFeed();
			}
			return sb.AddMarkDown(RuleMarkDownFormat.Count > 0 ? RuleMarkDownFormat : _DefaultRuleFormats[RuleFormat]);
		}
	}
}