namespace Advobot.Core.Classes
{
	public struct CommandCategory
    {
		public string Name { get; }

		public CommandCategory(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
