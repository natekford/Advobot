namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a method to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal sealed class MethodDetails : UsageDetails
	{
		public bool HasNoArgs { get; }
		public int ArgCount { get; }

		public MethodDetails(int deepness, string name, int parameters) : base(deepness, name)
		{
			ArgCount = parameters;
			HasNoArgs = ArgCount == 0;
		}
	}
}