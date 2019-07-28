namespace Advobot.Gacha.Checkers
{
	public interface IChecker<T>
	{
		bool CanDo(T id);
		void HasBeenDone(T id);
	}
}
