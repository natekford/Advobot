using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Resources;
using Advobot.Services.ImageResizing;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;
using Discord.Webhook;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands
{
	[Category(nameof(Webhooks))]
	public sealed class Webhooks : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.CreateWebhook))]
		[LocalizedAlias(nameof(Aliases.CreateWebhook))]
		[LocalizedSummary(nameof(Summaries.CreateWebhook))]
		[Meta("a177bff8-5ade-4c21-8e6a-97a254c26331", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class CreateWebhook : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyChannel(ManageWebhooks)]
				[LocalizedSummary(nameof(Summaries.CreateWebhookChannel))]
				ITextChannel channel,
				[Remainder, Username]
				[LocalizedSummary(nameof(Summaries.CreateWebhookName))]
				string name
			)
			{
				var webhook = await channel.CreateWebhookAsync(name, options: GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(webhook);
			}
		}

		[LocalizedGroup(nameof(Groups.DeleteWebhook))]
		[LocalizedAlias(nameof(Aliases.DeleteWebhook))]
		[LocalizedSummary(nameof(Summaries.DeleteWebhook))]
		[Meta("8fb67520-b0b2-4d77-8588-0b9924b767c0", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class DeleteWebhook : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[LocalizedSummary(nameof(Summaries.DeleteWebhookWebhook))]
				IWebhook webhook
			)
			{
				await webhook.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(webhook);
			}
		}

		[LocalizedGroup(nameof(Groups.DisplayWebhooks))]
		[LocalizedAlias(nameof(Aliases.DisplayWebhooks))]
		[LocalizedSummary(nameof(Summaries.DisplayWebhooks))]
		[Meta("b8e90320-b827-4b61-81ea-92d43ea1ba6e", IsEnabled = true)]
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
			public async Task<RuntimeResult> Command(
				[LocalizedSummary(nameof(Summaries.DisplayWebhooksChannel))]
				ITextChannel channel
			)
			{
				var webhooks = await channel.GetWebhooksAsync().CAF();
				return Responses.Webhooks.DisplayWebhooks(channel, webhooks);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyWebhookChannel))]
		[LocalizedAlias(nameof(Aliases.ModifyWebhookChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyWebhookChannel))]
		[Meta("082ca529-66b7-4c39-ade2-3f2501778070", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class ModifyWebhookChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[LocalizedSummary(nameof(Summaries.ModifyWebhookChannelWebhook))]
				IWebhook webhook,
				[CanModifyChannel(ManageWebhooks)]
				[LocalizedSummary(nameof(Summaries.ModifyWebhookChannelChannel))]
				ITextChannel channel
			)
			{
				await webhook.ModifyAsync(x => x.Channel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Webhooks.ModifiedChannel(webhook, channel);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyWebhookIcon))]
		[LocalizedAlias(nameof(Aliases.ModifyWebhookIcon))]
		[LocalizedSummary(nameof(Summaries.ModifyWebhookIcon))]
		[Meta("bcfae3ac-2e52-4151-b692-738ed7297bab", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class ModifyWebhookIcon : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(
				[LocalizedSummary(nameof(Summaries.ModifyWebhookIconWebhook))]
				IWebhook webhook,
				[LocalizedSummary(nameof(Summaries.ModifyWebhookIconUrl))]
				Uri url
			)
			{
				var position = Enqueue(new IconCreationContext(Context, url, default, "Webhook Icon",
					(ctx, ms) => webhook.ModifyAsync(x => x.Image = new Image(ms), ctx.GenerateRequestOptions())));
				return Responses.Snowflakes.EnqueuedIcon(webhook, position);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove(
				[LocalizedSummary(nameof(Summaries.ModifyWebhookIconWebhook))]
				IWebhook webhook
			)
			{
				await webhook.ModifyAsync(x => x.Image = new Image(), GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.RemovedIcon(webhook);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyWebhookName))]
		[LocalizedAlias(nameof(Aliases.ModifyWebhookName))]
		[LocalizedSummary(nameof(Summaries.ModifyWebhookName))]
		[Meta("953dd979-c51a-4a1b-b4ba-05576faf11c2", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class ModifyWebhookName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[LocalizedSummary(nameof(Summaries.ModifyWebhookNameWebhook))]
				IWebhook webhook,
				[Remainder, Username]
				[LocalizedSummary(nameof(Summaries.ModifyWebhookNameName))]
				string name
			)
			{
				await webhook.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(webhook, name);
			}
		}

		[LocalizedGroup(nameof(Groups.SpeakThroughWebhook))]
		[LocalizedAlias(nameof(Aliases.SpeakThroughWebhook))]
		[LocalizedSummary(nameof(Summaries.SpeakThroughWebhook))]
		[Meta("d830df02-b33b-4e95-88d7-8acb029506f6")]
		[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
		public sealed class SpeakThroughWebhook : AdvobotModuleBase
		{
			private static readonly ConcurrentDictionary<ulong, DiscordWebhookClient> _Clients
				= new ConcurrentDictionary<ulong, DiscordWebhookClient>();

			private static readonly ConcurrentDictionary<ulong, ulong> _GuildsToWebhooks
				= new ConcurrentDictionary<ulong, ulong>();

			[Command(RunMode = RunMode.Async)]
			public Task Command(
				[LocalizedSummary(nameof(Summaries.SpeakThroughWebhookWebhook))]
				IWebhook webhook,
				[Remainder]
				[LocalizedSummary(nameof(Summaries.SpeakThroughWebhookText))]
				string text
			)
			{
				var webhookId = _GuildsToWebhooks.AddOrUpdate(Context.Guild.Id, webhook.Id, (_, v) =>
				{
					//If the most recently used webhook does not match the id of the supplied one, remove that client
					if (v != webhook.Id && _Clients.TryRemove(v, out var removed))
					{
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