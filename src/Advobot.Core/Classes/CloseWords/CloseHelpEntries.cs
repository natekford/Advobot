using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.CloseWords
{
	public class CloseHelpEntries : CloseWords<HelpEntry>
    {
		public CloseHelpEntries(IEnumerable<HelpEntry> suppliedObjects, string input) : base(suppliedObjects, input) { }

		protected override int FindCloseness(HelpEntry obj, string input)
		{
			var nameCloseness = FindCloseName(obj.Name, input);
			var aliasCloseness = FindCloseAlias(obj.Aliases, input);
			return Math.Min(nameCloseness, aliasCloseness);
		}
		private int FindCloseAlias(string[] aliases, string input)
			=> aliases.Select(x => FindCloseName(x, input)).DefaultIfEmpty(int.MaxValue).Min();
	}
}
