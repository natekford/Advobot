namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Information about something to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal class UsageDetails
	{
		public int Deepness { get; protected set; }
		public string Name { get; protected set; }

		public UsageDetails(int deepness, string name)
		{
			Deepness = deepness;
			Name = !string.IsNullOrWhiteSpace(name) ? CapitalizeFirstLetter(name) : name;
		}

		protected static string CapitalizeFirstLetter(string n)
		{
			return n[0].ToString().ToUpper() + n.Substring(1, n.Length - 1);
		}
		public override string ToString()
		{
			return Name;
		}
	}
}