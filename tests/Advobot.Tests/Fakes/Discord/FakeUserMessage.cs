﻿
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeUserMessage : FakeMessage, IUserMessage
	{
		public IUserMessage ReferencedMessage => throw new NotImplementedException();

		public FakeUserMessage(FakeMessageChannel channel, FakeUser author, string content)
					: base(channel, author, content) { }

		public Task CrosspostAsync(RequestOptions? options = null)
			=> throw new NotImplementedException();

		public Task ModifyAsync(Action<MessageProperties> func, RequestOptions? options = null)
			=> throw new NotImplementedException();

		public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions? options = null)
			=> throw new NotImplementedException();

		public Task PinAsync(RequestOptions? options = null)
			=> throw new NotImplementedException();

		public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
			=> throw new NotImplementedException();

		public Task UnpinAsync(RequestOptions? options = null)
			=> throw new NotImplementedException();
	}
}