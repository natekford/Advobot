using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Modules;
using Advobot.Services.ImageResizing;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands
{
	public sealed class Webhooks : ModuleBase
	{
		[Group(nameof(DisplayWebhooks)), ModuleInitialismAlias(typeof(DisplayWebhooks))]
		[LocalizedSummary(nameof(Summaries.DisplayWebhooks))]
		[CommandMeta("b8e90320-b827-4b61-81ea-92d43ea1ba6e", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
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
		[CommandMeta("a177bff8-5ade-4c21-8e6a-97a254c26331", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class CreateWebhook : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageWebhooks)] ITextChannel channel,
				[Remainder, Username] string name)
			{
				var webhook = await channel.CreateWebhookAsync(name, options: GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(webhook);
			}
		}

		[Group(nameof(DeleteWebhook)), ModuleInitialismAlias(typeof(DeleteWebhook))]
		[LocalizedSummary(nameof(Summaries.DeleteWebhook))]
		[CommandMeta("8fb67520-b0b2-4d77-8588-0b9924b767c0", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
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
		[CommandMeta("953dd979-c51a-4a1b-b4ba-05576faf11c2", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class ModifyWebhookName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IWebhook webhook,
				[Remainder, Username] string name)
			{
				await webhook.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(webhook, name);
			}
		}

		[Group(nameof(ModifyWebhookChannel)), ModuleInitialismAlias(typeof(ModifyWebhookChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyWebhookChannel))]
		[CommandMeta("082ca529-66b7-4c39-ade2-3f2501778070", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class ModifyWebhookChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IWebhook webhook,
				[Channel(ManageWebhooks)] ITextChannel channel)
			{
				await webhook.ModifyAsync(x => x.Channel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Webhooks.ModifiedChannel(webhook, channel);
			}
		}

		[Group(nameof(ModifyWebhookIcon)), ModuleInitialismAlias(typeof(ModifyWebhookIcon))]
		[LocalizedSummary(nameof(Summaries.ModifyWebhookIcon))]
		[CommandMeta("bcfae3ac-2e52-4151-b692-738ed7297bab", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
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

		[Group(nameof(SpeakThroughWebhook)), ModuleInitialismAlias(typeof(SpeakThroughWebhook))]
		[LocalizedSummary(nameof(Summaries.SpeakThroughWebhook))]
		[CommandMeta("d830df02-b33b-4e95-88d7-8acb029506f6")]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class SpeakThroughWebhook : AdvobotModuleBase
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