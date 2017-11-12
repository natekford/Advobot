using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Core.Classes.Rules
{
	public class RuleHolder : ISetting
	{
		[JsonProperty("Categories")]
		private List<RuleCategory> _Categories = new List<RuleCategory>();
		[JsonIgnore]
		public IReadOnlyList<RuleCategory> Categories => _Categories.AsReadOnly();

		public void AddCategory(RuleCategory category) => _Categories.Add(category);
		public bool RemoveCategory(int index)
		{
			if (index >= 0 && index < _Categories.Count)
			{
				_Categories.RemoveAt(index);
				return true;
			}
			return false;
		}
		public bool RemoveCategory(RuleCategory category) => _Categories.Remove(category);
		public void ChangeCategory(int index, RuleCategory category)
		{
			if (index >= 0 && index < _Categories.Count)
			{
				_Categories[index] = category;
			}
		}

		public override string ToString() => ToString(new RuleFormatter());
		public string ToString(RuleFormatter formatter)
		{
			var sb = new StringBuilder();
			for (int c = 0; c < _Categories.Count; ++c)
			{
				sb.AppendLineFeed(_Categories[c].ToString(formatter, c));
			}
			return sb.ToString();
		}
		public string ToString(SocketGuild guild) => ToString();
	}
}
