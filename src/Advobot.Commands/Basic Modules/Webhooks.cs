using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.ImageResizing;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.Webhook;

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
			await MessageUtils.SendMessageAsync(Context.Channel, string.Join("\n\n", parts)).CAF();
		}

		private string FormatWebhooks(ITextChannel channel, IEnumerable<IWebhook> webhooks)
			=> $"**{channel.Format()}**:\n{string.Join("\n", webhooks.Select(x => $"`{x.Format()}`"))}";
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
		public async Task Command(IWebhook webhook, [Remainder, ValidateString(Target.Name)] string name)
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
		public async Task Command(IWebhook webhook, [ValidateObject(true, Verif.CanManageWebhooks)] ITextChannel channel)
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
	[Summary("Sends a message through a webhook. Use this command if you're annoying.")]
	[PermissionRequirement(new[] { GuildPermission.ManageWebhooks }, null)]
	[DefaultEnabled(false)]
	public sealed class SendMessageThroughWebhook : AdvobotModuleBase
	{
		private static readonly ConcurrentDictionary<ulong, ulong> _GuildsToWebhooks = new ConcurrentDictionary<ulong, ulong>();
		private static readonly ConcurrentDictionary<ulong, DiscordWebhookClient> _Clients = new ConcurrentDictionary<ulong, DiscordWebhookClient>();

		[Command(RunMode = RunMode.Async)]
		public async Task Command(IWebhook webhook, [Remainder] string text)
		{
			var webhookId = _GuildsToWebhooks.AddOrUpdate(Context.Guild.Id, webhook.Id, (k, v) =>
			{
				//If the most recently used webhook does not match the id of the supplied one, remove that client
				if (v != webhook.Id)
				{
					_Clients.TryRemove(v, out var removed);
				}
				return webhook.Id;
			});
			//If the client already exists, use that, otherwise create a new client
			await _Clients.GetOrAdd(webhookId, _ => new DiscordWebhookClient(webhook)).SendMessageAsync(text).CAF();
		}
	}
}