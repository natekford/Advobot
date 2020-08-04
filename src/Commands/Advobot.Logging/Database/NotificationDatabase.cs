using System;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Logging.Database
{
	public sealed class NotificationDatabase : DatabaseBase<SQLiteConnection>
	{
		public NotificationDatabase(INotificationDatabaseStarter starter) : base(starter)
		{
		}

		public async Task<IReadOnlyCustomNotification?> GetAsync(
			Notification notification,
			ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new
			{
				GuildId = guildId.ToString(),
				Event = GetNotificationName(notification),
			};
			return await connection.QuerySingleOrDefaultAsync<CustomNotification>(@"
				SELECT *
				FROM Notification
				WHERE GuildId = @GuildId AND Event = @Event
			", param).CAF();
		}

		public async Task UpdateNotificationChannelAsync(
			Notification notification,
			ulong guildId,
			ulong? channelId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new
			{
				GuildId = guildId.ToString(),
				Event = GetNotificationName(notification),
				ChannelId = channelId?.ToString()
			};
			await connection.ExecuteAsync(@"
				INSERT OR IGNORE INTO Notification
					( GuildId, Event )
					VALUES
					( @GuildId, @Event );
				UPDATE Notification
				SET ChannelId = @ChannelId
				WHERE GuildId = @GuildId AND Event = @Event
			", param).CAF();
		}

		public async Task UpdateNotificationContentAsync(
			Notification notification,
			ulong guildId,
			string? content)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new
			{
				GuildId = guildId.ToString(),
				Event = GetNotificationName(notification),
				Content = content
			};
			await connection.ExecuteAsync(@"
				INSERT OR IGNORE INTO Notification
					( GuildId, Event )
					VALUES
					( @GuildId, @Event );
				UPDATE Notification
				SET Content = @Content
				WHERE GuildId = @GuildId AND Event = @Event
			", param).CAF();
		}

		public async Task UpdateNotificationEmbedAsync(
			Notification notification,
			ulong guildId,
			IReadOnlyCustomEmbed? embed)
		{
			using var connection = await GetConnectionAsync().CAF();

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
			await connection.ExecuteAsync(@"
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
			", param).CAF();
		}

		private string GetNotificationName(Notification notification) => notification switch
		{
			Notification.Goodbye => "G",
			Notification.Welcome => "W",
			_ => throw new ArgumentOutOfRangeException(nameof(notification)),
		};
	}
}