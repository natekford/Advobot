using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Core.Classes.Rules
{
	public class Rule : ISetting
	{
		[JsonProperty]
		public string Text { get; private set; }

		public Rule(string text)
		{
			this.Text = text;
		}

		public void ChangeText(string text) => this.Text = text;

		public override string ToString() => ToString(new RuleFormatter(), 0, 0).ToString();
		public string ToString(RuleFormatter formatter, int index, int rules) => formatter.FormatRule(this, index, rules);
		public string ToString(SocketGuild guild) => ToString();
	}
}
