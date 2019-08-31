using System;
using System.Resources;

using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized name.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedNameBaseAttribute : NameAttribute, ILocalized
	{
		/// <summary>
		/// The name of the summary to use for localization.
		/// </summary>
		public string Name { get; }

		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedSummaryBaseAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		protected LocalizedNameBaseAttribute(string name, ResourceManager resources)
			: base(resources.GetString(name))
		{
			Name = name;
			ResourceManager = resources;
		}
	}
}