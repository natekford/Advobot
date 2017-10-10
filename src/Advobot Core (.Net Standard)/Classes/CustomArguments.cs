using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot.Classes
{
	public class CustomArguments
	{
		private Dictionary<string, string> _Args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public CustomArguments(string input)
		{
			var unseperatedArgs = input.Split('"').Select((element, index) =>
			{
				return index % 2 == 0
					? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
					: new[] { element };
			}).SelectMany(x => x).Where(x => !String.IsNullOrWhiteSpace(x));

			foreach (var arg in unseperatedArgs)
			{
				var split = arg.Split(new[] { ':' }, 2);
				if (split.Length == 2)
				{
					_Args.Add(split[0], split[1]);
				}
			}
		}

		public string this[string name]
		{
			get => _Args.ContainsKey(name) ? _Args[name] : null;
		}

		public void LimitArgsToTheseNames(IEnumerable<string> names)
		{
			foreach (var key in _Args.Keys.ToList())
			{
				if (!names.CaseInsContains(key))
				{
					_Args.Remove(key);
				}
			}
		}

		public bool TryGetValue(string name, out string value)
		{
			return _Args.TryGetValue(name, out value);
		}
		public bool TryGetInt(string name, out int value)
		{
			if (_Args.TryGetValue(name, out string val))
			{
				return int.TryParse(val, out value);
			}
			else
			{
				value = -1;
				return false;
			}
		}
	}
}
