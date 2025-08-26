using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;
using Discord.Commands;
using Discord.Webhook;

using System.Collections.Concurrent;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands;

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
			var webhook = await channel.CreateWebhookAsync(name, options: GetOptions()).ConfigureAwait(false);
			return Responses.Snowflakes.Created(webhook);
		}
	}

	[LocalizedGroup(nameof(Groups.SpeakThroughWebhook))]
	[LocalizedAlias(nameof(Aliases.SpeakThroughWebhook))]
	[LocalizedSummary(nameof(Summaries.SpeakThroughWebhook))]
	[Meta("d830df02-b33b-4e95-88d7-8acb029506f6")]
	[RequireGuildPermissions(GuildPermission.ManageWebhooks)]
	public sealed class SpeakThroughWebhook : AdvobotModuleBase
	{
		private static readonly ConcurrentDictionary<ulong, DiscordWebhookClient> _Clients = new();

		[Command(RunMode = RunMode.Async)]
		public Task Command(
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