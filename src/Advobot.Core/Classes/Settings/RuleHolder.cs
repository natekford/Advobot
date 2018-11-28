using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds rules on a guild.
	/// </summary>
	public sealed class RuleHolder : IGuildFormattable
	{
		/// <summary>
		/// Holds the categories for rules which in turn hold the rules.
		/// </summary>
		[JsonProperty]
		public Dictionary<string, List<string>> Categories = new Dictionary<string, List<string>>();

		/// <summary>
		/// Sends the rules to the specified channel.
		/// </summary>
		/// <param name="formatter"></param>
		/// <param name="category"></param>
		/// <returns></returns>
		public IEnumerable<string> GetParts(RuleFormatter formatter, string category = null)
		{
			if (category != null)
			{
				return SplitFormattedCategoryIntoValidParts(ToString(formatter, category));
			}

			var formattedCategories = Categories.Select(x => ToString(formatter, x.Key)).ToList();
			var formattedRules = string.Join("\n", formattedCategories);
			//If all of the rules can be sent in one message, do that.
			if (!string.IsNullOrWhiteSpace(formattedRules) && formattedRules.Length <= 2000)
			{
				return new[] { formattedRules };
			}

			//If not, go by category
			var parts = new List<string>();
			foreach (var formattedCategory in formattedCategories)
			{
				parts.AddRange(SplitFormattedCategoryIntoValidParts(formattedCategory));
			}
			return parts;
		}
		private IEnumerable<string> SplitFormattedCategoryIntoValidParts(string formattedCategory)
		{
			//Null category gets ignored
			if (string.IsNullOrWhiteSpace(formattedCategory))
			{
				yield break;
			}
			//Short enough categories just get sent on their own
			if (formattedCategory.Length <= 2000)
			{
				yield return formattedCategory;
				yield break;
			}

			var sb = new StringBuilder();
			foreach (var part in formattedCategory.Split('\n'))
			{
				//If the current stored text + the new part is too big, send the current stored text
				//Then start building new stored text to send
				if (sb.Length + part.Length >= 2000)
				{
					yield return sb.ToString();
					sb.Clear();
				}
				sb.Append(part);
			}
			//Send the last remaining text
			if (sb.Length > 0)
			{
				yield return sb.ToString();
			}
		}
		/// <inheritdoc />
		public override string ToString()
			=> ToString(new RuleFormatter());
		/// <inheritdoc />
		public string Format(SocketGuild guild = null)
			=> ToString();
		/// <summary>
		/// Uses the specified rule formatter to format every rule category.
		/// </summary>
		/// <param name="formatter"></param>
		/// <returns></returns>
		public string ToString(RuleFormatter formatter)
		{
			var sb = new StringBuilder();
			var index = 0;
			foreach (var kvp in Categories)
			{
				sb.AppendLineFeed(formatter.FormatName(kvp.Key));
				for (var r = 0; r < kvp.Value.Count; ++r)
				{
					sb.AppendLineFeed(formatter.FormatRule(kvp.Value[r], r, kvp.Value.Count));
				}
				++index;
			}
			return sb.ToString();
		}
		/// <summary>
		/// Uses the specified rule formatter to format the specified category.
		/// </summary>
		/// <param name="formatter"></param>
		/// <param name="category"></param>
		/// <returns></returns>
		public string ToString(RuleFormatter formatter, string category)
		{
			var c = Categories[category];
			var sb = new StringBuilder();
			sb.AppendLineFeed(formatter.FormatName(category));
			for (var r = 0; r < c.Count; ++r)
			{
				sb.AppendLineFeed(formatter.FormatRule(c[r], r, c.Count));
			}
			return sb.ToString();
		}
	}
}
