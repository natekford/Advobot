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
			Text = text;
		}

		public void ChangeText(string text)
		{
			Text = text;
		}

		public override string ToString()
		{
			return ToString(new RuleFormatter(), 0, 0).ToString();
		}
		public string ToString(RuleFormatter formatter, int index, int rulesInCategory)
		{
			return formatter.FormatRule(this, index, rulesInCategory);
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
