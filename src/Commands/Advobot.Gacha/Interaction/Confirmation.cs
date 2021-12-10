namespace Advobot.Gacha.Interaction;

public sealed class Confirmation : IInteraction
{
	public string Name { get; }

	public bool Value { get; }

	public Confirmation(string name, bool value)
	{
		Name = name;
		Value = value;
	}
}