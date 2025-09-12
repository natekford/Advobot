using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to find an <see cref="IWebhook"/> on a guild.
/// </summary>
[TypeReaderTargetType(typeof(IWebhook))]
public sealed class WebhookTypeReader : TypeReader
{
	/// <summary>
	/// Checks for any webhooks matching the input. Input is tested as a webhook id and a username.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="input"></param>
	/// <param name="services"></param>
	/// <returns></returns>
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var webhooks = await context.Guild.GetWebhooksAsync().ConfigureAwait(false);

		if (ulong.TryParse(input, out var id)
			&& webhooks.FirstOrDefault(x => x.Id == id) is IWebhook wh)
		{
			var wrapper = await WebhookWrapper.CreateAsync(wh, context.Guild).ConfigureAwait(false);
			if (wrapper is null)
			{
				return TypeReaderResult.FromSuccess(wrapper);
			}
		}

		var wrappers = new List<IWebhook>();
		foreach (var webhook in webhooks.Where(x => x.Name.CaseInsEquals(input)))
		{
			var wrapper = await WebhookWrapper.CreateAsync(webhook, context.Guild).ConfigureAwait(false);
			if (wrapper is not null)
			{
				wrappers.Add(wrapper);
			}
		}

		return TypeReaderUtils.SingleValidResult(wrappers, "webhooks", input);
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