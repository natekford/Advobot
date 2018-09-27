using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// Formats rules to look nice.
	/// </summary>
	public sealed class RuleFormatter
	{
		private static readonly ImmutableDictionary<RuleFormat, MarkDownFormat[]> _DefaultTitleFormats = new Dictionary<RuleFormat, MarkDownFormat[]>
		{
			{ default, new[] { MarkDownFormat.Bold } },
			{ RuleFormat.Numbers, new[] { MarkDownFormat.Bold } },
			{ RuleFormat.Dashes, new[] { MarkDownFormat.Code } },
			{ RuleFormat.Bullets, new[] { MarkDownFormat.Bold } },
			{ RuleFormat.Bold, new[] { MarkDownFormat.Bold | MarkDownFormat.Italics } }
		}.ToImmutableDictionary();
		private static readonly ImmutableDictionary<RuleFormat, MarkDownFormat[]> _DefaultRuleFormats = new Dictionary<RuleFormat, MarkDownFormat[]>
		{
			{ default, new MarkDownFormat[0] },
			{ RuleFormat.Numbers, new MarkDownFormat[0] },
			{ RuleFormat.Dashes, new MarkDownFormat[0] },
			{ RuleFormat.Bullets, new MarkDownFormat[0] },
			{ RuleFormat.Bold, new[] { MarkDownFormat.Bold } }
		}.ToImmutableDictionary();
		private static readonly ImmutableArray<MarkDownFormat> _MarkDownFormats = Enum.GetValues(typeof(MarkDownFormat)).Cast<MarkDownFormat>().ToImmutableArray();

		/// <summary>
		/// The character to put after numbers in the lists.
		/// </summary>
		public char CharAfterNumbers { get; set; } = '.';
		/// <summary>
		/// The main format to use for rules.
		/// </summary>
		public RuleFormat RuleFormat { get; set; } = RuleFormat.Numbers;
		/// <summary>
		/// Markdown supplied for titles.
		/// </summary>
		public IList<MarkDownFormat> TitleMarkDownFormat { get; } = new List<MarkDownFormat>();
		/// <summary>
		/// Markdown supplied for rules.
		/// </summary>
		public IList<MarkDownFormat> RuleMarkDownFormat { get; } = new List<MarkDownFormat>();
		/// <summary>
		/// Additional formatting options.
		/// </summary>
		public IList<RuleFormatOption> Options { get; } = new List<RuleFormatOption>();

		/// <summary>
		/// Format the name of a rule category.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public string FormatName(string name)
		{
			var n = name.FormatTitle().Trim(' ');
			if (Options.Contains(RuleFormatOption.ExtraLines))
			{
				n = n + "\n";
			}
			return AddMarkDown(TitleMarkDownFormat.Any() ? TitleMarkDownFormat : _DefaultTitleFormats[RuleFormat], n);
		}
		/// <summary>
		/// Format the rule itself.
		/// </summary>
		/// <param name="rule"></param>
		/// <param name="index"></param>
		/// <param name="rulesInCategory"></param>
		/// <returns></returns>
		public string FormatRule(string rule, int index, int rulesInCategory)
		{
			string r;
			switch (RuleFormat)
			{
				case RuleFormat.Numbers:
				case RuleFormat.Bold:
					r = Options.Contains(RuleFormatOption.NumbersSameLength)
						? $"`{(index + 1).ToString().PadLeft(rulesInCategory.ToString().Length, '0')}"
						: $"`{index + 1}`";
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
			r = (CharAfterNumbers != default ? AddCharAfterNumbers(r, CharAfterNumbers) : r).Trim();
			if (Options.Contains(RuleFormatOption.ExtraLines))
			{
				r = r + "\n";
			}
			return AddMarkDown(RuleMarkDownFormat.Any() ? RuleMarkDownFormat : _DefaultRuleFormats[RuleFormat], r);
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
				if (char.IsNumber(c) && (i + 1 == text.Length || !char.IsNumber(text[i + 1])))
				{
					sb.Append(charToAdd);
				}
			}
			return sb.ToString();
		}
		private string AddMarkDown(IEnumerable<MarkDownFormat> markdown, string text)
		{
			foreach (var md in markdown)
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
					case MarkDownFormat.StrikeThrough:
						text = $"~~{text.EscapeBackTicks()}~~";
						break;
				}
			}
			return text;
		}
	}
}
