using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Advobot.Interfaces
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
		Task AddExperience(SocketMessage message);
		/// <summary>
		/// Removes experience from the author of the supplied message.
		/// This is reliant upon keeping messages cached, which can be disrupted by a bot restart.
		/// Due to that, do not rely heavily on it.
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		Task RemoveExperience(Cacheable<IMessage, ulong> cached, ISocketMessageChannel channel);
		/// <summary>
		/// Calculates the level from the given experience.
		/// </summary>
		/// <param name="experience"></param>
		/// <returns></returns>
		int CalculateLevel(int experience);
		/// <summary>
		/// Gets the information about a user's xp and its distribution.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		IUserExperienceInformation GetUserInformation(ulong userId);
	}
}