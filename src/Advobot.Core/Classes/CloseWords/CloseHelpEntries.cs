using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using AdvorangesUtils;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through help entries.
	/// </summary>
	public sealed class CloseHelpEntries : CloseWords<IHelpEntry>
	{
		private CommandSettings _Settings { get; }

		/// <summary>
		/// Creates an instance of <see cref="CloseHelpEntries"/>.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <param name="maxAllowedCloseness"></param>
		/// <param name="maxOutput"></param>
		public CloseHelpEntries(CommandSettings settings, IEnumerable<IHelpEntry> source, string search, int maxAllowedCloseness = 4, int maxOutput = 5)
			: base(source, search, maxAllowedCloseness, maxOutput)
		{
			_Settings = settings;
			Matches = FindMatches();
		}

		/// <inheritdoc />
		protected override bool IsCloseWord(IHelpEntry obj, out CloseWord closeWord)
		{
			var nameCloseness = FindCloseness(obj.Name, Search);
			var aliasCloseness = obj.Aliases.Select(x => FindCloseness(x, Search)).DefaultIfEmpty(int.MaxValue).Min();
			var closeness = Math.Min(nameCloseness, aliasCloseness);
			return (closeWord = closeness < MaxAllowedCloseness ? new CloseWord(closeness, obj.Name, obj.ToString(_Settings)) : null) != null;
		}
		/// <inheritdoc />
		protected override bool TryGetCloseWord(IEnumerable<IHelpEntry> objs, IEnumerable<string> used, out CloseWord closeWord)
		{
			var obj = objs.FirstOrDefault(x => !used.Contains(x.Name) && x.Name.CaseInsContains(Search));
			return (closeWord = obj != null ? new CloseWord(int.MaxValue, obj.Name, obj.ToString(_Settings)) : null) != null;
		}
	}
}
