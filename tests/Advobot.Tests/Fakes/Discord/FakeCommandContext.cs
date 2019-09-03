using Advobot.Tests.Fakes.Discord.Channels;

using Discord;
using Discord.Commands;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeCommandContext : ICommandContext
	{
		public FakeTextChannel Channel { get; }
		public FakeClient Client { get; }
		public FakeGuild Guild { get; }
		public FakeUserMessage Message { get; }
		public FakeGuildUser User { get; }
		IMessageChannel ICommandContext.Channel => Channel;
		IDiscordClient ICommandContext.Client => Client;
		IGuild ICommandContext.Guild => Guild;
		IUserMessage ICommandContext.Message => Message;
		IUser ICommandContext.User => User;

		public FakeCommandContext(FakeClient client, FakeUserMessage message)
		{
			Client = client;
			Channel = (FakeTextChannel)message.Channel;
			User = (FakeGuildUser)message.Author;
			Message = message;
			Guild = User.Guild;
		}
	}
}