using Advobot.Tests.Fakes.Discord.Channels;

using Discord;
using Discord.Commands;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeCommandContext : ICommandContext
	{
		public FakeCommandContext(FakeClient client, FakeUserMessage message)
		{
			Client = client;
			Channel = message.Channel;
			User = message.Author;
			Message = message;
			Guild = ((FakeGuildChannel)message.Channel).Guild;
		}

		public FakeMessageChannel Channel { get; }
		public FakeClient Client { get; }
		public FakeGuild Guild { get; }
		public FakeUserMessage Message { get; }
		public FakeUser User { get; }
		IMessageChannel ICommandContext.Channel => Channel;
		IDiscordClient ICommandContext.Client => Client;
		IGuild ICommandContext.Guild => Guild;
		IUserMessage ICommandContext.Message => Message;
		IUser ICommandContext.User => User;
	}
}