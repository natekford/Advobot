using Advobot.Utilities;

using Discord.Commands;

using System.Resources;

namespace Advobot.Localization;

/// <summary>
/// Used for a localized summary.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class LocalizedSummaryAttribute(string name, ResourceManager resources) : SummaryAttribute(resources.GetStringEnsured(name)), ILocalized
{
	private static readonly ResourceManager _RM = Resources.Summaries.ResourceManager;

	/// <summary>
	/// The name of the summary to use for localization.
	/// </summary>
	public string Name { get; } = name;
	/// <inheritdoc />
	public ResourceManager ResourceManager { get; } = resources;

	/// <summary>
	/// Creates an instance of <see cref="LocalizedSummaryAttribute"/>.
	/// </summary>
	/// <param name="name"></param>
	public LocalizedSummaryAttribute(string name) : this(name, _RM)
	{
	}
}