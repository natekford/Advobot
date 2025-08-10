using FluentMigrator;

namespace Advobot.AutoMod.Database.Migrations;

[Migration(20200913203000)]
public sealed class AddSelfRoleTable : Migration
{
	public override void Down()
		=> Delete.Table("SelfRole");

	public override void Up()
	{
		Execute.Sql(@"
		CREATE TABLE IF NOT EXISTS SelfRole
		(
			GuildId					TEXT NOT NULL,
			RoleId					TEXT NOT NULL,
			GroupId					INTEGER NOT NULL,
			PRIMARY KEY(RoleId)
		);
		CREATE INDEX IF NOT EXISTS SelfRole_GuildId_Index ON SelfRole
		(
			GuildId
		);
		CREATE INDEX IF NOT EXISTS SelfRole_GuildId_Group_Index ON SelfRole
		(
			GuildId,
			GroupId
		);
		");
	}
}