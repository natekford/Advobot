namespace Advobot.Logging
{
	public enum LogAction : long
	{
		None = 0,

		/// <summary>
		/// Log users joining the guild.
		/// </summary>
		UserJoined = 1U << 0,

		/// <summary>
		/// Log users leaving the guild.
		/// </summary>
		UserLeft = 1U << 1,

		/// <summary>
		/// Log users changing their name.
		/// </summary>
		UserUpdated = 1U << 2,

		/// <summary>
		/// Log messages being received.
		/// </summary>
		MessageReceived = 1U << 3,

		/// <summary>
		/// Log messages being edited.
		/// </summary>
		MessageUpdated = 1U << 4,

		/// <summary>
		/// Log messages being deleted.
		/// </summary>
		MessageDeleted = 1U << 5,
	}
}