using FluentMigrator;

namespace Advobot.Gacha.Migrations
{
	[Migration(20200803174800)]
	public sealed class AddClaimTable : Migration
	{
		public override void Down()
			=> Delete.Table("Claim");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Claim
			(
				ClaimId						INTEGER NOT NULL,
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				ImageUrl					TEXT,
				IsPrimaryClaim				INTEGER NOT NULL,
				PRIMARY KEY(GuildId, CharacterId)
			);
			CREATE INDEX IF NOT EXISTS Claim_GuildId_Index ON Claim
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Claim_GuildId_UserId_Index ON Claim
			(
				GuildId,
				UserId
			);
			");
		}
	}
}