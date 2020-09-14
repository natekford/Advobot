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
	public class LocalizedCommandAttribute : CommandAttribute, ILocalized
	{
		private static readonly ResourceManager _RM = Resources.Groups.ResourceManager;

		/// <summary>
		/// The name of the command to use for localization.
		/// </summary>
		public string Name { get; }
		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedCommandAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public LocalizedCommandAttribute(string name) : this(name, _RM)
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="LocalizedCommandAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		public LocalizedCommandAttribute(string name, ResourceManager resources)
			: base(resources.GetStringEnsured(name))
		{
			Name = name;
			ResourceManager = resources;
		}
	}
}