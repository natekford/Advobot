using System.Collections;

namespace Advobot.Services.HelpEntries;

/// <summary>
/// Contains information about a parameter precondition.
/// </summary>
public interface IParameterPrecondition : ISummarizable
{
	/// <summary>
	/// Whether or not the passed in value can have all its inner values checked if it's an <see cref="IEnumerable"/>.
	/// </summary>
	bool AllowEnumerating { get; }
	/// <summary>
	/// Whether or not commands invoked from a user not in the guild should be processed.
	/// </summary>
	bool AllowNonGuildInvokers { get; }
	/// <summary>
	/// Whether or not default value passed in to this parameter precondition should be instant success.
	/// </summary>
	bool AllowOptional { get; }
}