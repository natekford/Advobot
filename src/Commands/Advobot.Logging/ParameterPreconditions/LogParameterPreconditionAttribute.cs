using System;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Logging.ReadOnlyModels;
using Advobot.Logging.Service;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using static Advobot.Logging.Resources.Responses;

namespace Advobot.Logging.ParameterPreconditions
{
	/// <summary>
	/// Makes sure the passed in <see cref="ITextChannel"/> is not the current log channel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public abstract class LogParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{
		/// <inheritdoc />
		public override string Summary
			=> LogParameterPreconditionSummary.Format(LogName.WithNoMarkdown());

		/// <summary>
		/// Gets the name of the log.
		/// </summary>
		protected abstract string LogName { get; }

		/// <summary>
		/// Gets the current id of this log.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract ulong GetId(IReadOnlyLogChannels channels);

		/// <inheritdoc />
		protected override async Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (!(value is ITextChannel channel))
			{
				return this.FromOnlySupports(typeof(ITextChannel));
			}

			var service = services.GetRequiredService<ILoggingService>();
			var channels = await service.GetLogChannelsAsync(context.Guild.Id).CAF();
			if (GetId(channels) != channel.Id)
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError(LogParameterPreconditionSummary.Format(
				channel.Format().WithBlock(),
				LogName.WithNoMarkdown()
			));
		}
	}
}