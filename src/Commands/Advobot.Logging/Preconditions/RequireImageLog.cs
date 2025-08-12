using Advobot.Logging.Database.Models;

namespace Advobot.Logging.Preconditions;

/// <summary>
/// Requires an image log to be set.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireImageLog : LogPrecondition
{
	/// <inheritdoc />
	protected override string LogName => Resources.Responses.VariableImageLog;

	/// <inheritdoc />
	protected override ulong GetId(LogChannels channels)
		=> channels.ImageLogId;
}