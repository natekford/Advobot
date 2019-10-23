namespace Advobot.Logging
{
	public enum Log : long
	{
		None = 0,
		Image = 1U << 0,
		Mod = 1U << 1,
		Server = 1U << 2,
	}
}