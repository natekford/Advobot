using System.Collections.Generic;
using System.Linq;
using Advobot.Classes.Settings;
using AdvorangesUtils;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through quotes.
	/// </summary>
	public sealed class CloseQuotes : CloseWords<Quote>
	{
		/// <summary>
		/// Creates an instance of <see cref="CloseQuotes"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <param name="maxAllowedCloseness"></param>
		/// <param name="maxOutput"></param>
		public CloseQuotes(IEnumerable<Quote> source, string search, int maxAllowedCloseness = 4, int maxOutput = 5)
			: base(source, search, maxAllowedCloseness, maxOutput)
		{
			Matches = FindMatches();
		}

		/// <inheritdoc />
		protected override bool IsCloseWord(Quote obj, out CloseWord closeWord)
		{
			var closeness = FindCloseness(obj.Name, Search);
			return (closeWord = closeness < MaxAllowedCloseness ? new CloseWord(closeness, obj.Name, obj.Description) : null) != null;
		}
		/// <inheritdoc />
		protected override bool TryGetCloseWord(IEnumerable<Quote> objs, IEnumerable<string> used, out CloseWord closeWord)
		{
			var obj = objs.FirstOrDefault(x => !used.Contains(x.Name) && x.Name.CaseInsContains(Search));
			return (closeWord = obj != null ? new CloseWord(int.MaxValue, obj.Name, obj.Description) : null) != null;
		}
	}
}