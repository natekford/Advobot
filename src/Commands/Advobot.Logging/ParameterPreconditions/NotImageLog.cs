using Advobot.Logging.Models;

using Discord;

namespace Advobot.Logging.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="ITextChannel"/> is not the current image log channel.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class NotImageLog : LogParameterPrecondition
{
	/// <inheritdoc />
	protected override string LogName => Resources.Responses.VariableImageLog;

	/// <inheritdoc />
	protected override ulong GetId(LogChannels channels)
		=> channels.ImageLogId;
}