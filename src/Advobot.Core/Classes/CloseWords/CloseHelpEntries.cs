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
		public CloseHelpEntries(TimeSpan time, ICommandContext context, string input) 
			: base(time, context, Constants.HELP_ENTRIES.GetHelpEntries(), input) { }

		protected override CloseWord FindCloseWord(HelpEntry obj, string input)
		{
			var nameCloseness = FindCloseName(obj.Name, input);
			var aliasCloseness = obj.Aliases.Select(x => FindCloseName(x, input)).DefaultIfEmpty(int.MaxValue).Min();
			return new CloseWord(Math.Min(nameCloseness, aliasCloseness), obj.Name, obj.ToString());
		}
		protected override CloseWord FindCloseWord(IEnumerable<HelpEntry> objs, IEnumerable<string> alreadyUsedNames, string input)
		{
			var obj = objs.FirstOrDefault(x => !alreadyUsedNames.Contains(x.Name) && x.Name.CaseInsContains(input));
			return new CloseWord(int.MaxValue, obj.Name, obj.ToString());
		}
	}
}
