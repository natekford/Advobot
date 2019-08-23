using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels
{
	//Because Discord.Net uses a Nuget package for IAsyncEnumerable from pre .Net Core 3.0/Standard 2.0
	extern alias oldasyncenumerable;

	public class FakeMessageChannel : FakeChannel, IMessageChannel
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
			=> throw new NotImplementedException();
		public IDisposable EnterTypingState(RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();
		public oldasyncenumerable::System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();
		public oldasyncenumerable::System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();
		public oldasyncenumerable::System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task<IReadOnlyCollection<IMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
			=> throw new NotImplementedException();
		public Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
			=> throw new NotImplementedException();
		public Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task TriggerTypingAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}
