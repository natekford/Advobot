using Advobot.Classes;
using Advobot.Logging.Database;
using Advobot.Logging.Utilities;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.WebSocket;

using static Advobot.Resources.Responses;

namespace Advobot.Logging.Service;

public sealed class NotificationService
{
	private readonly INotificationDatabase _Db;
	private readonly MessageSenderQueue _MessageQueue;

	public NotificationService(
		INotificationDatabase db,
		BaseSocketClient client,
		MessageSenderQueue queue)
	{
		_Db = db;
		_MessageQueue = queue;

		client.UserJoined += OnUserJoined;
		client.UserLeft += OnUserLeft;
	}

	private async Task OnEvent(Notification notifType, SocketGuild guild, SocketUser user)
	{
		var notification = await _Db.GetAsync(notifType, guild.Id).CAF();
		if (notification is null || notification.GuildId == 0)
		{
			return;
		}

		var channel = guild.GetTextChannel(notification.ChannelId);
		if (channel is null)
		{
			return;
		}

		var content = notification.Content
			?.CaseInsReplace(NotificationUtils.USER_MENTION, user?.Mention ?? VariableInvalidUser)
			?.CaseInsReplace(NotificationUtils.USER_STRING, user?.Format() ?? VariableInvalidUser);
		var embedWrapper = notification.EmbedEmpty() ? null : notification.BuildWrapper();

		_MessageQueue.Enqueue((channel, new(embedWrapper)
		{
			Content = content,
			AllowedMentions = NotificationUtils.UserMentions,
		}));
	}

	private Task OnUserJoined(SocketGuildUser user)
		=> OnEvent(Notification.Welcome, user.Guild, user);

	private Task OnUserLeft(SocketGuild guild, SocketUser user)
		=> OnEvent(Notification.Goodbye, guild, user);
}