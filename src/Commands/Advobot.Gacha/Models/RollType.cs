namespace Advobot.Gacha.Models;

[Flags]
public enum RollType : ulong
{
	Nothing = 0,
	Game = 1U << 0,
	Manga = 1U << 1,
	Anime = 1U << 2,
	All = Game | Manga | Anime,
}