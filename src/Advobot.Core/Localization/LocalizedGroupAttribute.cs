using System.Resources;

using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized group.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class LocalizedGroupAttribute : GroupAttribute, ILocalized
	{
		private static readonly ResourceManager _RM = Resources.Groups.ResourceManager;

		/// <summary>
		/// The name of the group to use for localization.
		/// </summary>
		public string Name { get; }
		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedGroupAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public LocalizedGroupAttribute(string name) : this(name, _RM)
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="LocalizedGroupAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		public LocalizedGroupAttribute(string name, ResourceManager resources)
			: base(resources.GetStringEnsured(name))
		{
			Name = name;
			ResourceManager = resources;
		}
	}
}