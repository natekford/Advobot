using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.ImageResizing;
using Advobot.Classes.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;

namespace Advobot.CommandMarking
{
	public sealed class Webhooks : ModuleBase
	{
		[Group(nameof(DisplayWebhooks)), ModuleInitialismAlias(typeof(DisplayWebhooks))]
		[Summary("Lists all the webhooks on the guild or the specified channel.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class DisplayWebhooks : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
				=> Responses.Webhooks.DisplayWebhooks(Context.Guild, await Context.Guild.GetWebhooksAsync().CAF());
			[Command]
			public async Task<RuntimeResult> Command(SocketTextChannel channel)
				=> Responses.Webhooks.DisplayWebhooks(channel, await channel.GetWebhooksAsync().CAF());
		}

		[Group(nameof(CreateWebhook)), ModuleInitialismAlias(typeof(CreateWebhook))]
		[Summary("Creates a webhook for the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class CreateWebhook : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateTextChannel(ChannelPermission.ManageWebhooks, FromContext = true)] SocketTextChannel channel,
				[Remainder, ValidateUsername] string name)
			{
				var webhook = await channel.CreateWebhookAsync(name, options: GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(webhook);
			}
		}

		[Group(nameof(DeleteWebhook)), ModuleInitialismAlias(typeof(DeleteWebhook))]
		[Summary("Deletes a webhook from the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class DeleteWebhook : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(IWebhook webhook)
			{
				await webhook.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(webhook);
			}
		}

		[Group(nameof(ModifyWebhookName)), ModuleInitialismAlias(typeof(ModifyWebhookName))]
		[Summary("Changes the name of a webhook.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(IWebhook webhook, [Remainder, ValidateUsername] string name)
			{
				await webhook.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(webhook, name);
			}
		}

		[Group(nameof(ModifyWebhookChannel)), ModuleInitialismAlias(typeof(ModifyWebhookChannel))]
		[Summary("Changes the channel of a webhook.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(IWebhook webhook,
				[ValidateTextChannel(ChannelPermission.ManageWebhooks, FromContext = true)] SocketTextChannel channel)
			{
				await webhook.ModifyAsync(x => x.Channel = Optional.Create<ITextChannel>(channel), GenerateRequestOptions()).CAF();
				return Responses.Webhooks.ModifiedChannel(webhook, channel);
			}
		}

		[Group(nameof(ModifyWebhookIcon)), ModuleInitialismAlias(typeof(ModifyWebhookIcon))]
		[Summary("Changes the icon of a webhook.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookIcon : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(IWebhook webhook, Uri url)
			{
				var position = Enqueue(new IconCreationArgs("Webhook Icon", Context, url, default,
					(ctx, ms) => webhook.ModifyAsync(x => x.Image = new Image(ms), ctx.GenerateRequestOptions())));
				return Responses.Snowflakes.EnqueuedIcon(webhook, position);
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Remove(IWebhook webhook)
			{
				await webhook.ModifyAsync(x => x.Image = new Image(), GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.RemovedIcon(webhook);
			}
		}

		[Group(nameof(SendMessageThroughWebhook)), ModuleInitialismAlias(typeof(SendMessageThroughWebhook))]
		[Summary("Sends a message through a webhook. Use this command if you're annoying.")]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(false)]
		public sealed class SendMessageThroughWebhook : AdvobotModuleBase
		{
			private static readonly ConcurrentDictionary<ulong, ulong> _GuildsToWebhooks = new ConcurrentDictionary<ulong, ulong>();
			private static readonly ConcurrentDictionary<ulong, DiscordWebhookClient> _Clients = new ConcurrentDictionary<ulong, DiscordWebhookClient>();

			[Command(RunMode = RunMode.Async)]
			public Task Command(IWebhook webhook, [Remainder] string text)
			{
				var webhookId = _GuildsToWebhooks.AddOrUpdate(Context.Guild.Id, webhook.Id, (k, v) =>
				{
					//If the most recently used webhook does not match the id of the supplied one, remove that client
					if (v != webhook.Id)
					{
						_Clients.TryRemove(v, out var removed);
						removed.Dispose();
					}
					return webhook.Id;
				});
				//If the client already exists, use that, otherwise create a new client
				return _Clients.GetOrAdd(webhookId, _ => new DiscordWebhookClient(webhook)).SendMessageAsync(text);
			}
		}
	}
}