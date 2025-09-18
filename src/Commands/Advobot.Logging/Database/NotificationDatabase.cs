using Advobot.Logging.Database.Models;
using Advobot.SQLite;

using System.Data.SQLite;

namespace Advobot.Logging.Database;

public sealed class NotificationDatabase(IConnectionString<NotificationDatabase> connection)
	: DatabaseBase<SQLiteConnection>(connection)
{
	public async Task<CustomNotification?> GetAsync(
		Notification notification,
		ulong guildId)
	{
		var param = new
		{
			GuildId = guildId.ToString(),
			Event = GetNotificationName(notification),
		};
		return await GetOneAsync<CustomNotification>(@"
			SELECT *
			FROM Notification
			WHERE GuildId = @GuildId AND Event = @Event
		", param).ConfigureAwait(false);
	}

	public Task<int> UpsertNotificationChannelAsync(
		Notification notification,
		ulong guildId,
		ulong? channelId)
	{
		var param = new
		{
			GuildId = guildId.ToString(),
			Event = GetNotificationName(notification),
			ChannelId = channelId?.ToString()
		};
		return ModifyAsync(@"
			INSERT OR IGNORE INTO Notification
				( GuildId, Event )
				VALUES
				( @GuildId, @Event );
			UPDATE Notification
			SET ChannelId = @ChannelId
			WHERE GuildId = @GuildId AND Event = @Event
		", param);
	}

	public Task<int> UpsertNotificationContentAsync(
		Notification notification,
		ulong guildId,
		string? content)
	{
		var param = new
		{
			GuildId = guildId.ToString(),
			Event = GetNotificationName(notification),
			Content = content
		};
		return ModifyAsync(@"
			INSERT OR IGNORE INTO Notification
				( GuildId, Event )
				VALUES
				( @GuildId, @Event );
			UPDATE Notification
			SET Content = @Content
			WHERE GuildId = @GuildId AND Event = @Event
		", param);
	}

	public Task<int> UpsertNotificationEmbedAsync(
		Notification notification,
		ulong guildId,
		CustomEmbed? embed)
	{
		var param = new
		{
			GuildId = guildId.ToString(),
			Event = GetNotificationName(notification),
			AuthorIconUrl = embed?.AuthorIconUrl?.ToString(),
			AuthorName = embed?.AuthorName,
			AuthorUrl = embed?.AuthorUrl?.ToString(),
			Color = embed?.Color ?? 0,
			Description = embed?.Description,
			Footer = embed?.Footer,
			FooterIconUrl = embed?.FooterIconUrl?.ToString(),
			ImageUrl = embed?.ImageUrl?.ToString(),
			ThumbnailUrl = embed?.ThumbnailUrl?.ToString(),
			Title = embed?.Title,
			Url = embed?.Url?.ToString(),
		};
		return ModifyAsync(@"
			INSERT OR IGNORE INTO Notification
				( GuildId, Event )
				VALUES
				( @GuildId, @Event );
			UPDATE Notification
			SET
				AuthorIconUrl = @AuthorIconUrl,
				AuthorName = @AuthorName,
				AuthorUrl = @AuthorUrl,
				Color = @Color,
				Description = @Description,
				Footer = @Footer,
				FooterIconUrl = @FooterIconUrl,
				ImageUrl = @ImageUrl,
				ThumbnailUrl = @ThumbnailUrl,
				Title = @Title,
				Url = @Url
			WHERE GuildId = @GuildId AND Event = @Event
		", param);
	}

	private string GetNotificationName(Notification notification) => notification switch
	{
		Notification.Goodbye => "G",
		Notification.Welcome => "W",
		_ => throw new ArgumentOutOfRangeException(nameof(notification)),
	};
}