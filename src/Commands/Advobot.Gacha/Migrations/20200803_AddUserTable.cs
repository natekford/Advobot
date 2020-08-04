using FluentMigrator;

namespace Advobot.Gacha.Migrations
{
	[Migration(20200803175100)]
	public sealed class AddUserTable : Migration
	{
		public override void Down()
			=> Delete.Table("User");

		public override void Up()
		{
			Execute.Sql(@"
			CREATE TABLE IF NOT EXISTS User
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				PRIMARY KEY(GuildId, UserId)
			);
			");
		}
	}
}