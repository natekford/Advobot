using Advobot.Utilities;

using Discord.Commands;

using System.Resources;

namespace Advobot.Localization;

/// <summary>
/// Used for a localized command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class LocalizedCommandAttribute(string name, ResourceManager resources)
	: CommandAttribute(resources.GetStringEnsured(name)), ILocalized
{
	private static readonly ResourceManager _RM = Resources.Groups.ResourceManager;

	/// <summary>
	/// The name of the command to use for localization.
	/// </summary>
	public string Name { get; } = name;
	/// <inheritdoc />
	public ResourceManager ResourceManager { get; } = resources;

	/// <summary>
	/// Creates an instance of <see cref="LocalizedCommandAttribute"/>.
	/// </summary>
	/// <param name="name"></param>
	public LocalizedCommandAttribute(string name) : this(name, _RM)
	{
	}
}