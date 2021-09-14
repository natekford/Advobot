using System.Resources;

using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized parameter name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class LocalizedNameAttribute : NameAttribute, ILocalized
	{
		private static readonly ResourceManager _RM = Resources.Parameters.ResourceManager;

		/// <summary>
		/// The name of the summary to use for localization.
		/// </summary>
		public string Name { get; }
		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedNameAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public LocalizedNameAttribute(string name) : this(name, _RM)
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="LocalizedSummaryAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		public LocalizedNameAttribute(string name, ResourceManager resources)
			: base(resources.GetStringEnsured(name))
		{
			Name = name;
			ResourceManager = resources;
		}
	}
}