using FluentMigrator;

namespace Advobot.AutoMod.Migrations
{
	[Migration(20200813205300)]
	public sealed class AddRemovablePunishmentTable : Migration
	{
		public override void Down()
			=> Delete.Table("RemovablePunishment");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS RemovablePunishment
			(
				GuildId					TEXT NOT NULL,
				UserId					TEXT NOT NULL,
				PunishmentType			TEXT NOT NULL,
				EndTimeTicks			INTEGER NOT NULL,
				RoleId					TEXT,
				PRIMARY KEY(GuildId, UserId, PunishmentType)
			);
			CREATE INDEX IF NOT EXISTS RemovablePunishment_GuildId_Index ON RemovablePunishment
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS RemovablePunishment_GuildId_PunishmentType_Index ON RemovablePunishment
			(
				GuildId,
				PunishmentType
			);
			");
		}
	}
}