using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels
{
	//Because Discord.Net uses a Nuget package for IAsyncEnumerable from pre .Net Core 3.0/Standard 2.0
	extern alias oldasyncenumerable;

	public sealed class FakeTextChannel : FakeGuildChannel, ITextChannel
	{
		public ulong? CategoryId => ProtectedCategoryId;
		public bool IsNsfw { get; set; }
		public string Mention => $"<#{Id}>";
		public int SlowModeInterval { get; set; }
		public string Topic { get; set; }

		public FakeTextChannel(FakeGuild guild) : base(guild)
		{
		}

		public Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
		{
			var invite = new FakeInviteMetadata(this, FakeGuild.FakeCurrentUser)
			{
				MaxAge = maxAge,
				MaxUses = maxUses,
				IsTemporary = isTemporary,
			};
			return Task.FromResult<IInviteMetadata>(invite);
		}

		public Task<IWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
			=> throw new NotImplementedException();

		public Task<ICategoryChannel> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		{
			var match = FakeGuild.FakeChannels.SingleOrDefault(x => x.Id == CategoryId);
			return Task.FromResult((ICategoryChannel)match);
		}

		public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
		{
			var matches = FakeGuild.FakeInvites.Where(x => x.ChannelId == Id).ToArray();
			return Task.FromResult<IReadOnlyCollection<IInviteMetadata>>(matches);
		}

		public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
		{
			var matches = FakeGuild.FakeWebhooks.Where(x => x.ChannelId == Id);
			var match = matches.SingleOrDefault(x => x.Id == id);
			return Task.FromResult<IWebhook>(match);
		}

		public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null)
		{
			var matches = FakeGuild.FakeWebhooks.Where(x => x.ChannelId == Id).ToArray();
			return Task.FromResult<IReadOnlyCollection<IWebhook>>(matches);
		}

		public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
		{
			ModifyAsync((Action<GuildChannelProperties>)func);

			var args = new TextChannelProperties();
			func(args);

			IsNsfw = args.IsNsfw.GetValueOrDefault();
			SlowModeInterval = args.SlowModeInterval.GetValueOrDefault();
			Topic = args.Topic.GetValueOrDefault();

			return Task.CompletedTask;
		}

		public async Task SyncPermissionsAsync(RequestOptions options = null)
		{
			var category = await GetCategoryAsync().CAF();
			if (category == null)
			{
				return;
			}

			_Permissions.Clear();
			foreach (var overwrite in category.PermissionOverwrites)
			{
				_Permissions[overwrite.TargetId] = overwrite;
			}
		}
	}
}