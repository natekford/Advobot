using System;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions;
using Advobot.Modules;
using Advobot.Services.ImageResizing;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands
{
	public sealed class Client : ModuleBase
	{
		[Group(nameof(ModifyBotName)), ModuleInitialismAlias(typeof(ModifyBotName))]
		[LocalizedSummary(nameof(Summaries.ModifyBotName))]
		[CommandMeta("6882dc55-3557-4366-8c4c-2954b46cfb2b", IsEnabled = true)]
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

		[Group(nameof(ModifyBotIcon)), ModuleInitialismAlias(typeof(ModifyBotIcon))]
		[LocalizedSummary(nameof(Summaries.ModifyBotIcon))]
		[CommandMeta("096006e2-da07-4935-ab35-4e5099663da9", IsEnabled = true)]
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
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Remove()
			{
				await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image()).CAF();
				return Responses.Snowflakes.RemovedIcon(Context.Client.CurrentUser);
			}
		}

		[Group(nameof(DisconnectBot)), ModuleInitialismAlias(new[] { "runescapeservers" }, typeof(DisconnectBot))]
		[LocalizedSummary(nameof(Summaries.DisconnectBot))]
		[CommandMeta("10f3bf15-0652-4bd7-a29f-630136d0164a", IsEnabled = true)]
		[RequireBotOwner]
		public sealed class DisconnectBot : AdvobotModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command()
				=> Context.Client.DisconnectBotAsync();
		}

		[Group(nameof(RestartBot)), ModuleInitialismAlias(typeof(RestartBot))]
		[LocalizedSummary(nameof(Summaries.RestartBot))]
		[CommandMeta("ca7caf70-9f40-4931-a99a-96f667edda16", IsEnabled = true)]
		[RequireBotOwner]
		public sealed class RestartBot : AdvobotModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public Task Command()
				=> Context.Client.RestartBotAsync(BotSettings);
		}
	}
}