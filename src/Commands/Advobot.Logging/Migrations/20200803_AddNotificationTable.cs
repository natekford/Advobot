using FluentMigrator;

namespace Advobot.Logging.Migrations
{
	[Migration(20200803193000)]
	public sealed class AddNotificationTable : Migration
	{
		public override void Down()
			=> Delete.Table("Notification");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Notification
			(
				GuildId						TEXT NOT NULL,
				ChannelId					TEXT,
				Event						TEXT NOT NULL,
				Content						TEXT,
				AuthorIconUrl				TEXT,
				AuthorName					TEXT,
				AuthorUrl					TEXT,
				Color						INTEGER DEFAULT 0 NOT NULL,
				Description					TEXT,
				Footer						TEXT,
				FooterIconUrl				TEXT,
				ImageUrl					TEXT,
				ThumbnailUrl				TEXT,
				Title						TEXT,
				Url							TEXT,
				PRIMARY KEY(GuildId, Event)
			);
			CREATE INDEX IF NOT EXISTS Notification_GuildId_Index ON Notification
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Notification_ChannelId_Index ON Notification
			(
				ChannelId
			);
			");
		}
	}
}