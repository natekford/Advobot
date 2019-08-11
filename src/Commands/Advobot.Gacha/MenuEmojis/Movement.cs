namespace Advobot.Gacha.MenuEmojis
{
	public sealed class Movement : IMenuAction
	{
		public string Name { get; }
		public int Value { get; }

		public Movement(string name, int value)
		{
			Name = name;
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
