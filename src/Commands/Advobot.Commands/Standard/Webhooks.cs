using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.StringLengthValidation;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.Services.ImageResizing;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using static Discord.ChannelPermission;

namespace Advobot.Commands.Standard
{
	public sealed class Webhooks : ModuleBase
	{
		[Group(nameof(DisplayWebhooks)), ModuleInitialismAlias(typeof(DisplayWebhooks))]
		[LocalizedSummary(nameof(Summaries.DisplayWebhooks))]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class DisplayWebhooks : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
			{
				var webhooks = await Context.Guild.GetWebhooksAsync().CAF();
				return Responses.Webhooks.DisplayWebhooks(Context.Guild, webhooks);
			}
			[Command]
			public async Task<RuntimeResult> Command(ITextChannel channel)
			{
				var webhooks = await channel.GetWebhooksAsync().CAF();
				return Responses.Webhooks.DisplayWebhooks(channel, webhooks);
			}
		}

		[Group(nameof(CreateWebhook)), ModuleInitialismAlias(typeof(CreateWebhook))]
		[LocalizedSummary(nameof(Summaries.CreateWebhook))]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class CreateWebhook : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[ValidateTextChannel(ManageWebhooks, FromContext = true)] ITextChannel channel,
				[Remainder, ValidateUsername] string name)
			{
				var webhook = await channel.CreateWebhookAsync(name, options: GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(webhook);
			}
		}

		[Group(nameof(DeleteWebhook)), ModuleInitialismAlias(typeof(DeleteWebhook))]
		[LocalizedSummary(nameof(Summaries.DeleteWebhook))]
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
		[LocalizedSummary(nameof(Summaries.ModifyWebhookName))]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IWebhook webhook,
				[Remainder, ValidateUsername] string name)
			{
				await webhook.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(webhook, name);
			}
		}

		[Group(nameof(ModifyWebhookChannel)), ModuleInitialismAlias(typeof(ModifyWebhookChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyWebhookChannel))]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IWebhook webhook,
				[ValidateTextChannel(ManageWebhooks, FromContext = true)] ITextChannel channel)
			{
				await webhook.ModifyAsync(x => x.Channel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Webhooks.ModifiedChannel(webhook, channel);
			}
		}

		[Group(nameof(ModifyWebhookIcon)), ModuleInitialismAlias(typeof(ModifyWebhookIcon))]
		[LocalizedSummary(nameof(Summaries.ModifyWebhookIcon))]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(true)]
		public sealed class ModifyWebhookIcon : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(IWebhook webhook, Uri url)
			{
				var position = Enqueue(new IconCreationContext(Context, url, default, "Webhook Icon",
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
		[LocalizedSummary(nameof(Summaries.SendMessageThroughWebhook))]
		[UserPermissionRequirement(GuildPermission.ManageWebhooks)]
		[EnabledByDefault(false)]
		public sealed class SendMessageThroughWebhook : AdvobotModuleBase
		{
			private static readonly ConcurrentDictionary<ulong, ulong> _GuildsToWebhooks
				= new ConcurrentDictionary<ulong, ulong>();
			private static readonly ConcurrentDictionary<ulong, DiscordWebhookClient> _Clients
				= new ConcurrentDictionary<ulong, DiscordWebhookClient>();

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