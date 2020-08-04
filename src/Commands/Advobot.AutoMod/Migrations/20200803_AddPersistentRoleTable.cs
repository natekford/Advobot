using FluentMigrator;

namespace Advobot.AutoMod.Migrations
{
	[Migration(20200803174000)]
	public sealed class AddPersistentRoleTable : Migration
	{
		public override void Down()
			=> Delete.Table("PersistentRole");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS PersistentRole
			(
				GuildId					TEXT NOT NULL,
				UserId					TEXT NOT NULL,
				RoleId					TEXT NOT NULL,
				PRIMARY KEY(GuildId, UserId, RoleId)
			);
			CREATE INDEX IF NOT EXISTS PersistentRole_GuildId_Index ON PersistentRole
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS PersistentRole_GuildId_UserId_Index ON PersistentRole
			(
				GuildId,
				UserId
			);
			");
		}
	}
}