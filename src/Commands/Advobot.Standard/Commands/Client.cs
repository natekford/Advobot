using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions;
using Advobot.Resources;
using Advobot.Utilities;

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
		public async Task Command()
		{
			await Context.Client.StopAsync().ConfigureAwait(false);
			Environment.Exit(0);
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
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = name).ConfigureAwait(false);
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
			=> Context.Client.RestartBotAsync(BotConfig);
	}
}