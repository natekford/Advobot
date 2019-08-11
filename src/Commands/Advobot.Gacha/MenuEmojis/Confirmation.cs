namespace Advobot.Gacha.MenuEmojis
{
	public sealed class Confirmation : IMenuAction
	{
		public string Name { get; }
		public bool Value { get; }

		public Confirmation(string name, bool value)
		{
			Name = name;
			Value = value;
		}
	}
}
