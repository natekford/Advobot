namespace Advobot.Services.Help;

/// <summary>
/// Contains information about a command.
/// </summary>
public interface IHelpCommand : IHelpItem
{
	/// <summary>
	/// Other names to invoke the command.
	/// </summary>
	IReadOnlyList<string> Aliases { get; }
	/// <summary>
	/// The parameters to use this command.
	/// </summary>
	IReadOnlyList<IHelpParameter> Parameters { get; }
	/// <summary>
	/// The base permissions to use the command.
	/// </summary>
	IReadOnlyList<IHelpPrecondition> Preconditions { get; }
}