namespace Advobot.Logging.Service.Context;

public interface ILogContext<out T> : ILogContext
{
	T State { get; }
}