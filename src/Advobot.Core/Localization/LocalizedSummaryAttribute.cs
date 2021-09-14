using System.Resources;

using Advobot.Utilities;

using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized summary.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class LocalizedSummaryAttribute : SummaryAttribute, ILocalized
	{
		private static readonly ResourceManager _RM = Resources.Summaries.ResourceManager;

		/// <summary>
		/// The name of the summary to use for localization.
		/// </summary>
		public string Name { get; }
		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedSummaryAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public LocalizedSummaryAttribute(string name) : this(name, _RM)
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="LocalizedSummaryAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		public LocalizedSummaryAttribute(string name, ResourceManager resources)
			: base(resources.GetStringEnsured(name))
		{
			Name = name;
			ResourceManager = resources;
		}
	}
}