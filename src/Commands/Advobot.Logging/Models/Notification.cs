namespace Advobot.Logging.Models;

public enum Notification : long
{
	None = 0,
	Goodbye = 1U << 0,
	Welcome = 1U << 1,
}