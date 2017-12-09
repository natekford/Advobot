using Advobot.Core.Interfaces;
using Discord.Commands;
using System.Linq;
using System.Reflection;

namespace Advobot.Core.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a method to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal class MethodDetails : IArgument
	{
		public int Deepness { get; }
		public string Name { get; }
		public bool HasNoArgs { get; }
		public int ArgCount { get; }

		public MethodDetails(int deepness, MethodInfo method)
		{
			this.Deepness = deepness;
			this.Name = method.GetCustomAttribute<CommandAttribute>()?.Text;
			this.ArgCount = method.GetParameters().Count();
			this.HasNoArgs = this.ArgCount == 0;
		}

		public override string ToString() => this.Name;
	}
}