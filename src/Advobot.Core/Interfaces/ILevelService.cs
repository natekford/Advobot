using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for giving experience and rewards for chatting.
	/// </summary>
	public interface ILevelService : IUsesDatabase
	{
		/// <summary>
		/// Adds experience to the author of the supplied message.
		/// </summary>
		/// <param name="message"></param>
		Task AddExperienceAsync(SocketMessage message);
		/// <summary>
		/// Removes experience from the author of the supplied message.
		/// This is reliant upon keeping messages cached, which can be disrupted by a bot restart.
		/// Due to that, do not rely heavily on it.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		Task RemoveExperienceAsync(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel);
		/// <summary>
		/// Calculates the level from the given experience.
		/// </summary>
		/// <param name="experience"></param>
		/// <returns></returns>
		int CalculateLevel(int experience);
		/// <summary>
		/// Gets the rank of the user in the guild, and returns the total amount of users who have gained xp in the guild.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		(int Rank, int TotalUsers) GetGuildRank(SocketGuild guild, ulong userId);
		/// <summary>
		/// Gets the rank of the user globally, and returns the total amount of users who have gained xp globally.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		(int Rank, int TotalUsers) GetGlobalRank(ulong userId);
		/// <summary>
		/// Gets the information about a user's xp and its distribution.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		IUserExperienceInformation GetUserXpInformation(ulong userId);
		/// <summary>
		/// Sends the user's xp information to the channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="userId">The user to get the information for.</param>
		/// <param name="global">Whether to include global information. If false, includes only guild information.</param>
		/// <returns></returns>
		Task SendUserXpInformationAsync(SocketTextChannel channel, ulong userId, bool global);
	}
}