using Advobot.Logging.Models;

namespace Advobot.Logging.Preconditions;

/// <summary>
/// Requires a mod log to be set.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireModLog : LogPrecondition
{
	/// <inheritdoc />
	protected override string LogName => Resources.Responses.VariableModLog;

	/// <inheritdoc />
	protected override ulong GetId(LogChannels channels)
		=> channels.ModLogId;
}