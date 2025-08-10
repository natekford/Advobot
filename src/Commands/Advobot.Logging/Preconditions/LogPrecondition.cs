using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Preconditions;
using Advobot.Utilities;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.Preconditions;

/// <summary>
/// Requires a log channel to be set.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public abstract class LogPrecondition : AdvobotPrecondition
{
	/// <inheritdoc />
	public override string Summary
		=> LogPreconditionSummary.Format(LogName.WithNoMarkdown());

	/// <summary>
	/// Gets the name of the log.
	/// </summary>
	protected abstract string LogName { get; }

	/// <inheritdoc />
	public override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		CommandInfo command,
		IServiceProvider services)
	{
		var service = services.GetRequiredService<ILoggingDatabase>();
		var channels = await service.GetLogChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		if (GetId(channels) != 0)
		{
			return this.FromSuccess();
		}
		return PreconditionResult.FromError(LogPreconditionError.Format(LogName.WithNoMarkdown()));
	}

	/// <summary>
	/// Gets the current id of the log channel.
	/// </summary>
	/// <param name="channels"></param>
	/// <returns></returns>
	protected abstract ulong GetId(LogChannels channels);
}