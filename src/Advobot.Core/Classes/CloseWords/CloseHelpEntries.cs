using System;
using System.Linq;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through <see cref="Constants.HELP_ENTRIES"/>.
	/// </summary>
	public class CloseHelpEntries : CloseWords<HelpEntry>
	{
		public CloseHelpEntries(string input) : base(Constants.HELP_ENTRIES.GetHelpEntries(), input) { }

		protected override int FindCloseness(HelpEntry obj, string input)
		{
			var nameCloseness = FindCloseName(obj.Name, input);
			var aliasCloseness = obj.Aliases.Select(x => FindCloseName(x, input)).DefaultIfEmpty(int.MaxValue).Min();
			return Math.Min(nameCloseness, aliasCloseness);
		}
	}
}
