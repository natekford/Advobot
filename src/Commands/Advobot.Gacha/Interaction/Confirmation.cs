namespace Advobot.Gacha.Interaction;

public sealed class Confirmation(string name, bool value) : IInteraction
{
	public string Name { get; } = name;

	public bool Value { get; } = value;
}