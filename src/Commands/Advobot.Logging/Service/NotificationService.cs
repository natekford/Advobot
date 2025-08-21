using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Utilities;
using Advobot.Services;
using Advobot.Services.Events;
using Advobot.Utilities;

using Discord;

using Microsoft.Extensions.Logging;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.Service;

public sealed class NotificationService(
	ILogger<NotificationService> logger,
	NotificationDatabase db,
	EventProvider eventProvider,
	MessageQueue queue
) : StartableService
{
	protected override Task StartAsyncImpl()
	{
		eventProvider.UserJoined.Add(OnUserJoined);
		eventProvider.UserLeft.Add(OnUserLeft);

		return Task.CompletedTask;
	}

	protected override Task StopAsyncImpl()
	{
		eventProvider.UserJoined.Remove(OnUserJoined);
		eventProvider.UserLeft.Remove(OnUserLeft);

		return base.StopAsyncImpl();
	}

	private async Task OnEvent(Notification notifType, IGuild guild, IUser user)
	{
		var notification = await db.GetAsync(notifType, guild.Id).ConfigureAwait(false);
		if (notification is null || notification.GuildId == 0)
		{
			return;
		}

		var channel = await guild.GetTextChannelAsync(notification.ChannelId).ConfigureAwait(false);
		if (channel is null)
		{
			return;
		}

		logger.LogInformation(
			message: "Sending event of type {Event} to {@Info}.",
			args: [notifType, new
			{
				Guild = guild.Id,
				Channel = channel.Id,
				User = user.Id,
			}]
		);

		var content = notification.Content
			?.CaseInsReplace(NotificationUtils.USER_MENTION, user?.Mention ?? VariableInvalidUser)
			?.CaseInsReplace(NotificationUtils.USER_STRING, user?.Format() ?? VariableInvalidUser);
		var embedWrapper = notification.EmbedEmpty() ? null : notification.BuildWrapper();

		queue.EnqueueSend(channel, new(embedWrapper)
		{
			Content = content,
			AllowedMentions = NotificationUtils.UserMentions,
		});
	}

	private Task OnUserJoined(IGuildUser user)
		=> OnEvent(Notification.Welcome, user.Guild, user);

	private Task OnUserLeft(IGuild guild, IUser user)
		=> OnEvent(Notification.Goodbye, guild, user);
}