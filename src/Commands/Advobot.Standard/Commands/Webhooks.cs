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
public sealed class Webhooks : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.CreateWebhook), nameof(Names.CreateWebhookAlias))]
	[LocalizedSummary(nameof(Summaries.CreateWebhook))]
	[Meta("a177bff8-5ade-4c21-8e6a-97a254c26331", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
	public sealed class CreateWebhook : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Create(
			[CanModifyChannel(ChannelPermission.ManageWebhooks)]
			[LocalizedSummary(nameof(Summaries.CreateWebhookChannel))]
			ITextChannel channel,
			[Remainder]
			[Username]
			[LocalizedSummary(nameof(Summaries.CreateWebhookName))]
			string name
		)
		{
			var webhook = await channel.CreateWebhookAsync(name, options: GetOptions()).ConfigureAwait(false);
			return Responses.Snowflakes.Created(webhook);
		}
	}

	[LocalizedCommand(nameof(Names.SpeakThroughWebhook), nameof(Names.SpeakThroughWebhookAlias))]
	[LocalizedSummary(nameof(Summaries.SpeakThroughWebhook))]
	[Id("d830df02-b33b-4e95-88d7-8acb029506f6")]
	[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
	public sealed class SpeakThroughWebhook : AdvobotModuleBase
	{
		private static readonly ConcurrentDictionary<ulong, DiscordWebhookClient> _Clients = new();

		[Command]
		public Task Speak(
			[LocalizedSummary(nameof(Summaries.SpeakThroughWebhookWebhook))]
			IWebhook webhook,
			[Remainder]
			[LocalizedSummary(nameof(Summaries.SpeakThroughWebhookText))]
			string text
		)
		{
			var client = _Clients.GetOrAdd(webhook.Id, _ => new(webhook));
			return client.SendMessageAsync(text);
		}
	}
}