using Discord;

namespace Advobot.Gacha.MenuEmojis
{
	public sealed class MovementEmoji : Emoji, IMenuEmote
	{
		public int Value { get; }

		public MovementEmoji(Emoji emoji, int value) : this(emoji.Name, value) { }
		public MovementEmoji(string unicode, int value) : base(unicode)
		{
			Value = value;
		}

		public bool TryUpdatePage(ref int currentPage, int pageCount)
		{
			var current = currentPage;
			//Don't use standard % because it does not do what we want for negative values
			currentPage = (currentPage + Value % pageCount + pageCount) % pageCount;
			return current != currentPage;
		}
	}
}
