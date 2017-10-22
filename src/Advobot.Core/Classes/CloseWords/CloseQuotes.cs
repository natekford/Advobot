using System.Collections.Generic;

namespace Advobot.Core.Classes.CloseWords
{
	public class CloseQuotes : CloseWords<Quote>
	{
		public CloseQuotes(IEnumerable<Quote> suppliedObjects, string input)
			: base(suppliedObjects, input) { }

		protected override int FindCloseness(Quote obj, string input)
		{
			return FindCloseName(obj.Name, input);
		}
	}
}
