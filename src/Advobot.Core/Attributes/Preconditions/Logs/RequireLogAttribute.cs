using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions.Logs
{
	/// <summary>
	/// Requires a log channel to be set.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class RequireLogAttribute : PreconditionAttribute
	{
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
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			if (GetId(settings) != 0)
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError($"There is no {LogName} log to remove.");
		}
		/// <summary>
		/// Gets the current id of the log channel.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract ulong GetId(IGuildSettings settings);
		/// <inheritdoc />
		public override string ToString()
			=> $"{LogName} log must exist";
	}
}
