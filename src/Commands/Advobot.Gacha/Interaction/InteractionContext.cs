using Discord;
using Discord.WebSocket;

namespace Advobot.Gacha.Interaction
{
	public sealed class InteractionContext : IInteractionContext
	{
		public IGuildUser User { get; }
		public ITextChannel Channel { get; }
		public IGuild Guild => Channel.Guild;
		public IInteraction Action { get; }

		public InteractionContext(IMessage message, IInteraction action) : this(action)
		{
			User = (IGuildUser)message.Author;
			Channel = (ITextChannel)message.Channel;
		}
		public InteractionContext(SocketReaction reaction, IInteraction action) : this(action)
		{
			User = (IGuildUser)reaction.User.Value;
			Channel = (ITextChannel)reaction.Channel;
		}
		private InteractionContext(IInteraction action)
		{
			Action = action;
		}
	}
}
