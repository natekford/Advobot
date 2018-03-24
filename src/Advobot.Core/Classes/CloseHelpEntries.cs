using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through help entries.
	/// </summary>
	public sealed class CloseHelpEntries : CloseWords<HelpEntry>
	{
		/// <summary>
		/// Initializes the object. Parameterless constructor is used for the database.
		/// </summary>
		public CloseHelpEntries() { }
		/// <summary>
		/// Initializes the object with the supplied values.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="context"></param>
		/// <param name="helpEntryHolder"></param>
		/// <param name="search"></param>
		public CloseHelpEntries(TimeSpan time, ICommandContext context, HelpEntryHolder helpEntryHolder, string search) 
			: base(time, context, helpEntryHolder.GetHelpEntries(), search) { }

		/// <inheritdoc />
		protected override bool IsCloseWord(HelpEntry obj, string search, out CloseWord closeWord)
		{
			var nameCloseness = FindCloseness(obj.Name, search);
			var aliasCloseness = obj.Aliases.Select(x => FindCloseness(x, search)).DefaultIfEmpty(int.MaxValue).Min();
			var closeness = Math.Min(nameCloseness, aliasCloseness);
			var success = closeness < MaxAllowedCloseness;
			closeWord = success ? new CloseWord(closeness, obj.Name, obj.ToString()) : null;
			return success;
		}
		/// <inheritdoc />
		protected override bool TryGetCloseWord(
			IEnumerable<HelpEntry> objs,
			IEnumerable<string> used,
			string search,
			out CloseWord closeWord)
		{
			var obj = objs.FirstOrDefault(x => !used.Contains(x.Name) && x.Name.CaseInsContains(search));
			closeWord = obj != null ? new CloseWord(int.MaxValue, obj.Name, obj.ToString()) : null;
			return obj != null;
		}
	}
}
