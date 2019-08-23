using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Tests.Fakes.Discord.Channels;
using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	//Because Discord.Net uses a Nuget package for IAsyncEnumerable from pre .Net Core 3.0/Standard 2.0
	extern alias oldasyncenumerable;

	public class FakeUserMessage : FakeMessage, IUserMessage
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotImplementedException();

		public FakeUserMessage(FakeMessageChannel channel, FakeUser author, string content)
			: base(channel, author, content) { }

		public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
			=> throw new NotImplementedException();
		public oldasyncenumerable::System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task PinAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task RemoveAllReactionsAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
			=> throw new NotImplementedException();
		public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
			=> throw new NotImplementedException();
		public Task UnpinAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null)
			=> throw new NotImplementedException();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}
