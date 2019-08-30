namespace Advobot.Gacha.Database
{
	public interface IDatabaseStarter
	{
		string GetConnectionString();

		bool IsDatabaseCreated();
	}
}