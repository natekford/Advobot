using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.HelpEntries;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using Advobot.TypeReaders;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands
{
	[Category(nameof(Misc))]
	public sealed class Misc : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.Help))]
		[LocalizedAlias(nameof(Aliases.Help))]
		[LocalizedSummary(nameof(Summaries.Help))]
		[Meta("0e89a6fd-5c9c-4008-a912-7c719ea7827d", IsEnabled = true, CanToggle = false)]
		public sealed class Help : AdvobotModuleBase
		{
			[Command]
			[LocalizedSummary(nameof(Summaries.HelpGeneralHelp))]
			public Task<RuntimeResult> CommandAsync()
				=> Responses.Misc.GeneralHelp(Context.Settings.GetPrefix(BotSettings));

			[Command, Priority(1)]
			[LocalizedSummary(nameof(Summaries.HelpModuleHelp))]
			public Task<RuntimeResult> CommandAsync(
				[LocalizedSummary(nameof(Summaries.HelpVariableCommand))]
				[LocalizedName(nameof(Names.Command))]
				[Remainder]
				IModuleHelpEntry helpEntry
			) => Responses.Misc.Help(helpEntry, Context.Settings);

			[Command, Priority(2)]
			[LocalizedSummary(nameof(Summaries.HelpCommandHelp))]
			public Task<RuntimeResult> CommandAsync(
				[LocalizedSummary(nameof(Summaries.HelpVariableExactCommand))]
				[LocalizedName(nameof(Names.Command))]
				IModuleHelpEntry helpEntry,
				[LocalizedSummary(nameof(Summaries.HelpVariableCommandPosition))]
				[LocalizedName(nameof(Names.Position))]
				[Positive]
				int position
			) => Responses.Misc.Help(helpEntry, position - 1);

			[Command(RunMode = RunMode.Async), Priority(0)]
			[Hidden]
			public async Task<RuntimeResult> CommandAsync(
				[OverrideTypeReader(typeof(CloseHelpEntryTypeReader))]
				[Remainder]
				IEnumerable<IModuleHelpEntry> helpEntries
			)
			{
				var entry = await NextItemAtIndexAsync(helpEntries.ToArray(), x => x.Name).CAF();
				if (entry.HasValue)
				{
					return Responses.Misc.Help(entry.Value, Context.Settings);
				}
				return AdvobotResult.IgnoreFailure;
			}
		}

		[LocalizedGroup(nameof(Groups.Commands))]
		[LocalizedAlias(nameof(Aliases.Commands))]
		[LocalizedSummary(nameof(Summaries.Commands))]
		[Meta("ec0f7aef-85d6-4251-9c8e-7c70890f455e", IsEnabled = true, CanToggle = false)]
		public sealed class Commands : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IHelpEntryService HelpEntries { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[Command]
			public Task<RuntimeResult> CommandAsync()
				=> Responses.Misc.GeneralCommandInfo(HelpEntries.GetCategories(), Prefix);

			[Command]
			public Task<RuntimeResult> CommandAsync([CommandCategory] string category)
				=> Responses.Misc.CategoryCommands(HelpEntries.GetHelpEntries(category), category);
		}

		[LocalizedGroup(nameof(Groups.MakeAnEmbed))]
		[LocalizedAlias(nameof(Aliases.MakeAnEmbed))]
		[LocalizedSummary(nameof(Summaries.MakeAnEmbed))]
		[Meta("6acf2d14-b251-46a6-a645-095cbc8300f9", IsEnabled = true)]
		[RequireGenericGuildPermissions]
		public sealed class MakeAnEmbed : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> CommandAsync([Remainder] CustomEmbed args)
				=> Responses.Misc.MakeAnEmbed(args);
		}

		[LocalizedGroup(nameof(Groups.MessageRole))]
		[LocalizedAlias(nameof(Aliases.MessageRole))]
		[LocalizedSummary(nameof(Summaries.MessageRole))]
		[Meta("db524980-4a8e-4933-aa9b-527094d60165", IsEnabled = false)]
		[RequireGenericGuildPermissions]
		public sealed class MessageRole : AdvobotModuleBase
		{
			[Command]
			public async Task CommandAsync(
				[CanModifyRole, NotEveryone, NotMentionable] IRole role,
				[Remainder] string message)
			{
				var cut = message.Substring(0, Math.Min(message.Length, 250));
				var text = $"From `{Context.User.Format()}`, {role.Mention}: {cut}";
				await role.ModifyAsync(x => x.Mentionable = true, GenerateRequestOptions()).CAF();
				await ReplyAsync(text).CAF();
				await role.ModifyAsync(x => x.Mentionable = false, GenerateRequestOptions()).CAF();
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
			public async Task CommandAsync([Remainder] string message)
			{
				var owner = (await Context.Client.GetApplicationInfoAsync().CAF()).Owner;
				var cut = message.Substring(0, Math.Min(message.Length, 250));
				var text = $"`{Context.User.Format()}` - `{Context.Guild.Format()}`:\n```\n{cut}```";
				await owner.SendMessageAsync(text).CAF();
			}
		}

#warning reimplement
		/*
		[LocalizedGroup(nameof(Groups.Remind))]
		[LocalizedAlias(nameof(Aliases.Remind))]
		[LocalizedSummary(nameof(Summaries.Remind))]
		[Meta("3cedf19e-7a4d-47c0-ac2f-1c39a92026ec", IsEnabled = true)]
		public sealed class Remind : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> CommandAsync(
				[RemindTime] int minutes,
				[Remainder] string message)
			{
				var time = TimeSpan.FromMinutes(minutes);
				Timers.Add(new TimedMessage(time, Context.User, message));
				return Responses.Misc.Remind(time);
			}
		}*/

		[LocalizedGroup(nameof(Groups.Test))]
		[LocalizedAlias(nameof(Aliases.Test))]
		[LocalizedSummary(nameof(Summaries.Test))]
		[Meta("6c0b693e-e3ac-421e-910e-3178110d791d", IsEnabled = true)]
		[RequireBotOwner]
		[Hidden]
		public sealed class Test : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> CommandAsync(string response = "joe")
			{
				await Context.Guild.GetInvitesAsync().CAF();
				return AdvobotResult.Success(response);
			}
		}
	}
}