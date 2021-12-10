using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Resources;
using Advobot.Services.ImageResizing;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands;

[Category(nameof(Client))]
public sealed class Client : ModuleBase
{
	[LocalizedGroup(nameof(Groups.DisconnectBot))]
	[LocalizedAlias(nameof(Aliases.DisconnectBot), nameof(Aliases.RunescapeServers))]
	[LocalizedSummary(nameof(Summaries.DisconnectBot))]
	[Meta("10f3bf15-0652-4bd7-a29f-630136d0164a", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class DisconnectBot : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public Task Command()
			=> Context.Client.DisconnectBotAsync();
	}

	[LocalizedGroup(nameof(Groups.ModifyBotIcon))]
	[LocalizedAlias(nameof(Aliases.ModifyBotIcon))]
	[LocalizedSummary(nameof(Summaries.ModifyBotIcon))]
	[Meta("096006e2-da07-4935-ab35-4e5099663da9", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class ModifyBotIcon : ImageResizerModule
	{
		[Command]
		public Task<RuntimeResult> Command(Uri url)
		{
			var position = Enqueue(new IconCreationContext(Context, url, default, "Bot Icon",
				(ctx, ms) => ctx.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(ms), ctx.GenerateRequestOptions())));
			return Responses.Snowflakes.EnqueuedIcon(Context.Client.CurrentUser, position);
		}

		[LocalizedCommand(nameof(Groups.Remove))]
		[LocalizedAlias(nameof(Aliases.Remove))]
		public async Task<RuntimeResult> Remove()
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image()).CAF();
			return Responses.Snowflakes.RemovedIcon(Context.Client.CurrentUser);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyBotName))]
	[LocalizedAlias(nameof(Aliases.ModifyBotName))]
	[LocalizedSummary(nameof(Summaries.ModifyBotName))]
	[Meta("6882dc55-3557-4366-8c4c-2954b46cfb2b", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class ModifyBotName : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command([Remainder, Username] string name)
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = name).CAF();
			return Responses.Snowflakes.ModifiedName(Context.Client.CurrentUser, name);
		}
	}

	[LocalizedGroup(nameof(Groups.RestartBot))]
	[LocalizedAlias(nameof(Aliases.RestartBot))]
	[LocalizedSummary(nameof(Summaries.RestartBot))]
	[Meta("ca7caf70-9f40-4931-a99a-96f667edda16", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class RestartBot : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public Task Command()
			=> Context.Client.RestartBotAsync(BotSettings);
	}
}