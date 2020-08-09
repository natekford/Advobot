using System;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.Logging.Database
{
	public sealed class NotificationDatabase : DatabaseBase<SQLiteConnection>
	{
		public NotificationDatabase(IConnectionFor<NotificationDatabase> conn) : base(conn)
		{
		}

		public async Task<IReadOnlyCustomNotification?> GetAsync(
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
			", param).CAF();
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
			IReadOnlyCustomEmbed? embed)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				Event = GetNotificationName(notification),
				embed?.AuthorIconUrl,
				embed?.AuthorName,
				embed?.AuthorUrl,
				Color = embed?.Color ?? 0,
				embed?.Description,
				embed?.Footer,
				embed?.FooterIconUrl,
				embed?.ImageUrl,
				embed?.ThumbnailUrl,
				embed?.Title,
				embed?.Url,
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
}