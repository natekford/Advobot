using Advobot.Interfaces;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Advobot.Classes.Rules
{
	public class RuleCategory : ISetting
	{
		public string Name { get; private set; }
		public IReadOnlyList<string> Rules => _Rules.AsReadOnly();
		private List<string> _Rules = new List<string>();

		public RuleCategory(string name)
		{
			Name = name;
		}

		public void ChangeName(string name)
		{
			Name = name;
		}
		public void AddRule(string rule)
		{
			_Rules.Add(rule);
		}
		public void RemoveRule(int index)
		{
			if (index >= 0 && index < _Rules.Count)
			{
				_Rules.RemoveAt(index);
			}
		}
		public void UpdateRule(int index, string rule)
		{
			if (index >= 0 && index < Rules.Count)
			{
				_Rules[index] = rule;
			}
		}

		public override string ToString()
		{
			return new RuleFormatter().FormatRuleCategory(this);
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}
