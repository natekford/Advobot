using Advobot.Utilities;

using Discord.Commands;

using System.Resources;

namespace Advobot.Localization;

/// <summary>
/// Used for a localized group.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class LocalizedGroupAttribute(string name, ResourceManager resources)
	: GroupAttribute(resources.GetStringEnsured(name)), ILocalized
{
	private static readonly ResourceManager _RM = Resources.Groups.ResourceManager;

	/// <summary>
	/// The name of the group to use for localization.
	/// </summary>
	public string Name { get; } = name;
	/// <inheritdoc />
	public ResourceManager ResourceManager { get; } = resources;

	/// <summary>
	/// Creates an instance of <see cref="LocalizedGroupAttribute"/>.
	/// </summary>
	/// <param name="name"></param>
	public LocalizedGroupAttribute(string name) : this(name, _RM)
	{
	}
}