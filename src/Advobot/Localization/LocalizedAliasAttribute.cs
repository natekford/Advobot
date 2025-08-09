using Advobot.Utilities;

using Discord.Commands;

using System.Resources;

namespace Advobot.Localization;

/// <summary>
/// Used for a localized alias.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class LocalizedAliasAttribute(string[] names, ResourceManager resources)
	: AliasAttribute(Array.ConvertAll(names, resources.GetStringEnsured)), ILocalized
{
	private static readonly ResourceManager _RM = Resources.Aliases.ResourceManager;

	/// <summary>
	/// The names of the aliases to use for localization.
	/// </summary>
	public IReadOnlyList<string> Names { get; } = names;
	/// <inheritdoc />
	public ResourceManager ResourceManager { get; } = resources;

	/// <summary>
	/// Creates an instance of <see cref="LocalizedAliasAttribute"/>.
	/// </summary>
	/// <param name="names"></param>
	public LocalizedAliasAttribute(params string[] names) : this(names, _RM)
	{
	}
}