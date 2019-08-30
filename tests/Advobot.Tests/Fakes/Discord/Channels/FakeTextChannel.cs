using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels
{
	//Because Discord.Net uses a Nuget package for IAsyncEnumerable from pre .Net Core 3.0/Standard 2.0
	extern alias oldasyncenumerable;

	public class FakeTextChannel : FakeGuildChannel, ITextChannel
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public bool IsNsfw => throw new NotImplementedException();
		public string Topic => throw new NotImplementedException();
		public int SlowModeInterval => throw new NotImplementedException();
		public string Mention => throw new NotImplementedException();
		public ulong? CategoryId => throw new NotImplementedException();

		public FakeTextChannel(FakeGuild guild) : base(guild)
		{
		}

		public Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task SyncPermissionsAsync(RequestOptions options = null)
			=> throw new NotImplementedException();

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}