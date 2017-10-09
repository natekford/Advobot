using Advobot.Interfaces;
using Discord.Commands;
using System.Reflection;

namespace Advobot.Classes.UsageGeneration
{
	internal class MethodDetails : IArgument
	{
		public int Deepness { get; }
		public string Name { get; }

		public MethodDetails(int deepness, MethodInfo method)
		{
			Deepness = deepness;
			Name = method.GetCustomAttribute<CommandAttribute>()?.Text;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}