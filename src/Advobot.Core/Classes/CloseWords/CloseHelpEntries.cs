using Advobot.Core.Utilities;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through <see cref="Constants.HELP_ENTRIES"/>.
	/// </summary>
	public class CloseHelpEntries : CloseWords<HelpEntry>
	{
		public CloseHelpEntries() { }
		public CloseHelpEntries(TimeSpan time, ICommandContext context, HelpEntryHolder helpEntryHolder, string search) 
			: base(time, context, helpEntryHolder.GetHelpEntries(), search) { }

		protected override CloseWord FindCloseWord(HelpEntry obj, string search)
		{
			var nameCloseness = FindCloseness(obj.Name, search);
			var aliasCloseness = obj.Aliases.Select(x => FindCloseness(x, search)).DefaultIfEmpty(int.MaxValue).Min();
			var closeness = Math.Min(nameCloseness, aliasCloseness);
			return closeness > MaxAllowedCloseness ? null : new CloseWord(closeness, obj.Name, obj.ToString());
		}
		protected override CloseWord FindCloseWord(IEnumerable<HelpEntry> objs, IEnumerable<string> alreadyUsedNames, string search)
		{
			var obj = objs.FirstOrDefault(x => !alreadyUsedNames.Contains(x.Name) && x.Name.CaseInsContains(search));
			return obj == null ? null : new CloseWord(int.MaxValue, obj.Name, obj.ToString());
		}
	}
}
