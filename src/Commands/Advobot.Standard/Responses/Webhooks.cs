using System.Collections.Generic;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Utilities;
using Discord;

namespace Advobot.Standard.Responses
{
	public sealed class Webhooks : CommandResponses
	{
		private Webhooks() { }

		public static AdvobotResult DisplayWebhooks(ISnowflakeEntity source, IReadOnlyCollection<IWebhook> webhooks)
		{
			return Success(new EmbedWrapper
			{
				Title = Title.FormatInterpolated($"Webhooks For {source}"),
				Description = BigBlock.FormatInterpolated($"{webhooks}"),
			});
		}
		public static AdvobotResult ModifiedChannel(IWebhook webhook, ITextChannel channel)
			=> Success(Default.FormatInterpolated($"Successfully changed the channel of {webhook} to {channel}."));
	}
}
