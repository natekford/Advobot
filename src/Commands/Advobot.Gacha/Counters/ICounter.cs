namespace Advobot.Gacha.Counters
{
	public interface ICounter<T>
	{
		bool CanDo(T id);

		void HasBeenDone(T id);
	}
}