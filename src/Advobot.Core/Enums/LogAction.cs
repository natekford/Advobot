using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Allows certain guild events to be logged when these are in <see cref="Interfaces.IGuildSettings.LogActions"/>.
	/// </summary>
	[Flags]
	public enum LogAction : uint
	{
		UserJoined = (1U << 0),
		UserLeft = (1U << 1),
		UserUpdated = (1U << 2),
		MessageReceived = (1U << 3),
		MessageUpdated = (1U << 4),
		MessageDeleted = (1U << 5)
	}
}
