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

		public InteractionContext(IMessage message, IInteraction action)
			: this((IGuildUser)message.Author, (ITextChannel)message.Channel, action) { }
		public InteractionContext(SocketReaction reaction, IInteraction action)
			: this((IGuildUser)reaction.User.Value, (ITextChannel)reaction.Channel, action) { }
		private InteractionContext(IGuildUser user, ITextChannel channel, IInteraction action)
		{
			User = user;
			Channel = channel;
			Action = action;
		}
	}
}
