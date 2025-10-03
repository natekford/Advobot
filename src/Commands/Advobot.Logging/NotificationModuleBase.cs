using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Resetters;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;

namespace Advobot.Logging;

public abstract class NotificationModuleBase(Notification notification)
	: AdvobotModuleBase
{
	[InjectService]
	public required NotificationDatabase Db { get; set; }
	public abstract NotificationResetter DefaultSetter { get; }

	[Command(nameof(Names.Channel), nameof(Names.ChannelAlias), AllowInheritance = true)]
	public async Task<AdvobotResult> Channel(
		[CanModifyChannel(ChannelPermission.ManageChannels)]
		ITextChannel channel)
	{
		await Db.UpsertNotificationChannelAsync(notification, Context.Guild.Id, channel.Id).ConfigureAwait(false);
		return Responses.Notifications.ModifiedChannel(notification, channel);
	}

	[Command(nameof(Names.Content), nameof(Names.ContentAlias), AllowInheritance = true)]
	public async Task<AdvobotResult> Content([Remainder] string? content = null)
	{
		await Db.UpsertNotificationContentAsync(notification, Context.Guild.Id, content).ConfigureAwait(false);
		return Responses.Notifications.ModifiedContent(notification, content);
	}

	[Command(nameof(Names.Default), nameof(Names.DefaultAlias), AllowInheritance = true)]
	public async Task<AdvobotResult> Default()
	{
		await DefaultSetter.ResetAsync(Context).ConfigureAwait(false);
		return Responses.Notifications.Default(notification);
	}

	[Command(nameof(Names.Disable), nameof(Names.DisableAlias), AllowInheritance = true)]
	public async Task<AdvobotResult> Disable()
	{
		await Db.UpsertNotificationChannelAsync(notification, Context.Guild.Id, null).ConfigureAwait(false);
		return Responses.Notifications.Disabled(notification);
	}

	[Command(nameof(Names.Embed), nameof(Names.EmbedAlias), AllowInheritance = true)]
	public async Task<AdvobotResult> Embed(CustomEmbed? embed = null)
	{
		await Db.UpsertNotificationEmbedAsync(notification, Context.Guild.Id, embed).ConfigureAwait(false);
		return Responses.Notifications.ModifiedEmbed(notification, embed);
	}

	[Command(nameof(Names.Send), nameof(Names.SendAlias), AllowInheritance = true)]
	public async Task<AdvobotResult> Send()
	{
		var customNotif = await Db.GetAsync(notification, Context.Guild.Id).ConfigureAwait(false);
		return Responses.Notifications.SendNotification(notification, customNotif);
	}
}