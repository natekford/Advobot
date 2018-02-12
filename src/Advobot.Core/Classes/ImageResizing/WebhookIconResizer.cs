using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public class WebhookIconResizer : ImageResizer<IconResizerArguments>
	{
		private ConcurrentDictionary<ulong, IWebhook> _Webhooks = new ConcurrentDictionary<ulong, IWebhook>();

		public WebhookIconResizer(int threads) : base(threads) { }

		public void AddWebhook(IGuild guild, IWebhook webhook)
		{
			_Webhooks.AddOrUpdate(guild.Id, webhook, (k, v) => webhook);
		}
		protected override async Task Create(AdvobotSocketCommandContext context, IconResizerArguments args, Uri uri, RequestOptions options, string nameForEmote)
		{
			using (var resp = await ImageUtils.ResizeImageAsync(uri, context, args).CAF())
			{
				if (!_Webhooks.TryRemove(context.Guild.Id, out var webhook))
				{
					await MessageUtils.SendErrorMessageAsync(context, new Error("Unable to find the webhook to update."));
					return;
				}
				if (resp.IsSuccess)
				{
					await webhook.ModifyAsync(x => x.Image = new Image(resp.Stream), options).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(context, $"Successfully updated the webhook icon of `{webhook.Format()}`.").CAF();
					return;
				}
				await MessageUtils.SendErrorMessageAsync(context, new Error($"Failed to update the webhook icon of `{webhook.Format()}`. Reason: {resp.Error}.")).CAF();
			}
		}
	}

}
