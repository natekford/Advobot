namespace Advobot.Levels.Service;

/// <summary>
/// Abstraction for giving experience and rewards for chatting.
/// </summary>
public interface ILevelService
{
	int CalculateLevel(int experience);
}