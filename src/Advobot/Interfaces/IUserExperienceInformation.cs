using System;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for the information used for the level service.
	/// </summary>
	public interface IUserExperienceInformation
	{
		/// <summary>
		/// The id of the user.
		/// </summary>
		ulong UserId { get; }
		/// <summary>
		/// The current total experience of the user.
		/// </summary>
		int Experience { get; }
		/// <summary>
		/// How many messages this has used for adding experience.
		/// This can be useful for finding the average xp gained per message.
		/// This is not 100% accurate however.
		/// </summary>
		int MessageCount { get; }
		/// <summary>
		/// The time the user last gained xp at.
		/// </summary>
		DateTime Time { get; }

		/// <summary>
		/// Adds experience to the user if the author of the message is this user.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="experience"></param>
		void AddExperience(SocketUserMessage message, int experience);
		/// <summary>
		/// Removes the experience from the user if the author of the message is this user.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="experience"></param>
		void RemoveExperience(SocketUserMessage message, int experience);
		/// <summary>
		/// Gets the experience a user has in a specific guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		int GetExperience(SocketGuild guild);
		/// <summary>
		/// Gets the experience a user has in a specific channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		int GetExperience(SocketTextChannel channel);
	}
}