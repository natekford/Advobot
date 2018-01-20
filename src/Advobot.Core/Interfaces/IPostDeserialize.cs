using Discord.WebSocket;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// An interfaces for guild settings to be post deserialized with.
	/// </summary>
	public interface IPostDeserialize
    {
		/// <summary>
		/// Sets some values which require a guild to be gotten.
		/// </summary>
		/// <param name="guild"></param>
		void PostDeserialize(SocketGuild guild);
    }
}
