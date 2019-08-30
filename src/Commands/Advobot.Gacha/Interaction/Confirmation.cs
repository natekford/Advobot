namespace Advobot.Gacha.Interaction
{
	public sealed class Confirmation : IInteraction
	{
		public Confirmation(string name, bool value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; }
		public bool Value { get; }
	}
}