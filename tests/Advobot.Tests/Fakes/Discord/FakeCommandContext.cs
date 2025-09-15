using Advobot.Modules;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public class FakeCommandContext : IGuildContext
{
	public FakeGuildUser Bot => Guild.FakeCurrentUser;
	public FakeTextChannel Channel { get; }
	public FakeClient Client { get; }
	public FakeGuild Guild { get; }
	public Guid Id { get; } = Guid.NewGuid();
	public FakeUserMessage Message { get; }
	public IServiceProvider Services { get; }
	public object Source => Message;
	public FakeGuildUser User { get; }
	ITextChannel IGuildContext.Channel => Channel;
	IDiscordClient IGuildContext.Client => Client;
	IGuild IGuildContext.Guild => Guild;
	IUserMessage IGuildContext.Message => Message;
	IGuildUser IGuildContext.User => User;

	public FakeCommandContext(IServiceProvider services, FakeClient client, FakeUserMessage message)
	{
		Services = services;
		Client = client;
		Channel = (FakeTextChannel)message.FakeChannel;
		User = (FakeGuildUser)message.FakeAuthor;
		Message = message;
		Guild = User.Guild;
	}
}