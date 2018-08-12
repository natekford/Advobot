using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.ImageResizing;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Webhooks
{
	[Category(typeof(GetWebhooks)), Group(nameof(GetWebhooks)), TopLevelShortAlias(typeof(GetWebhooks))]
	[Summary("Lists all the webhooks on the guild or the specified channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class GetWebhooks : AdvobotModuleBase
	{
		[Command]
		public async Task Command(ITextChannel channel)
		{
			var webhooks = await channel.GetWebhooksAsync().CAF();
			if (!webhooks.Any())
			{
				var error = new Error($"The channel `{channel.Format()}` does not have any webhooks.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			await MessageUtils.SendMessageAsync(Context.Channel, FormatWebhooks(channel, webhooks)).CAF();
		}
		[Command]
		public async Task Command()
		{
			var webhooks = (await Context.Guild.GetWebhooksAsync().CAF()).GroupBy(x => x.ChannelId);
			if (!webhooks.Any())
			{
				var error = new Error($"The guild does not have any webhooks.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			var parts = webhooks.Select(x => FormatWebhooks(Context.Guild.GetTextChannel(x.Key), x));
			await MessageUtils.SendMessageAsync(Context.Channel, String.Join("\n\n", parts)).CAF();
		}

		private string FormatWebhooks(ITextChannel channel, IEnumerable<IWebhook> webhooks)
		{
			return $"**{channel.Format()}**:\n{String.Join("\n", webhooks.Select(x => $"`{x.Format()}`"))}";
		}
	}

	[Category(typeof(DeleteWebhook)), Group(nameof(DeleteWebhook)), TopLevelShortAlias(typeof(DeleteWebhook))]
	[Summary("Deletes a webhook from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteWebhook : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IWebhook webhook)
		{
			await webhook.DeleteAsync(GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the webhook `{webhook.Format()}`.").CAF();
		}
	}

	[Category(typeof(ModifyWebhookName)), Group(nameof(ModifyWebhookName)), TopLevelShortAlias(typeof(ModifyWebhookName))]
	[Summary("Changes the name of a webhook.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyWebhookName : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IWebhook webhook, [Remainder, VerifyStringLength(Target.Name)] string name)
		{
			await webhook.ModifyAsync(x => x.Name = name, GetRequestOptions()).CAF();
			var resp = $"Successfully changed the name of `{webhook.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyWebhookChannel)), Group(nameof(ModifyWebhookChannel)), TopLevelShortAlias(typeof(ModifyWebhookChannel))]
	[Summary("Changes the channel of a webhook.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyWebhookChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IWebhook webhook, [VerifyObject(true, Verif.CanManageWebhooks)] ITextChannel channel)
		{
			await webhook.ModifyAsync(x => x.Channel = Optional.Create(channel), GetRequestOptions()).CAF();
			var resp = $"Successfully set the channel of `{webhook.Format()}` to `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyWebhookIcon)), Group(nameof(ModifyWebhookIcon)), TopLevelShortAlias(typeof(ModifyWebhookIcon))]
	[Summary("Changes the icon of a webhook.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyWebhookIcon : AdvobotModuleBase
	{
		private static WebhookIconResizer _Resizer = new WebhookIconResizer(4);

		[Command]
		public async Task Command(IWebhook webhook, Uri url)
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on a webhook icon.")).CAF();
				return;
			}

			_Resizer.EnqueueArguments(Context, new IconResizerArguments(), url, GetRequestOptions(), webhook.Id.ToString());
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in webhook icon creation queue: {_Resizer.QueueCount}.").CAF();
			if (_Resizer.CanStart)
			{
				_Resizer.StartProcessing();
			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(IWebhook webhook)
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on a webhook icon.")).CAF();
				return;
			}

			await webhook.ModifyAsync(x => x.Image = new Image(), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully removed the webhook icon from {webhook.Format()}.").CAF();
		}
	}

	[Category(typeof(SendMessageThroughWebhook)), Group(nameof(SendMessageThroughWebhook)), TopLevelShortAlias(typeof(SendMessageThroughWebhook))]
	[Summary("Sends a message through a webhook.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(false)]
	public sealed class SendMessageThroughWebhook : AdvobotModuleBase
	{
		private static ConcurrentDictionary<ulong, RateLimit> _RateLimits = new ConcurrentDictionary<ulong, RateLimit>();

		[Command]
		public async Task Command(IWebhook webhook, [Remainder] string text)
		{
			if (_RateLimits.TryGetValue(Context.Guild.Id, out var rateLimit) && rateLimit.Messages < 1 && DateTime.UtcNow < rateLimit.Time)
			{
				var error = new Error($"Cannot send a new message to the webhook until `{rateLimit.Time.ToLongTimeString()}` UTC");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			var req = (HttpWebRequest)WebRequest.Create($"https://canary.discordapp.com/api/webhooks/{webhook.Id}/{webhook.Token}");
			req.Proxy = new WebProxy();
			req.Credentials = CredentialCache.DefaultCredentials;
			req.Method = HttpMethod.Post.Method;
			req.Accept = req.ContentType = "application/json";

			var bytes = new ASCIIEncoding().GetBytes($@"{{ ""content"":""{text}"" }}");
			req.ContentLength = bytes.Length;
			using (var s = await req.GetRequestStreamAsync().CAF())
			{
				await s.WriteAsync(bytes, 0, bytes.Length).CAF();
			}

			var resp = (HttpWebResponse)(await req.GetResponseAsync().CAF());
			rateLimit = _RateLimits.GetOrAdd(Context.Guild.Id, new RateLimit());
			var headers = resp.Headers;
			rateLimit.Time = (new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(Convert.ToInt64(resp.Headers["X-RateLimit-Reset"]))).ToUniversalTime();
			rateLimit.Messages = Convert.ToInt32(resp.Headers["X-RateLimit-Remaining"]);
		}

		private struct RateLimit
		{
			public int Messages { get; set; }
			public DateTime Time { get; set; }
		}
	}
}
