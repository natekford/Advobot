namespace Advobot.Services.GuildSettings.Settings
{
	/// <summary>
	/// Specify what punishment should be given.
	/// </summary>
	public enum Punishment
	{
		/// <summary>
		/// Indicates this is the default value and to do nothing.
		/// </summary>
		Nothing,
		/// <summary>
		/// Make a user unable to hear anything.
		/// </summary>
		Deafen,
		/// <summary>
		/// Make a user unable to speak in voice chat.
		/// </summary>
		VoiceMute,
		/// <summary>
		/// Make a user unable to type in text chat.
		/// </summary>
		RoleMute,
		/// <summary>
		/// Remove a user from the server.
		/// </summary>
		Kick,
		/// <summary>
		/// Remove a user from the server and delete their recent messages.
		/// </summary>
		Softban,
		/// <summary>
		/// Ban a user from the server.
		/// </summary>
		Ban,
	}
}