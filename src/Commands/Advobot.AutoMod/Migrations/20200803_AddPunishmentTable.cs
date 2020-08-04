using FluentMigrator;

namespace Advobot.AutoMod.Migrations
{
	[Migration(20200803174100)]
	public sealed class AddPunishmentTable : Migration
	{
		public override void Down()
			=> Delete.Table("Punishment");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Punishment
			(
				GuildId					TEXT NOT NULL,
				PunishmentType			TEXT NOT NULL,
				Instances				INTEGER NOT NULL,
				LengthTicks				INTEGER,
				RoleId					TEXT,
				PRIMARY KEY(GuildId, PunishmentType, Instances)
			);
			CREATE INDEX IF NOT EXISTS Punishment_GuildId_Index ON Punishment
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Punishment_GuildId_PunishmentType_Index ON Punishment
			(
				GuildId,
				PunishmentType
			);
			");
		}
	}
}