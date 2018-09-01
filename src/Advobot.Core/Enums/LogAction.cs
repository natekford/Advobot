namespace Advobot.Enums
{
	/// <summary>
	/// Allows certain guild events to be logged when these are in <see cref="Interfaces.IGuildSettings.LogActions"/>.
	/// </summary>
	public enum LogAction
	{
		/// <summary>
		/// Log users joining the guild.
		/// </summary>
		UserJoined,
		/// <summary>
		/// Log users leaving the guild.
		/// </summary>
		UserLeft,
		/// <summary>
		/// Log users changing their name.
		/// </summary>
		UserUpdated,
		/// <summary>
		/// Log messages being received.
		/// </summary>
		MessageReceived,
		/// <summary>
		/// Log messages being edited.
		/// </summary>
		MessageUpdated,
		/// <summary>
		/// Log messages being deleted.
		/// </summary>
		MessageDeleted,
	}
}
