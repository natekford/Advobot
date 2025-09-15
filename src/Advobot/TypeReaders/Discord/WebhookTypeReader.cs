using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders.Discord;

/// <summary>
/// Attempts to find an <see cref="IWebhook"/>.
/// </summary>
[TypeReaderTargetTypes(typeof(IWebhook))]
public sealed class WebhookTypeReader : DiscordTypeReader<IWebhook>
{
	/// <inheritdoc />
	public override async ITask<ITypeReaderResult<IWebhook>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var joined = Join(context, input);
		var webhooks = await context.Guild.GetWebhooksAsync().ConfigureAwait(false);

		if (ulong.TryParse(joined, out var id)
			&& webhooks.FirstOrDefault(x => x.Id == id) is IWebhook wh)
		{
			var wrapper = await WebhookWrapper.CreateAsync(wh, context.Guild).ConfigureAwait(false);
			if (wrapper is not null)
			{
				return Success(wrapper);
			}
		}

		var matches = new List<IWebhook>();
		foreach (var webhook in webhooks.Where(x => x.Name.CaseInsEquals(joined)))
		{
			var wrapper = await WebhookWrapper.CreateAsync(webhook, context.Guild).ConfigureAwait(false);
			if (wrapper is not null)
			{
				matches.Add(wrapper);
			}
		}
		return SingleValidResult(matches);
	}

	private sealed class WebhookWrapper(IWebhook Actual, IIntegrationChannel IntegrationChannel) : IWebhook
	{
		public ulong? ApplicationId => Actual.ApplicationId;
		public string AvatarId => Actual.AvatarId;
		public IIntegrationChannel Channel => IntegrationChannel;
		public ulong? ChannelId => Actual.ChannelId;
		public DateTimeOffset CreatedAt => Actual.CreatedAt;
		public IUser Creator => Actual.Creator;
		public IGuild Guild => Actual.Guild;
		public ulong? GuildId => Actual.GuildId;
		public ulong Id => Actual.Id;
		public string Name => Actual.Name;
		public string Token => Actual.Token;
		public WebhookType Type => Actual.Type;

		public static async Task<WebhookWrapper?> CreateAsync(IWebhook webhook, IGuild guild)
		{
			var channel = await guild.GetChannelAsync(webhook.ChannelId ?? 0).ConfigureAwait(false);
			if (channel is IIntegrationChannel integrationChannel)
			{
				return new(webhook, integrationChannel);
			}
			return null;
		}

		public Task DeleteAsync(RequestOptions? options = null)
			=> Actual.DeleteAsync(options);

		public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
			=> Actual.GetAvatarUrl(format, size);

		public Task ModifyAsync(Action<WebhookProperties> func, RequestOptions? options = null)
			=> Actual.ModifyAsync(func, options);
	}
}