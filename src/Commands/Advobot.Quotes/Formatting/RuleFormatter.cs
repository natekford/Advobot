using Advobot.Quotes.Models;
using Advobot.Utilities;

using Discord.Commands;

using System.Text;

namespace Advobot.Quotes.Formatting;

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
			if (n < 10)
				return 1;
			if (n < 100)
				return 2;
			if (n < 1000)
				return 3;
			if (n < 10000)
				return 4;
			if (n < 100000)
				return 5;
			if (n < 1000000)
				return 6;
			if (n < 10000000)
				return 7;
			if (n < 100000000)
				return 8;
			if (n < 1000000000)
				return 9;
			return 10;
		}
		else
		{
			if (n > -10)
				return 2;
			if (n > -100)
				return 3;
			if (n > -1000)
				return 4;
			if (n > -10000)
				return 5;
			if (n > -100000)
				return 6;
			if (n > -1000000)
				return 7;
			if (n > -10000000)
				return 8;
			if (n > -100000000)
				return 9;
			if (n > -1000000000)
				return 10;
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
	private static readonly Dictionary<RuleFormat, HashSet<MarkDownFormat>> _DefaultRuleFormats
		= new()
		{
			{ default, [] },
			{ RuleFormat.Numbers, [] },
			{ RuleFormat.Dashes, [] },
			{ RuleFormat.Bullets, [] },
			{ RuleFormat.Bold, [MarkDownFormat.Bold] }
		};
	private static readonly Dictionary<RuleFormat, HashSet<MarkDownFormat>> _DefaultTitleFormats
		= new()
		{
			{ default, [MarkDownFormat.Bold] },
			{ RuleFormat.Numbers, [MarkDownFormat.Bold] },
			{ RuleFormat.Dashes, [MarkDownFormat.Code] },
			{ RuleFormat.Bullets, [MarkDownFormat.Bold] },
			{ RuleFormat.Bold, [MarkDownFormat.Bold | MarkDownFormat.Italics] }
		};

	private HashSet<RuleFormatOption>? _Options;

	/// <summary>
	/// The character to put after numbers in the lists.
	/// </summary>
	public char CharAfterNumbers { get; set; } = '.';
	/// <summary>
	/// Additional formatting options.
	/// </summary>
	public IEnumerable<RuleFormatOption>? Options
	{
		get => _Options;
		set => _Options = value is null ? null : [.. value];
	}
	/// <summary>
	/// The main format to use for rules.
	/// </summary>
	public RuleFormat RuleFormat { get; set; } = RuleFormat.Numbers;
	/// <summary>
	/// Markdown supplied for rules.
	/// </summary>
	public IEnumerable<MarkDownFormat>? RuleMarkDownFormat { get; set; }
	/// <summary>
	/// Markdown supplied for titles.
	/// </summary>
	public IEnumerable<MarkDownFormat>? TitleMarkDownFormat { get; set; }

	public string Format(RuleCategory category, IReadOnlyList<Rule> rules)
	{
		var sb = new StringBuilder();
		AppendCategory(sb, category, rules);
		return sb.ToString();
	}

	public string Format(IReadOnlyDictionary<RuleCategory, IReadOnlyList<Rule>> rules)
	{
		var sb = new StringBuilder();
		foreach (var kvp in rules)
		{
			AppendCategory(sb, kvp.Key, kvp.Value);
		}
		return sb.ToString();
	}

	private void AppendCategory(StringBuilder sb, RuleCategory category, IReadOnlyList<Rule> rules)
	{
		FormatName(sb, category.Value);
		for (var i = 0; i < rules.Count; ++i)
		{
			FormatRule(sb, rules[i].Value, i, rules.Count).AppendLine();
		}
		sb.AppendLine();
	}

	private StringBuilder FormatName(StringBuilder sb, string name)
	{
		sb.Append(name.ToTitleCase()).TrimEnd();
		if (_Options?.Contains(RuleFormatOption.ExtraLines) == true)
		{
			sb.AppendLine();
		}
		sb.AddMarkDown(TitleMarkDownFormat ?? _DefaultTitleFormats[RuleFormat]);
		return sb;
	}

	private StringBuilder FormatRule(
		StringBuilder sb,
		string rule,
		int index,
		int count)
	{
		static StringBuilder PotentiallyPad(StringBuilder sb, int index, int count, bool sameLength)
		{
			sb.Append('`');

			var position = index + 1;
			if (sameLength)
			{
				var curLength = position.DigitCount();
				var padLength = count.DigitCount();
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
				var sameLength = _Options?.Contains(RuleFormatOption.NumbersSameLength) == true;
				PotentiallyPad(sb, index, count, sameLength);
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
		if (_Options?.Contains(RuleFormatOption.ExtraLines) == true)
		{
			sb.AppendLine();
		}
		return sb.AddMarkDown(RuleMarkDownFormat ?? _DefaultRuleFormats[RuleFormat]);
	}
}