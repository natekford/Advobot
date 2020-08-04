using FluentMigrator;

namespace Advobot.Gacha.Migrations
{
	[Migration(20200803175200)]
	public sealed class AddWishTable : Migration
	{
		public override void Down()
			=> Delete.Table("Wish");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Wish
			(
				WishId						INTEGER NOT NULL,
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				PRIMARY KEY(GuildId, UserId, CharacterId)
			);
			CREATE INDEX IF NOT EXISTS Wish_GuildId_Index ON Wish
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Wish_GuildId_UserId_Index ON Wish
			(
				GuildId,
				UserId
			);
			CREATE INDEX IF NOT EXISTS Wish_GuildId_CharacterId_Index ON Wish
			(
				GuildId,
				CharacterId
			);
			");
		}
	}
}