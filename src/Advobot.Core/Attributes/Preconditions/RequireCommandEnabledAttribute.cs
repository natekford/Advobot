using System;
using System.Threading.Tasks;

using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
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
			foreach (var checker in services.GetServices<ICommandChecker>())
			{
				var result = await checker.CanInvokeAsync(context, command).CAF();
				if (!result.IsSuccess)
				{
					return result;
				}
			}
			return PreconditionResult.FromSuccess();

			if (!(context.User is IGuildUser user))
			{
				return PreconditionUtils.FromInvalidInvoker();
			}

			var settingsFactory = services.GetRequiredService<IGuildSettingsFactory>();
			var settings = await settingsFactory.GetOrCreateAsync(context.Guild).CAF();
			var meta = command.Module.Attributes.GetAttribute<MetaAttribute>();
			if (settings.CommandSettings.CanUserInvokeCommand(user, context.Channel, meta))
			{
				return PreconditionResult.FromSuccess();
			}
			return PreconditionResult.FromError("This command is disabled.");
		}
	}
}