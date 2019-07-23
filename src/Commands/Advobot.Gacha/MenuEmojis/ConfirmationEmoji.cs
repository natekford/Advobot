using Discord;

namespace Advobot.Gacha.MenuEmojis
{
	public sealed class ConfirmationEmoji : Emoji
	{
		public bool Value { get; }

		public ConfirmationEmoji(Emoji emoji, bool value) : this(emoji.Name, value) { }
		public ConfirmationEmoji(string unicode, bool value) : base(unicode)
		{
			Value = value;
		}
	}
}
