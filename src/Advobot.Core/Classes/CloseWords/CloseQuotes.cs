using Advobot.Core.Interfaces;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through <see cref="IGuildSettings.Quotes"/>.
	/// </summary>
	public class CloseQuotes : CloseWords<Quote>
	{
		public CloseQuotes(IGuildSettings settings, string input) : base(settings.Quotes, input) { }

		protected override int FindCloseness(Quote obj, string input) => FindCloseName(obj.Name, input);
	}
}
