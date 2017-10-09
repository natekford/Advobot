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
		public bool NoArgs { get; }

		public MethodDetails(int deepness, MethodInfo method)
		{
			Deepness = deepness;
			Name = method.GetCustomAttribute<CommandAttribute>()?.Text;
			NoArgs = !method.GetParameters().Any();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}