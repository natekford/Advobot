using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Services.GuildSettings;
using Advobot.Services.Help;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

using static Advobot.Standard.Responses.Misc;

namespace Advobot.Standard.Commands;

[Category(nameof(Misc))]
public sealed class Misc : ModuleBase
{
	[LocalizedGroup(nameof(Groups.Commands))]
	[LocalizedAlias(nameof(Aliases.Commands))]
	[LocalizedSummary(nameof(Summaries.Commands))]
	[Meta("ec0f7aef-85d6-4251-9c8e-7c70890f455e", IsEnabled = true, CanToggle = false)]
	public sealed class Commands : AdvobotModuleBase
	{
		public required IGuildSettingsService GuildSettings { get; set; }
		public required IHelpService HelpEntries { get; set; }

		[Command]
		public async Task<RuntimeResult> Command()
		{
			var prefix = await GuildSettings.GetPrefixAsync(Context.Guild).ConfigureAwait(false);
			return GeneralCommandInfo(HelpEntries.GetCategories(), prefix);
		}

		[Command]
		public Task<RuntimeResult> Command(Category category)
		{
			var entries = HelpEntries.GetHelpModules(category.Name);
			return CategoryCommands(entries, category.Name);
		}
	}

	[LocalizedGroup(nameof(Groups.Help))]
	[LocalizedAlias(nameof(Aliases.Help))]
	[LocalizedSummary(nameof(Summaries.Help))]
	[Meta("0e89a6fd-5c9c-4008-a912-7c719ea7827d", IsEnabled = true, CanToggle = false)]
	public sealed class Help : AdvobotModuleBase
	{
		public required IGuildSettingsService GuildSettings { get; set; }

		[Command]
		[LocalizedSummary(nameof(Summaries.HelpGeneralHelp))]
		public async Task<RuntimeResult> Command()
		{
			var prefix = await GuildSettings.GetPrefixAsync(Context.Guild).ConfigureAwait(false);
			return GeneralHelp(prefix);
		}

		[Command, Priority(1)]
		[LocalizedSummary(nameof(Summaries.HelpModuleHelp))]
		public Task<RuntimeResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommand))]
			[LocalizedName(nameof(Parameters.Command))]
			[Remainder]
			IHelpModule helpEntry
		) => Responses.Misc.Help(helpEntry);

		[Command, Priority(2)]
		[LocalizedSummary(nameof(Summaries.HelpCommandHelp))]
		public Task<RuntimeResult> Command(
			[LocalizedSummary(nameof(Summaries.HelpVariableCommandPosition))]
			[LocalizedName(nameof(Parameters.Position))]
			[Positive]
			int position,
			[LocalizedSummary(nameof(Summaries.HelpVariableExactCommand))]
			[LocalizedName(nameof(Parameters.Command))]
			[Remainder]
			IHelpModule helpEntry
		) => Responses.Misc.Help(helpEntry, position);

		[Command(RunMode = RunMode.Async), Priority(0)]
		[Hidden]
		public async Task<RuntimeResult> Command(
			[Remainder]
			IReadOnlyList<IHelpModule> helpEntries
		)
		{
			var entry = await NextItemAtIndexAsync(helpEntries, x => x.Name).ConfigureAwait(false);
			if (entry.HasValue)
			{
				return Responses.Misc.Help(entry.Value);
			}
			return AdvobotResult.IgnoreFailure;
		}
	}

	[LocalizedGroup(nameof(Groups.MessageBotOwner))]
	[LocalizedAlias(nameof(Aliases.MessageBotOwner))]
	[LocalizedSummary(nameof(Summaries.MessageBotOwner))]
	[Meta("3562f937-4d3c-46aa-afda-70e04040be53", IsEnabled = false)]
	[RequireGenericGuildPermissions]
	[RequireAllowedToDmBotOwner]
	public sealed class MessageBotOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] string message)
		{
			var owner = (await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner;
			var cut = message[..Math.Min(message.Length, 250)];
			var text = $"`{Context.User.Format()}` - `{Context.Guild.Format()}`:\n```\n{cut}```";
			await owner.SendMessageAsync(text).ConfigureAwait(false);
		}
	}

	[LocalizedGroup(nameof(Groups.MessageRole))]
	[LocalizedAlias(nameof(Aliases.MessageRole))]
	[LocalizedSummary(nameof(Summaries.MessageRole))]
	[Meta("db524980-4a8e-4933-aa9b-527094d60165", IsEnabled = false)]
	[RequireGenericGuildPermissions]
	public sealed class MessageRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			[CanModifyRole, NotEveryone, NotMentionable] IRole role,
			[Remainder] string message)
		{
			var cut = message[..Math.Min(message.Length, 250)];
			var text = $"From `{Context.User.Format()}`, {role.Mention}: {cut}";
			await role.ModifyAsync(x => x.Mentionable = true, GetOptions()).ConfigureAwait(false);
			await ReplyAsync(text).ConfigureAwait(false);
			await role.ModifyAsync(x => x.Mentionable = false, GetOptions()).ConfigureAwait(false);
		}
	}

	[LocalizedGroup(nameof(Groups.Test))]
	[LocalizedAlias(nameof(Aliases.Test))]
	[LocalizedSummary(nameof(Summaries.Test))]
	[Meta("6c0b693e-e3ac-421e-910e-3178110d791d", IsEnabled = true)]
	[RequireBotOwner]
	public sealed class Test : AdvobotModuleBase
	{
		[Command]
		public Task Command()
			=> Task.CompletedTask;
	}
}