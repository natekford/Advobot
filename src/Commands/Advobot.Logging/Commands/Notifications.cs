using Advobot.Logging.Database.Models;
using Advobot.Logging.Resetters;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Localization;

namespace Advobot.Logging.Commands;

[LocalizedCategory(nameof(Notifications))]
[LocalizedCommand(nameof(Groups.Notifications), nameof(Aliases.Notifications))]
public sealed class Notifications : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.ModifyGoodbyeMessage), nameof(Aliases.ModifyGoodbyeMessage))]
	[LocalizedSummary(nameof(Summaries.ModifyGoodbyeMessage))]
	[Id("c59f41ec-5892-496e-beaa-eabceca4bded")]
	[RequireGuildPermissions]
	public sealed class ModifyGoodbyeMessage : NotificationModuleBase
	{
		private const Notification Event = Notification.Goodbye;

		[InjectService]
		public required GoodbyeNotificationResetter DefaultSetter { get; set; }

		[LocalizedCommand(nameof(Groups.Channel), nameof(Aliases.Channel))]
		public async Task<AdvobotResult> Channel(
			[CanModifyChannel(ChannelPermission.ManageChannels)]
			ITextChannel channel)
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, channel.Id).ConfigureAwait(false);
			return Responses.Notifications.ModifiedChannel(Event, channel);
		}

		[LocalizedCommand(nameof(Groups.Content), nameof(Aliases.Content))]
		public async Task<AdvobotResult> Content([Remainder] string? content = null)
		{
			await Db.UpsertNotificationContentAsync(Event, Context.Guild.Id, content).ConfigureAwait(false);
			return Responses.Notifications.ModifiedContent(Event, content);
		}

		[LocalizedCommand(nameof(Groups.Default), nameof(Aliases.Default))]
		public async Task<AdvobotResult> Default()
		{
			await DefaultSetter.ResetAsync(Context).ConfigureAwait(false);
			return Responses.Notifications.Default(Event);
		}

		[LocalizedCommand(nameof(Groups.Disable), nameof(Aliases.Disable))]
		public async Task<AdvobotResult> Disable()
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, null).ConfigureAwait(false);
			return Responses.Notifications.Disabled(Event);
		}

		[LocalizedCommand(nameof(Groups.Embed), nameof(Aliases.Embed))]
		public async Task<AdvobotResult> Embed(CustomEmbed? embed = null)
		{
			await Db.UpsertNotificationEmbedAsync(Event, Context.Guild.Id, embed).ConfigureAwait(false);
			return Responses.Notifications.ModifiedEmbed(Event, embed);
		}

		[LocalizedCommand(nameof(Groups.Send), nameof(Aliases.Send))]
		public async Task<AdvobotResult> Send()
		{
			var notification = await Db.GetAsync(Event, Context.Guild.Id).ConfigureAwait(false);
			return Responses.Notifications.SendNotification(Event, notification);
		}
	}

	[LocalizedCommand(nameof(Groups.ModifyWelcomeMessage), nameof(Aliases.ModifyWelcomeMessage))]
	[LocalizedSummary(nameof(Summaries.ModifyWelcomeMessage))]
	[Id("e95c8444-6a9a-40e7-a287-91e59200d4b6")]
	[RequireGuildPermissions]
	public sealed class ModifyWelcomeMessage : NotificationModuleBase
	{
		private const Notification Event = Notification.Welcome;

		[InjectService]
		public required WelcomeNotificationResetter DefaultSetter { get; set; }

		[LocalizedCommand(nameof(Groups.Channel), nameof(Aliases.Channel))]
		public async Task<AdvobotResult> Channel(
			[CanModifyChannel(ChannelPermission.ManageChannels)]
			ITextChannel channel)
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, channel.Id).ConfigureAwait(false);
			return Responses.Notifications.ModifiedChannel(Event, channel);
		}

		[LocalizedCommand(nameof(Groups.Content), nameof(Aliases.Content))]
		public async Task<AdvobotResult> Content([Remainder] string? content = null)
		{
			await Db.UpsertNotificationContentAsync(Event, Context.Guild.Id, content).ConfigureAwait(false);
			return Responses.Notifications.ModifiedContent(Event, content);
		}

		[LocalizedCommand(nameof(Groups.Default), nameof(Aliases.Default))]
		public async Task<AdvobotResult> Default()
		{
			await DefaultSetter.ResetAsync(Context).ConfigureAwait(false);
			return Responses.Notifications.Default(Event);
		}

		[LocalizedCommand(nameof(Groups.Disable), nameof(Aliases.Disable))]
		public async Task<AdvobotResult> Disable()
		{
			await Db.UpsertNotificationChannelAsync(Event, Context.Guild.Id, null).ConfigureAwait(false);
			return Responses.Notifications.Disabled(Event);
		}

		[LocalizedCommand(nameof(Groups.Embed), nameof(Aliases.Embed))]
		public async Task<AdvobotResult> Embed(CustomEmbed? embed = null)
		{
			await Db.UpsertNotificationEmbedAsync(Event, Context.Guild.Id, embed).ConfigureAwait(false);
			return Responses.Notifications.ModifiedEmbed(Event, embed);
		}

		[LocalizedCommand(nameof(Groups.Send), nameof(Aliases.Send))]
		public async Task<AdvobotResult> Send()
		{
			var notification = await Db.GetAsync(Event, Context.Guild.Id).ConfigureAwait(false);
			return Responses.Notifications.SendNotification(Event, notification);
		}
	}
}