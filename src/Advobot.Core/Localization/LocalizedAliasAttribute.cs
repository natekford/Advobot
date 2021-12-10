using Advobot.Utilities;

using Discord.Commands;

using System.Resources;

namespace Advobot.Localization;

/// <summary>
/// Used for a localized alias.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class LocalizedAliasAttribute : AliasAttribute, ILocalized
{
	private static readonly ResourceManager _RM = Resources.Aliases.ResourceManager;

	/// <summary>
	/// The names of the aliases to use for localization.
	/// </summary>
	public IReadOnlyList<string> Names { get; }
	/// <inheritdoc />
	public ResourceManager ResourceManager { get; }

	/// <summary>
	/// Creates an instance of <see cref="LocalizedAliasAttribute"/>.
	/// </summary>
	/// <param name="names"></param>
	public LocalizedAliasAttribute(params string[] names) : this(names, _RM)
	{
	}

	/// <summary>
	/// Creates an instance of <see cref="LocalizedAliasAttribute"/>.
	/// </summary>
	/// <param name="names"></param>
	/// <param name="resources"></param>
	public LocalizedAliasAttribute(string[] names, ResourceManager resources)
		: base(Array.ConvertAll(names, x => resources.GetStringEnsured(x)))
	{
		Names = names;
		ResourceManager = resources;
	}
}