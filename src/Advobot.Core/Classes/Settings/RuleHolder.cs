using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdvorangesUtils;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// Holds rules on a guild.
	/// </summary>
	public sealed class RuleHolder
	{
		/// <summary>
		/// Holds the categories for rules which in turn hold the rules.
		/// </summary>
		[JsonProperty]
		public Dictionary<string, List<string>> Categories { get; set; } = new Dictionary<string, List<string>>();

		/// <summary>
		/// Sends the rules to the specified channel.
		/// </summary>
		/// <param name="formatter"></param>
		/// <param name="category"></param>
		/// <returns></returns>
		public IReadOnlyCollection<string> GetParts(RuleFormatter formatter, string? category = null)
		{
			if (category != null)
			{
				return SplitFormattedCategoryIntoValidParts(ToString(formatter, category)).ToArray();
			}

			var categories = Categories.Select(x => ToString(formatter, x.Key));
			var rules = categories.Join("\n");
			//If all of the rules can be sent in one message, do that.
			if (rules?.Length <= 2000)
			{
				return new[] { rules };
			}

			//If not, go by category
			return categories.SelectMany(x => SplitFormattedCategoryIntoValidParts(x)).ToArray();
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
		/// <summary>
		/// Uses the specified rule formatter to format every rule category.
		/// </summary>
		/// <param name="formatter"></param>
		/// <returns></returns>
		public string ToString(RuleFormatter formatter)
		{
			var sb = new StringBuilder();
			var categoryIndex = 0;
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
			foreach (var (Category, Rules) in Categories)
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
			{
				AppendCategory(formatter, sb, Rules, Category);
				++categoryIndex;
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
			var sb = new StringBuilder();
			AppendCategory(formatter, sb, Categories[category], category);
			return sb.ToString();
		}
		private void AppendCategory(RuleFormatter formatter, StringBuilder sb, IList<string> rules, string name)
		{
			sb.AppendLineFeed(formatter.FormatName(name));
			for (var i = 0; i < rules.Count; ++i)
			{
				sb.AppendLineFeed(formatter.FormatRule(rules[i], i, rules.Count));
			}
		}
	}
}
