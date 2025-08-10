namespace Advobot.Services.Help;

/// <summary>
/// Contains information about a module.
/// </summary>
public interface IHelpModule : IHelpItem
{
	/// <summary>
	/// Whether or not the command can be toggled.
	/// </summary>
	bool AbleToBeToggled { get; }
	/// <summary>
	/// Other names to invoke the command.
	/// </summary>
	IReadOnlyList<string> Aliases { get; }
	/// <summary>
	/// The category the command is in.
	/// </summary>
	string Category { get; }
	/// <summary>
	/// The overloads/actual commands in this module.
	/// </summary>
	IReadOnlyList<IHelpCommand> Commands { get; }
	/// <summary>
	/// Whether or not the command is on by default.
	/// </summary>
	bool EnabledByDefault { get; }
	/// <summary>
	/// The constant Id for this help entry.
	/// </summary>
	string Id { get; }
	/// <summary>
	/// The base permissions to use the command.
	/// </summary>
	IReadOnlyList<IHelpPrecondition> Preconditions { get; }
	/// <summary>
	/// Nested modules of this module.
	/// </summary>
	IReadOnlyList<IHelpModule> Submodules { get; }
}