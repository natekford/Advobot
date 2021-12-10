﻿namespace Advobot.Punishments;

/// <summary>
/// Specify what punishment should be given.
/// </summary>
public enum PunishmentType
{
	/// <summary>
	/// Indicates this is the default value and to do nothing.
	/// </summary>
	Nothing = 0,
	/// <summary>
	/// Make a user unable to hear anything.
	/// </summary>
	Deafen = 1,
	/// <summary>
	/// Make a user unable to speak in voice chat.
	/// </summary>
	VoiceMute = 2,
	/// <summary>
	/// Make a user unable to type in text chat.
	/// </summary>
	RoleMute = 3,
	/// <summary>
	/// Remove a user from the server.
	/// </summary>
	Kick = 4,
	/// <summary>
	/// Remove a user from the server and delete their recent messages.
	/// </summary>
	Softban = 5,
	/// <summary>
	/// Ban a user from the server.
	/// </summary>
	Ban = 6,
}