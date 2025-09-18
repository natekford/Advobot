using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Modules;
using Advobot.Preconditions;
using Advobot.Utilities;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

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

	public override async ValueTask<IResult> CheckAsync(
		IImmutableCommand command,
		IGuildContext context)
	{
		var db = GetDatabase(context.Services);
		var channels = await db.GetLogChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		if (GetId(channels) == 0)
		{
			return Result.Failure(LogPreconditionError.Format(
				LogName.WithNoMarkdown()
			));
		}
		return Result.EmptySuccess;
	}

	/// <summary>
	/// Gets the current id of the log channel.
	/// </summary>
	/// <param name="channels"></param>
	/// <returns></returns>
	protected abstract ulong GetId(LogChannels channels);

	[GetServiceMethod]
	private static LoggingDatabase GetDatabase(IServiceProvider services)
		=> services.GetRequiredService<LoggingDatabase>();
}