using System;
using System.Threading.Tasks;

using Advobot.Logging.ReadOnlyModels;
using Advobot.Logging.Service;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Logging.Resources.Responses;

namespace Advobot.Logging.Preconditions
{
	/// <summary>
	/// Requires a log channel to be set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class LogPreconditionAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
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
			var service = services.GetRequiredService<ILoggingService>();
			var channels = await service.GetLogChannelsAsync(context.Guild.Id).CAF();
			if (GetId(channels) != 0)
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError(LogPreconditionError.Format(LogName.WithNoMarkdown()));
		}

		/// <summary>
		/// Gets the current id of the log channel.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract ulong GetId(IReadOnlyLogChannels channels);
	}
}