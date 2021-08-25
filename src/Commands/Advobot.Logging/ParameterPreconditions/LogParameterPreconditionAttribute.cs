
using Advobot.GeneratedParameterPreconditions;
using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.ParameterPreconditions
{
	/// <summary>
	/// Makes sure the passed in <see cref="ITextChannel"/> is not the current log channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class LogParameterPreconditionAttribute
		: ITextChannelParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary
			=> LogParameterPreconditionSummary.Format(LogName.WithNoMarkdown());
		/// <summary>
		/// Gets the name of the log.
		/// </summary>
		protected abstract string LogName { get; }

		/// <inheritdoc />
		protected override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			ITextChannel value,
			IServiceProvider services)
		{
			var service = services.GetRequiredService<ILoggingDatabase>();
			var channels = await service.GetLogChannelsAsync(context.Guild.Id).CAF();
			if (GetId(channels) != value.Id)
			{
				return this.FromSuccess();
			}
			return PreconditionResult.FromError(LogParameterPreconditionSummary.Format(
				value.Format().WithBlock(),
				LogName.WithNoMarkdown()
			));
		}

		/// <summary>
		/// Gets the current id of this log.
		/// </summary>
		/// <param name="channels"></param>
		/// <returns></returns>
		protected abstract ulong GetId(LogChannels channels);
	}
}