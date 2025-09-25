using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;
using Discord.Webhook;

using System.Collections.Concurrent;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Names.WebhooksCategory))]
public sealed class Webhooks
{
	[Command(nameof(Names.CreateWebhook), nameof(Names.CreateWebhookAlias))]
	[LocalizedSummary(nameof(Summaries.CreateWebhookSummary))]
	[Meta("a177bff8-5ade-4c21-8e6a-97a254c26331", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
	public sealed class CreateWebhook : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Create(
			[CanModifyChannel(ChannelPermission.ManageWebhooks)]
			[LocalizedSummary(nameof(Summaries.CreateWebhookChannelSummary))]
			ITextChannel channel,
			[Remainder]
			[Username]
			[LocalizedSummary(nameof(Summaries.CreateWebhookNameSummary))]
			string name
		)
		{
			var webhook = await channel.CreateWebhookAsync(name, options: GetOptions()).ConfigureAwait(false);
			return Responses.Snowflakes.Created(webhook);
		}
	}

	[Command(nameof(Names.SpeakThroughWebhook), nameof(Names.SpeakThroughWebhookAlias))]
	[LocalizedSummary(nameof(Summaries.SpeakThroughWebhookSummary))]
	[Id("d830df02-b33b-4e95-88d7-8acb029506f6")]
	[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
	public sealed class SpeakThroughWebhook : AdvobotModuleBase
	{
		private static readonly ConcurrentDictionary<ulong, DiscordWebhookClient> _Clients = new();

		[Command]
		public Task Speak(
			[LocalizedSummary(nameof(Summaries.SpeakThroughWebhookWebhookSummary))]
			IWebhook webhook,
			[Remainder]
			[LocalizedSummary(nameof(Summaries.SpeakThroughWebhookTextSummary))]
			string text
		)
		{
			var client = _Clients.GetOrAdd(webhook.Id, _ => new(webhook));
			return client.SendMessageAsync(text);
		}
	}
}