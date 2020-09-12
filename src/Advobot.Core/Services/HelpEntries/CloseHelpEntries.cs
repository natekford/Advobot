using System;
using System.Collections.Generic;

using Advobot.Classes.CloseWords;

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
		public CloseHelpEntries(IEnumerable<IModuleHelpEntry> source)
			: base(source, x => x.Name) { }

		/// <inheritdoc />
		protected override int FindCloseness(string search, IModuleHelpEntry obj)
		{
			var closeness = FindCloseness(obj.Name, search);
			foreach (var alias in obj.Aliases)
			{
				closeness = Math.Min(closeness, FindCloseness(alias, search));
			}
			return closeness;
		}
	}
}