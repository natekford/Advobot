using System;
using System.Collections.Generic;
using System.Linq;

using Advobot.Classes.CloseWords;

using AdvorangesUtils;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through help entries.
	/// </summary>
	internal sealed class CloseHelpEntries : CloseWords<IModuleHelpEntry>
	{
		/// <summary>
		/// Creates an instance of <see cref="CloseHelpEntries"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="maxAllowedCloseness"></param>
		/// <param name="maxOutput"></param>
		public CloseHelpEntries(IEnumerable<IModuleHelpEntry> source, int maxAllowedCloseness = 4, int maxOutput = 5)
			: base(source, maxAllowedCloseness, maxOutput) { }

		/// <inheritdoc />
		protected override bool IsCloseWord(string search, IModuleHelpEntry obj, out CloseWord<IModuleHelpEntry>? closeWord)
		{
			var nameCloseness = FindCloseness(obj.Name, search);
			var aliasCloseness = obj.Aliases.Select(x => FindCloseness(x, search)).DefaultIfEmpty(int.MaxValue).Min();
			var closeness = Math.Min(nameCloseness, aliasCloseness);
			var success = closeness < MaxAllowedCloseness || obj.Name.CaseInsContains(search);
			closeWord = success ? new CloseWord<IModuleHelpEntry>(closeness, obj.Name, obj) : null;
			return success;
		}
	}
}