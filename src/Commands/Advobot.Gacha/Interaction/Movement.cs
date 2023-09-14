namespace Advobot.Gacha.Interaction;

public sealed class Movement(string name, int value) : IInteraction
{
	public string Name { get; } = name;

	public int Value { get; } = value;

	public bool TryUpdatePage(ref int currentPage, int pageCount)
	{
		var current = currentPage;
		//Don't use standard % because it does not do what we want for negative values
		currentPage = (currentPage + (Value % pageCount) + pageCount) % pageCount;
		return current != currentPage;
	}
}