using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions;
using Advobot.Utilities;

using Discord;

using Microsoft.Extensions.DependencyInjection;

using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.ParameterPreconditions;

/// <summary>
/// Makes sure the passed in <see cref="ITextChannel"/> is not the current log channel.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public abstract class LogParameterPrecondition : AdvobotParameterPrecondition<ITextChannel>
{
	/// <inheritdoc />
	public override string Summary
		=> LogParameterPreconditionSummary.Format(LogName.WithNoMarkdown());
	/// <summary>
	/// Gets the name of the log.
	/// </summary>
	protected abstract string LogName { get; }

	public override async ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IGuildContext context,
		ITextChannel? value)
	{
		var db = GetDatabase(context.Services);
		var channels = await db.GetLogChannelsAsync(context.Guild.Id).ConfigureAwait(false);
		if (GetId(channels) == value.Id)
		{
			return Result.Failure(LogParameterPreconditionSummary.Format(
				value.Format().WithBlock(),
				LogName.WithNoMarkdown()
			));
		}
		return CachedResults.Success;
	}

	/// <summary>
	/// Gets the current id of this log.
	/// </summary>
	/// <param name="channels"></param>
	/// <returns></returns>
	protected abstract ulong GetId(LogChannels channels);

	[GetServiceMethod]
	private static LoggingDatabase GetDatabase(IServiceProvider services)
		=> services.GetRequiredService<LoggingDatabase>();
}