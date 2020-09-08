namespace Advobot.Logging.Context
{
	public interface ILogContext<out T> : ILogContext
	{
		T State { get; }
	}
}