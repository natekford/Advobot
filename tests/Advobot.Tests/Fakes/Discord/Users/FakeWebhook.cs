using Advobot.Tests.Fakes.Discord.Channels;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Users;

public sealed class FakeWebhook(FakeTextChannel channel, FakeUser user)
	: FakeUser, IWebhook
{
	public ulong? ApplicationId => throw new NotImplementedException();
	public ulong? ChannelId => FakeChannel.Id;
	public FakeTextChannel FakeChannel { get; private set; } = channel;
	public FakeGuild FakeGuild { get; } = channel.FakeGuild;
	public FakeUser FakeUser { get; } = user;
	public ulong? GuildId => FakeGuild.Id;
	public string Name { get; set; }
	public string Token => throw new NotImplementedException();
	public WebhookType Type => throw new NotImplementedException();
	IIntegrationChannel IWebhook.Channel => FakeChannel;
	IUser IWebhook.Creator => FakeUser;
	IGuild IWebhook.Guild => FakeGuild;

	public Task DeleteAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task ModifyAsync(Action<WebhookProperties> func, RequestOptions? options = null)
	{
		var args = new WebhookProperties();
		func(args);

		FakeChannel = (FakeTextChannel)args.Channel.GetValueOrDefault(FakeChannel);
		Name = args.Name.GetValueOrDefault(Name);

		return Task.CompletedTask;
	}
}