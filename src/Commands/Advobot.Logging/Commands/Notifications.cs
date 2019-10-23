using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Logging.Localization;
using Advobot.Logging.Models;
using Advobot.Logging.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Logging.Commands
{
	[Category(nameof(Notifications))]
	[LocalizedGroup(nameof(Groups.Notifications))]
	[LocalizedAlias(nameof(Aliases.Notifications))]
	public sealed class Notifications : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.ModifyGoodbyeMessage))]
		[LocalizedAlias(nameof(Aliases.ModifyGoodbyeMessage))]
		[LocalizedSummary(nameof(Summaries.ModifyGoodbyeMessage))]
		[Meta("c59f41ec-5892-496e-beaa-eabceca4bded")]
		[RequireGuildPermissions]
		public sealed class ModifyGoodbyeMessage : NotificationModuleBase
		{
			private const Notification Event = Notification.Goodbye;

			[LocalizedCommand(nameof(Groups.Channel))]
			[LocalizedAlias(nameof(Aliases.Channel))]
			public async Task<RuntimeResult> Channel(
				[CanModifyChannel(ManageChannels | ManageRoles)]
				ITextChannel channel)
			{
				await Notifications.SetChannelAsync(Event, Context.Guild.Id, channel.Id).CAF();
				return Responses.Notifications.ModifiedChannel(Event, channel);
			}

			[LocalizedCommand(nameof(Groups.Content))]
			[LocalizedAlias(nameof(Aliases.Content))]
			public async Task<RuntimeResult> Content([Remainder] string? content = null)
			{
				await Notifications.SetContentAsync(Event, Context.Guild.Id, content).CAF();
				return Responses.Notifications.ModifiedContent(Event, content);
			}

			[LocalizedCommand(nameof(Groups.Disable))]
			[LocalizedAlias(nameof(Aliases.Disable))]
			public async Task<RuntimeResult> Disable()
			{
				await Notifications.DisableAsync(Event, Context.Guild.Id).CAF();
				return Responses.Notifications.Disabled(Event);
			}

			[LocalizedCommand(nameof(Groups.Embed))]
			[LocalizedAlias(nameof(Aliases.Embed))]
			public async Task<RuntimeResult> Embed(CustomEmbed? embed = null)
			{
				await Notifications.SetEmbedAsync(Event, Context.Guild.Id, embed).CAF();
				return Responses.Notifications.ModifiedEmbed(Event, embed);
			}

			[LocalizedCommand(nameof(Groups.Send))]
			[LocalizedAlias(nameof(Aliases.Send))]
			public async Task<RuntimeResult> Send()
			{
				var notification = await Notifications.GetAsync(Event, Context.Guild.Id).CAF();
				return Responses.Notifications.SendNotification(Event, notification);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyWelcomeMessage))]
		[LocalizedAlias(nameof(Aliases.ModifyWelcomeMessage))]
		[LocalizedSummary(nameof(Summaries.ModifyWelcomeMessage))]
		[Meta("e95c8444-6a9a-40e7-a287-91e59200d4b6")]
		[RequireGuildPermissions]
		public sealed class ModifyWelcomeMessage : NotificationModuleBase
		{
			private const Notification Event = Notification.Welcome;

			[LocalizedCommand(nameof(Groups.Channel))]
			[LocalizedAlias(nameof(Aliases.Channel))]
			public async Task<RuntimeResult> Channel(
				[CanModifyChannel(ManageChannels | ManageRoles)]
				ITextChannel channel)
			{
				await Notifications.SetChannelAsync(Event, Context.Guild.Id, channel.Id).CAF();
				return Responses.Notifications.ModifiedChannel(Event, channel);
			}

			[LocalizedCommand(nameof(Groups.Content))]
			[LocalizedAlias(nameof(Aliases.Content))]
			public async Task<RuntimeResult> Content([Remainder] string? content = null)
			{
				await Notifications.SetContentAsync(Event, Context.Guild.Id, content).CAF();
				return Responses.Notifications.ModifiedContent(Event, content);
			}

			[LocalizedCommand(nameof(Groups.Disable))]
			[LocalizedAlias(nameof(Aliases.Disable))]
			public async Task<RuntimeResult> Disable()
			{
				await Notifications.DisableAsync(Event, Context.Guild.Id).CAF();
				return Responses.Notifications.Disabled(Event);
			}

			[LocalizedCommand(nameof(Groups.Embed))]
			[LocalizedAlias(nameof(Aliases.Embed))]
			public async Task<RuntimeResult> Embed(CustomEmbed? embed = null)
			{
				await Notifications.SetEmbedAsync(Event, Context.Guild.Id, embed).CAF();
				return Responses.Notifications.ModifiedEmbed(Event, embed);
			}

			[LocalizedCommand(nameof(Groups.Send))]
			[LocalizedAlias(nameof(Aliases.Send))]
			public async Task<RuntimeResult> Send()
			{
				var notification = await Notifications.GetAsync(Event, Context.Guild.Id).CAF();
				return Responses.Notifications.SendNotification(Event, notification);
			}
		}
	}
}