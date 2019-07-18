using System.Threading.Tasks;
using Advobot.Classes;
using Discord;

namespace Advobot.Services.Levels
{
	/// <summary>
	/// Abstraction for giving experience and rewards for chatting.
	/// </summary>
	public interface ILevelService
	{
		/// <summary>
		/// Adds experience to the author of the supplied message.
		/// </summary>
		/// <param name="message"></param>
		Task AddExperienceAsync(IMessage message);
		/// <summary>
		/// Removes experience from the author of the supplied message.
		/// This is reliant upon keeping messages cached, which can be disrupted by a bot restart.
		/// Due to that, do not rely heavily on it.
		/// </summary>
		/// <param name="cached"></param>
		/// <returns></returns>
		Task RemoveExperienceAsync(Cacheable<IMessage, ulong> cached);
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
		/// <param name="user"></param>
		/// <returns></returns>
		(int Rank, int TotalUsers) GetGuildRank(IGuild guild, IUser user);
		/// <summary>
		/// Gets the rank of the user globally, and returns the total amount of users who have gained xp globally.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		(int Rank, int TotalUsers) GetGlobalRank(IUser user);
		/// <summary>
		/// Gets the information about a user's xp and its distribution.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		IUserExperienceInformation GetUserXpInformation(IUser user);
		/// <summary>
		/// Sends the user's xp information to the channel.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="user">The user to get the information for.</param>
		/// <param name="global">Whether to include global information. If false, includes only guild information.</param>
		/// <returns></returns>
		EmbedWrapper GetUserXpInformationEmbedWrapper(IGuild guild, IUser user, bool global);
	}
}