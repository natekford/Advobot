namespace Advobot.Levels.Database
{
	public interface IDatabaseStarter
	{
		string GetConnectionString();

		bool IsDatabaseCreated();
	}
}