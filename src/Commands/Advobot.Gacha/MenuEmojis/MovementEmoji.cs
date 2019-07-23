using Discord;

namespace Advobot.Gacha.MenuEmojis
{
	public sealed class MovementEmoji : Emoji
	{
		public int Value { get; }

		public MovementEmoji(Emoji emoji, int value) : this(emoji.Name, value) { }
		public MovementEmoji(string unicode, int value) : base(unicode)
		{
			Value = value;
		}

		public bool TryUpdatePage(ref int i, int m)
		{
			var current = i;
			//Don't use standard % because it does not do what we want for negative values
			i = (i + Value % m + m) % m;
			return current != i;
		}
	}
}
