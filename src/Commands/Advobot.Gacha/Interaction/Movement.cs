namespace Advobot.Gacha.Interaction
{
	public sealed class Movement : IInteraction
	{
		public Movement(string name, int value)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; }
		public int Value { get; }

		public bool TryUpdatePage(ref int currentPage, int pageCount)
		{
			var current = currentPage;
			//Don't use standard % because it does not do what we want for negative values
			currentPage = (currentPage + (Value % pageCount) + pageCount) % pageCount;
			return current != currentPage;
		}
	}
}