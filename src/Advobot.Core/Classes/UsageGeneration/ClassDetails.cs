using Advobot.Core.Interfaces;
using Discord.Commands;
using System;
using System.Reflection;

namespace Advobot.Core.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a class to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal class ClassDetails : IArgument
	{
		public int Deepness { get; }
		public string Name { get; }

		public ClassDetails(int deepness, Type classType)
		{
			this.Deepness = deepness;
			this.Name = classType.GetCustomAttribute<GroupAttribute>()?.Prefix;
		}

		public override string ToString() => this.Name;
	}
}