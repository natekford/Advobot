namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a class to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal sealed class ClassDetails : UsageDetails
	{
		public ClassDetails(int deepness, string name) : base(deepness, name) { }
	}
}