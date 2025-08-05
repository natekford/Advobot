using Advobot.Utilities;

using Discord.Commands;

using System.Resources;

namespace Advobot.Localization;

/// <summary>
/// Used for a localized parameter name.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class LocalizedNameAttribute(string name, ResourceManager resources) : NameAttribute(resources.GetStringEnsured(name)), ILocalized
{
	private static readonly ResourceManager _RM = Resources.Parameters.ResourceManager;

	/// <summary>
	/// The name of the summary to use for localization.
	/// </summary>
	public string Name { get; } = name;
	/// <inheritdoc />
	public ResourceManager ResourceManager { get; } = resources;

	/// <summary>
	/// Creates an instance of <see cref="LocalizedNameAttribute"/>.
	/// </summary>
	/// <param name="name"></param>
	public LocalizedNameAttribute(string name) : this(name, _RM)
	{
	}
}