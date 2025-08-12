using Advobot.Logging.Database.Models;

using Discord;

namespace Advobot.Logging.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="ITextChannel"/> is not the current server log channel.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotServerLog : LogParameterPrecondition
{
	/// <inheritdoc />
	protected override string LogName => Resources.Responses.VariableServerLog;

	/// <inheritdoc />
	protected override ulong GetId(LogChannels channels)
		=> channels.ServerLogId;
}