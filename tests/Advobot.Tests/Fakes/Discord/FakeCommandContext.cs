using Advobot.Tests.Fakes.Discord.Channels;
using Discord;
using Discord.Commands;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeCommandContext : ICommandContext
	{
		public FakeClient Client { get; }
		public FakeGuild Guild { get; }
		public FakeMessageChannel Channel { get; }
		public FakeUser User { get; }
		public FakeUserMessage Message { get; }

		public FakeCommandContext(FakeClient client, FakeUserMessage message)
		{
			Client = client;
			Channel = message.Channel;
			User = message.Author;
			Message = message;
			Guild = ((FakeGuildChannel)message.Channel).Guild;
		}

		IDiscordClient ICommandContext.Client => Client;
		IGuild ICommandContext.Guild => Guild;
		IMessageChannel ICommandContext.Channel => Channel;
		IUser ICommandContext.User => User;
		IUserMessage ICommandContext.Message => Message;
	}
}
