using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Resetters;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;
using Discord.Commands;

using static Advobot.Logging.Responses.Notifications;
using static Discord.ChannelPermission;

namespace Advobot.Logging.Commands;

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
		public required GoodbyeNotificationResetter DefaultSetter { get; set; }

		[LocalizedCommand(nameof(Groups.Channel))]
		[LocalizedAlias(nameof(Aliases.Channel))]
		public async Task<RuntimeResult> Channel(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			ITextChannel channel)
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, channel.Id).ConfigureAwait(false);
			return ModifiedChannel(Event, channel);
		}

		[LocalizedCommand(nameof(Groups.Content))]
		[LocalizedAlias(nameof(Aliases.Content))]
		public async Task<RuntimeResult> Content([Remainder] string? content = null)
		{
			await Db.UpsertNotificationContentAsync(Event, Context.Guild.Id, content).ConfigureAwait(false);
			return ModifiedContent(Event, content);
		}

		[LocalizedCommand(nameof(Groups.Default))]
		[LocalizedAlias(nameof(Aliases.Default))]
		public async Task<RuntimeResult> Default()
		{
			await DefaultSetter.ResetAsync(Context).ConfigureAwait(false);
			return Responses.Notifications.Default(Event);
		}

		[LocalizedCommand(nameof(Groups.Disable))]
		[LocalizedAlias(nameof(Aliases.Disable))]
		public async Task<RuntimeResult> Disable()
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, null).ConfigureAwait(false);
			return Disabled(Event);
		}

		[LocalizedCommand(nameof(Groups.Embed))]
		[LocalizedAlias(nameof(Aliases.Embed))]
		public async Task<RuntimeResult> Embed(CustomEmbed? embed = null)
		{
			await Db.UpsertNotificationEmbedAsync(Event, Context.Guild.Id, embed).ConfigureAwait(false);
			return ModifiedEmbed(Event, embed);
		}

		[LocalizedCommand(nameof(Groups.Send))]
		[LocalizedAlias(nameof(Aliases.Send))]
		public async Task<RuntimeResult> Send()
		{
			var notification = await Db.GetAsync(Event, Context.Guild.Id).ConfigureAwait(false);
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
		public required WelcomeNotificationResetter DefaultSetter { get; set; }

		[LocalizedCommand(nameof(Groups.Channel))]
		[LocalizedAlias(nameof(Aliases.Channel))]
		public async Task<RuntimeResult> Channel(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			ITextChannel channel)
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, channel.Id).ConfigureAwait(false);
			return ModifiedChannel(Event, channel);
		}

		[LocalizedCommand(nameof(Groups.Content))]
		[LocalizedAlias(nameof(Aliases.Content))]
		public async Task<RuntimeResult> Content([Remainder] string? content = null)
		{
			await Db.UpsertNotificationContentAsync(Event, Context.Guild.Id, content).ConfigureAwait(false);
			return ModifiedContent(Event, content);
		}

		[LocalizedCommand(nameof(Groups.Default))]
		[LocalizedAlias(nameof(Aliases.Default))]
		public async Task<RuntimeResult> Default()
		{
			await DefaultSetter.ResetAsync(Context).ConfigureAwait(false);
			return Responses.Notifications.Default(Event);
		}

		[LocalizedCommand(nameof(Groups.Disable))]
		[LocalizedAlias(nameof(Aliases.Disable))]
		public async Task<RuntimeResult> Disable()
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, null).ConfigureAwait(false);
			return Disabled(Event);
		}

		[LocalizedCommand(nameof(Groups.Embed))]
		[LocalizedAlias(nameof(Aliases.Embed))]
		public async Task<RuntimeResult> Embed(CustomEmbed? embed = null)
		{
			await Db.UpsertNotificationEmbedAsync(Event, Context.Guild.Id, embed).ConfigureAwait(false);
			return ModifiedEmbed(Event, embed);
		}

		[LocalizedCommand(nameof(Groups.Send))]
		[LocalizedAlias(nameof(Aliases.Send))]
		public async Task<RuntimeResult> Send()
		{
			var notification = await Db.GetAsync(Event, Context.Guild.Id).ConfigureAwait(false);
			return SendNotification(Event, notification);
		}
	}
}