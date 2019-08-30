using System;
using System.Threading.Tasks;

using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Checks to make sure the bot is loaded, the guild is loaded, the channel isn't ignored from commands, and the command is enabled for the user.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireCommandEnabledAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Command is turned on";

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var guildUser = await context.Guild.GetUserAsync(context.User.Id).CAF();
			if (settings.CommandSettings.IsCommandEnabled(guildUser, context.Channel, command))
			{
				return PreconditionUtils.FromSuccess();
			}
			return PreconditionUtils.FromError("This command is disabled.");
		}
	}
}