using Advobot.Quotes.ReadOnlyModels;

namespace Advobot.Quotes.Models
{
	public sealed class Quote : IReadOnlyQuote
	{
		public string Description { get; set; } = "";
		public ulong GuildId { get; set; }
		public string Name { get; set; } = "";
	}
}