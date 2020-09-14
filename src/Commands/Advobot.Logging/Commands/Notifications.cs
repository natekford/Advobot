using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Localization;
using Advobot.Logging.Models;
using Advobot.Logging.OptionSetters;
using Advobot.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Advobot.Logging.Responses.Notifications;
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
			public GoodbyeNotificationResetter DefaultSetter { get; set; } = null!;

			[LocalizedCommand(nameof(Groups.Channel))]
			[LocalizedAlias(nameof(Aliases.Channel))]
			public async Task<RuntimeResult> Channel(
				[CanModifyChannel(ManageChannels | ManageRoles)]
				ITextChannel channel)
			{
				await Notifications.SetChannelAsync(Event, Context.Guild.Id, channel.Id).CAF();
				return ModifiedChannel(Event, channel);
			}

			[LocalizedCommand(nameof(Groups.Content))]
			[LocalizedAlias(nameof(Aliases.Content))]
			public async Task<RuntimeResult> Content([Remainder] string? content = null)
			{
				await Notifications.SetContentAsync(Event, Context.Guild.Id, content).CAF();
				return ModifiedContent(Event, content);
			}

			[LocalizedCommand(nameof(Groups.Default))]
			[LocalizedAlias(nameof(Aliases.Default))]
			public async Task<RuntimeResult> Default()
			{
				await DefaultSetter.ResetAsync(Context).CAF();
				return Responses.Notifications.Default(Event);
			}

			[LocalizedCommand(nameof(Groups.Disable))]
			[LocalizedAlias(nameof(Aliases.Disable))]
			public async Task<RuntimeResult> Disable()
			{
				await Notifications.DisableAsync(Event, Context.Guild.Id).CAF();
				return Disabled(Event);
			}

			[LocalizedCommand(nameof(Groups.Embed))]
			[LocalizedAlias(nameof(Aliases.Embed))]
			public async Task<RuntimeResult> Embed(CustomEmbed? embed = null)
			{
				await Notifications.SetEmbedAsync(Event, Context.Guild.Id, embed).CAF();
				return ModifiedEmbed(Event, embed);
			}

			[LocalizedCommand(nameof(Groups.Send))]
			[LocalizedAlias(nameof(Aliases.Send))]
			public async Task<RuntimeResult> Send()
			{
				var notification = await Notifications.GetAsync(Event, Context.Guild.Id).CAF();
				return SendNotification(Event, notification);
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
			public WelcomeNotificationResetter DefaultSetter { get; set; } = null!;

			[LocalizedCommand(nameof(Groups.Channel))]
			[LocalizedAlias(nameof(Aliases.Channel))]
			public async Task<RuntimeResult> Channel(
				[CanModifyChannel(ManageChannels | ManageRoles)]
				ITextChannel channel)
			{
				await Notifications.SetChannelAsync(Event, Context.Guild.Id, channel.Id).CAF();
				return ModifiedChannel(Event, channel);
			}

			[LocalizedCommand(nameof(Groups.Content))]
			[LocalizedAlias(nameof(Aliases.Content))]
			public async Task<RuntimeResult> Content([Remainder] string? content = null)
			{
				await Notifications.SetContentAsync(Event, Context.Guild.Id, content).CAF();
				return ModifiedContent(Event, content);
			}

			[LocalizedCommand(nameof(Groups.Default))]
			[LocalizedAlias(nameof(Aliases.Default))]
			public async Task<RuntimeResult> Default()
			{
				await DefaultSetter.ResetAsync(Context).CAF();
				return Responses.Notifications.Default(Event);
			}

			[LocalizedCommand(nameof(Groups.Disable))]
			[LocalizedAlias(nameof(Aliases.Disable))]
			public async Task<RuntimeResult> Disable()
			{
				await Notifications.DisableAsync(Event, Context.Guild.Id).CAF();
				return Disabled(Event);
			}

			[LocalizedCommand(nameof(Groups.Embed))]
			[LocalizedAlias(nameof(Aliases.Embed))]
			public async Task<RuntimeResult> Embed(CustomEmbed? embed = null)
			{
				await Notifications.SetEmbedAsync(Event, Context.Guild.Id, embed).CAF();
				return ModifiedEmbed(Event, embed);
			}

			[LocalizedCommand(nameof(Groups.Send))]
			[LocalizedAlias(nameof(Aliases.Send))]
			public async Task<RuntimeResult> Send()
			{
				var notification = await Notifications.GetAsync(Event, Context.Guild.Id).CAF();
				return SendNotification(Event, notification);
			}
		}
	}
}