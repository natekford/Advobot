using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public class FakeUserMessage(FakeMessageChannel channel, FakeUser author, string content)
	: FakeMessage(channel, author, content), IUserMessage
{
	public IUserMessage ReferencedMessage => throw new NotImplementedException();
	public MessageResolvedData ResolvedData => throw new NotImplementedException();

	public Task CrosspostAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task ModifyAsync(Action<MessageProperties> func, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task PinAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
		=> throw new NotImplementedException();

	public Task UnpinAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();
}