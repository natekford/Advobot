using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.ParameterPreconditions.Logs
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
			=> $"Not the current {LogName} log";
		/// <summary>
		/// Gets the name of the log.
		/// </summary>
		protected abstract string LogName { get; }

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

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			if (GetId(settings) != channel.Id)
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError($"`{channel.Format()}` is already the current {LogName} log.");
		}
		/// <summary>
		/// Gets the current id of this log.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract ulong GetId(IGuildSettings settings);
	}
}
