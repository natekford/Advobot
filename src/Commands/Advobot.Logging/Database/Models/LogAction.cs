namespace Advobot.Logging.Database.Models;

public enum LogAction : long
{
	None = 0,
	UserJoined = 1U << 0,
	UserLeft = 1U << 1,
	UserUpdated = 1U << 2,
	MessageReceived = 1U << 3,
	MessageUpdated = 1U << 4,
	MessageDeleted = 1U << 5,
}