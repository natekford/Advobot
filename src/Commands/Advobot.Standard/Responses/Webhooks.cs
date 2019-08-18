using System.Collections.Generic;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;
using Discord;
using static Advobot.Standard.Resources.Responses;

namespace Advobot.Standard.Responses
{
	public sealed class Webhooks : CommandResponses
	{
		private Webhooks() { }

		public static AdvobotResult DisplayWebhooks(
			ISnowflakeEntity source,
			IReadOnlyCollection<IWebhook> webhooks)
		{
			return Success(new EmbedWrapper
			{
				Title = Title.Format(WebhooksTitleDisplayWebhooks, source),
				Description = BigBlock.FormatInterpolated($"{webhooks}"),
			});
		}
		public static AdvobotResult ModifiedChannel(IWebhook webhook, ITextChannel channel)
			=> Success(Default.Format(WebhooksModifiedChannel, webhook, channel));
	}
}
