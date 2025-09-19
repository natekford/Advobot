using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions;
using Advobot.Resources;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Names.ClientCategory))]
public sealed class Client : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.DisconnectBot), nameof(Names.DisconnectBot), nameof(Names.RunescapeServersAlias))]
	[LocalizedSummary(nameof(Summaries.DisconnectBot))]
	[Id("10f3bf15-0652-4bd7-a29f-630136d0164a")]
	[Meta(IsEnabled = true)]
	[RequireBotOwner]
	public sealed class DisconnectBot : AdvobotModuleBase
	{
		[InjectService]
		public required ShutdownApplication Exit { get; set; }

		[Command]
		public async Task Shutdown()
		{
			try
			{
				await Context.Channel.SendMessageAsync("Shutting down...").ConfigureAwait(false);
				await Context.Client.StopAsync().ConfigureAwait(false);
			}
			finally
			{
				Exit.Invoke(0);
			}
		}
	}

	[LocalizedCommand(nameof(Names.ModifyBotName), nameof(Names.ModifyBotNameAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyBotName))]
	[Id("6882dc55-3557-4366-8c4c-2954b46cfb2b")]
	[Meta(IsEnabled = true)]
	[RequireBotOwner]
	public sealed class ModifyBotName : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Modify(
			[Remainder]
			[Username]
			string name)
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = name).ConfigureAwait(false);
			return Responses.Snowflakes.ModifiedName(Context.Client.CurrentUser, name);
		}
	}
}