using Advobot.Embeds;
using Advobot.Modules;
using Advobot.Utilities;

using Discord;

using static Advobot.Resources.Responses;

namespace Advobot.Standard.Responses;

public sealed class Webhooks : AdvobotResult
{
	public static AdvobotResult DisplayWebhooks(
		ISnowflakeEntity source,
		IReadOnlyCollection<IWebhook> webhooks)
	{
		var title = WebhooksTitleDisplayWebhooks.Format(
			source.Format().WithBlock()
		);
		var description = webhooks
			.Select(x => x.Format())
			.Join(Environment.NewLine)
			.WithBigBlock()
			.Current;
		return Success(new EmbedWrapper
		{
			Title = title,
			Description = description,
		});
	}

	public static AdvobotResult ModifiedChannel(IWebhook webhook, ITextChannel channel)
	{
		return Success(WebhooksModifiedChannel.Format(
			webhook.Format().WithBlock(),
			channel.Format().WithBlock()
		));
	}
}