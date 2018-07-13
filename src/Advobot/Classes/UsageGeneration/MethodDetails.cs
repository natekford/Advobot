using System.Linq;
using System.Reflection;
using Discord.Commands;

namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a method to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal class MethodDetails
	{
		public int Deepness { get; }
		public string Name { get; }
		public bool HasNoArgs { get; }
		public int ArgCount { get; }

		public MethodDetails(int deepness, MethodInfo method)
		{
			Deepness = deepness;
			Name = method.GetCustomAttribute<CommandAttribute>()?.Text;
			ArgCount = method.GetParameters().Length;
			HasNoArgs = ArgCount == 0;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}