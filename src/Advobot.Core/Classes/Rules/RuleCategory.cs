using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Core.Classes.Rules
{
	public class RuleCategory : ISetting
	{
		[JsonProperty]
		public string Name { get; private set; }
		[JsonProperty("Rules")]
		private List<Rule> _Rules = new List<Rule>();
		[JsonIgnore]
		public IReadOnlyList<Rule> Rules => this._Rules.AsReadOnly();

		public RuleCategory(string name)
		{
			this.Name = name;
		}

		public void AddRule(Rule rule) => this._Rules.Add(rule);
		public bool RemoveRule(int index)
		{
			if (index >= 0 && index < this._Rules.Count)
			{
				this._Rules.RemoveAt(index);
				return true;
			}
			return false;
		}
		public bool RemoveRule(Rule rule) => this._Rules.Remove(rule);
		public void ChangeName(string name) => this.Name = name;
		public void ChangeRule(int index, string text)
		{
			if (index >= 0 && index < this.Rules.Count)
			{
				this._Rules[index].ChangeText(text);
			}
		}

		public override string ToString() => ToString(new RuleFormatter(), 0);
		public string ToString(RuleFormatter formatter, int index)
		{
			var sb = new StringBuilder();
			sb.AppendLineFeed(formatter.FormatName(this, 0));
			for (int r = 0; r < this.Rules.Count; ++r)
			{
				sb.AppendLineFeed(this.Rules[r].ToString(formatter, r, this.Rules.Count));
			}
			return sb.ToString();
		}
		public string ToString(SocketGuild guild) => ToString();
	}
}
