using System;
using System.Resources;

using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized summary.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedSummaryBaseAttribute : SummaryAttribute, ILocalized
	{
		/// <summary>
		/// Creates an instance of <see cref="LocalizedSummaryBaseAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		protected LocalizedSummaryBaseAttribute(string name, ResourceManager resources)
			: base(resources.GetString(name))
		{
			Name = name;
			ResourceManager = resources;
		}

		/// <summary>
		/// The name of the summary to use for localization.
		/// </summary>
		public string Name { get; }

		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }
	}
}