﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels
{
	public class FakeMessageChannel : FakeChannel, IMessageChannel
	{
		public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
			=> throw new NotImplementedException();

		public IDisposable EnterTypingState(RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();

		public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();

		public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();

		public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IReadOnlyCollection<IMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
			=> throw new NotImplementedException();

		public Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
			=> throw new NotImplementedException();

		public Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null)
			=> throw new NotImplementedException();

		public Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null)
			=> throw new NotImplementedException();

		public Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null)
			=> throw new NotImplementedException();

		public Task TriggerTypingAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
	}
}