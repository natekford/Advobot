using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.ImageResizing;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands
{
	[Group]
	public sealed class Webhooks : ModuleBase
	{
		[Group(nameof(GetWebhooks)), TopLevelShortAlias(typeof(GetWebhooks))]
		[Summary("Lists all the webhooks on the guild or the specified channel.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class GetWebhooks : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
				=> await ReplyIfAny(await Context.Guild.GetWebhooksAsync().CAF(), Context.Guild, "Webhooks", x => x.Format()).CAF();
			[Command]
			public async Task Command(ITextChannel channel)
				=> await ReplyIfAny(await channel.GetWebhooksAsync().CAF(), channel, "Webhooks", x => x.Format()).CAF();
		}

		[Group(nameof(DeleteWebhook)), TopLevelShortAlias(typeof(DeleteWebhook))]
		[Summary("Deletes a webhook from the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class DeleteWebhook : AdvobotModuleBase
		{
			[Command]
			public async Task Command(IWebhook webhook)
			{
				await webhook.DeleteAsync(GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully deleted the webhook `{webhook.Format()}`.").CAF();
			}
		}

		[Group(nameof(ModifyWebhookName)), TopLevelShortAlias(typeof(ModifyWebhookName))]
		[Summary("Changes the name of a webhook.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookName : AdvobotModuleBase
		{
			[Command]
			public async Task Command(IWebhook webhook, [Remainder, ValidateUsername] string name)
			{
				await webhook.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully changed the name of `{webhook.Format()}` to `{name}`.").CAF();
			}
		}

		[Group(nameof(ModifyWebhookChannel)), TopLevelShortAlias(typeof(ModifyWebhookChannel))]
		[Summary("Changes the channel of a webhook.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookChannel : AdvobotModuleBase
		{
			[Command]
			public async Task Command(
				IWebhook webhook,
				[ValidateTextChannel(CPerm.ManageWebhooks, FromContext = true)] SocketTextChannel channel)
			{
				await webhook.ModifyAsync(x => x.Channel = Optional.Create<ITextChannel>(channel), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully set the channel of `{webhook.Format()}` to `{channel.Format()}`.").CAF();
			}
		}

		[Group(nameof(ModifyWebhookIcon)), TopLevelShortAlias(typeof(ModifyWebhookIcon))]
		[Summary("Changes the icon of a webhook.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookIcon : ImageResizerModule
		{
			[Command]
			public async Task Command(IWebhook webhook, Uri url)
			{
				await ProcessAsync(new IconCreationArgs("Webhook Icon", Context, url, default, async (ctx, ms) =>
				{
					await webhook.ModifyAsync(x => x.Image = new Image(ms), ctx.GenerateRequestOptions()).CAF();
				})).CAF();
			}
			[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
			public async Task Remove(IWebhook webhook)
			{
				await webhook.ModifyAsync(x => x.Image = new Image(), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully removed the webhook icon from {webhook.Format()}.").CAF();
			}
		}

		[Group(nameof(SendMessageThroughWebhook)), TopLevelShortAlias(typeof(SendMessageThroughWebhook))]
		[Summary("Sends a message through a webhook. Use this command if you're annoying.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(false)]
		public sealed class SendMessageThroughWebhook : AdvobotModuleBase
		{
#warning make into service
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
}