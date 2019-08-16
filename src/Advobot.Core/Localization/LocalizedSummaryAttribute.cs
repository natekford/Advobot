using System;
using System.Globalization;
using System.Resources;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized summary.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedSummaryBaseAttribute : SummaryAttribute
	{
		/// <summary>
		/// The name of the summary to use for localization.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The resource manager containing the 
		/// </summary>
		protected ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedSummaryBaseAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resources"></param>
		public LocalizedSummaryBaseAttribute(string name, ResourceManager resources)
			: base(resources.GetString(name))
		{
			Name = name;
			ResourceManager = resources;
			ConsoleUtils.DebugWrite($"Current culture: {CultureInfo.CurrentCulture}");
		}
	}
}
