using System;
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
			var title = WebhooksTitleDisplayWebhooks.Format(
				source.Format().WithBlock()
			);
			var description = webhooks
				.ToDelimitedString(x => x.Format(), Environment.NewLine)
				.WithBigBlock()
				.Value;
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
}
