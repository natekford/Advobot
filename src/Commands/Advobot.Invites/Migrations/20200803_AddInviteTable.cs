using FluentMigrator;

namespace Advobot.Invites.Migrations;

[Migration(20200803173300)]
public sealed class AddInviteTable : Migration
{
	public override void Down()
		=> Delete.Table("Invite");

	public override void Up()
	{
		Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS Invite
			(
				GuildId						TEXT NOT NULL,
				Code						TEXT NOT NULL,
				Name						TEXT NOT NULL,
				HasGlobalEmotes				INTEGER NOT NULL,
				LastBumped					INTEGER NOT NULL,
				MemberCount					INTEGER NOT NULL,
				PRIMARY KEY(GuildId)
			);
			");
	}
}