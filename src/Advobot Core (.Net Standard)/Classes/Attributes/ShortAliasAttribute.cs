using Advobot.Enums;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Classes.Attributes
{
	public class ShortAliasAttribute : AliasAttribute
	{
		private static Dictionary<string, string> _PredefinedAliases = new Dictionary<string, string>
		{
			{ nameof(ActionType.Add),     "a"   },
			{ nameof(ActionType.Clear),   "clr" },
			{ nameof(ActionType.Create),  "cre" },
			{ nameof(ActionType.Current), "cur" },
			{ nameof(ActionType.Default), "def" },
			{ nameof(ActionType.Delete),  "del" },
			{ nameof(ActionType.Disable), "dis" },
			{ nameof(ActionType.Enable),  "e"   },
			{ nameof(ActionType.Modify),  "m"   },
			{ nameof(ActionType.Remove),  "r"   },
			{ nameof(ActionType.Setup),   "set" },
			{ nameof(ActionType.Show),    "sh"  },
		};

		public ShortAliasAttribute(string name) : base(Shorten(name)) { }

		private static string Shorten(string name)
		{
			if (_PredefinedAliases.TryGetValue(name, out var value))
			{
				return value;
			}

			var acronym = CreateAcronym(name);
			if (String.IsNullOrWhiteSpace(acronym))
			{
				throw new ArgumentException("Invalid alias provided. Must have at least one capital letter.");
			}

			return acronym;
		}
		private static string CreateAcronym(string alias)
		{
			var sb = new StringBuilder();
			foreach (var c in alias)
			{
				if (Char.IsUpper(c))
				{
					sb.Append(c);
				}
			}
			return sb.ToString().ToLower();
		}
	}
}
