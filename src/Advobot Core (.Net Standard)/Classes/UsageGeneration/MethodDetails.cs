using Advobot.Interfaces;
using Discord.Commands;
using System.Linq;
using System.Reflection;

namespace Advobot.Classes.UsageGeneration
{
	internal class MethodDetails : IArgument
	{
		public int Deepness { get; }
		public string Name { get; }
		public bool HasNoArgs { get; }
		public int ArgCount { get; }

		public MethodDetails(int deepness, MethodInfo method)
		{
			Deepness = deepness;
			Name = method.GetCustomAttribute<CommandAttribute>()?.Text;
			ArgCount = method.GetParameters().Count();
			HasNoArgs = ArgCount == 0;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}