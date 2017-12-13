using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Core.Classes.Rules
{
	/// <summary>
	/// Holds a bunch of strings representing rules.
	/// </summary>
	public class RuleCategory : ISetting
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty("Rules")]
		private List<string> _Rules = new List<string>();
		[JsonIgnore]
		public IReadOnlyList<string> Rules => _Rules.AsReadOnly();

		public RuleCategory(string name)
		{
			Name = name;
		}

		public void AddRule(string rule) => _Rules.Add(rule);
		public bool RemoveRule(int index) => index >= 0 && index < _Rules.Count && _Rules.Remove(_Rules[index]);
		public bool RemoveRule(string rule) => _Rules.Remove(rule);
		public void ChangeName(string name) => Name = name;
		public void ChangeRule(int index, string text)
		{
			if (index >= 0 && index < Rules.Count)
			{
				_Rules[index] = text;
			}
		}

		public override string ToString() => ToString(new RuleFormatter(), 0);
		public string ToString(RuleFormatter formatter, int index)
		{
			var sb = new StringBuilder();
			sb.AppendLineFeed(formatter.FormatName(Name, 0));
			for (int r = 0; r < Rules.Count; ++r)
			{
				sb.AppendLineFeed(formatter.FormatRule(Rules[r], r, Rules.Count));
			}
			return sb.ToString();
		}
		public string ToString(SocketGuild guild) => ToString();
	}
}
