using System;
using System.Resources;

using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedCommandBaseAttribute : CommandAttribute, ILocalized
	{
		/// <summary>
		/// The name of the command to use for localization.
		/// </summary>
		public string Name { get; }

		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedCommandBaseAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		protected LocalizedCommandBaseAttribute(string name, ResourceManager resources)
			: base(resources.GetStringEnsured(name))
		{
			Name = name;
			ResourceManager = resources;
		}
	}
}