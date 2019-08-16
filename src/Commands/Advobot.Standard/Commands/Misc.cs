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
	public sealed class Misc : ModuleBase
	{
		[Group(nameof(Help)), ModuleInitialismAlias(typeof(Help))]
		[LocalizedSummary(nameof(Summaries.Help))]
		[CommandMeta("0e89a6fd-5c9c-4008-a912-7c719ea7827d", IsEnabled = true, CanToggle = false)]
		public sealed class Help : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IHelpEntryService HelpEntries { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			private const string TEMP_SUMMARY = "Input the name of the module you want to get information for";

			[Command]
			[Summary("Prints out general help information for the bot.")]
			public Task<RuntimeResult> Command()
				=> Responses.Misc.GeneralHelp(Context.Settings.GetPrefix(BotSettings));
			[Command, Priority(1)]
			[Summary("Prints out help information for a specified module.")]
			public Task<RuntimeResult> Command(
				[Summary(TEMP_SUMMARY)] IHelpEntry command)
				=> Responses.Misc.Help(command, Context.Settings);
			[Command, Priority(2)]
			[Summary("Prints out help information for a specific command in a specified module.")]
			public Task<RuntimeResult> Command(
				[Summary(TEMP_SUMMARY)] IHelpEntry command,
				[Positive] int position)
				=> Responses.Misc.Help(command, Context.Settings, position - 1);
			[Command(RunMode = RunMode.Async), Priority(0)]
			[Summary("Attempts to find a help entry with a name similar to the input. This command only gets used if an invalid name is provided.")]
			public async Task<RuntimeResult> Command(
				[Summary(TEMP_SUMMARY), OverrideTypeReader(typeof(CloseHelpEntryTypeReader))] IEnumerable<IHelpEntry> command)
			{
				var entry = await NextItemAtIndexAsync(command.ToArray(), x => x.Name).CAF();
				if (entry != null)
				{
					return Responses.Misc.Help(entry, Context.Settings);
				}
				return AdvobotResult.IgnoreFailure;
			}
		}

		[Group(nameof(Commands)), ModuleInitialismAlias(typeof(Commands))]
		[LocalizedSummary(nameof(Summaries.Commands))]
		[CommandMeta("ec0f7aef-85d6-4251-9c8e-7c70890f455e", IsEnabled = true, CanToggle = false)]
		public sealed class Commands : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IHelpEntryService HelpEntries { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> All()
				=> Responses.Misc.AllCommands(HelpEntries.GetHelpEntries());
			[Command]
			public Task<RuntimeResult> Command([CommandCategory] string category)
				=> Responses.Misc.CategoryCommands(HelpEntries.GetHelpEntries(category), category);
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Misc.GeneralCommandInfo(HelpEntries.GetCategories(), Prefix);
		}

		[Group(nameof(MakeAnEmbed)), ModuleInitialismAlias(typeof(MakeAnEmbed))]
		[LocalizedSummary(nameof(Summaries.MakeAnEmbed))]
		[CommandMeta("6acf2d14-b251-46a6-a645-095cbc8300f9", IsEnabled = true)]
		[RequireGenericGuildPermissions]
		public sealed class MakeAnEmbed : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([Remainder] CustomEmbed args)
				=> Responses.Misc.MakeAnEmbed(args);
		}

		[Group(nameof(MessageRole)), ModuleInitialismAlias(typeof(MessageRole))]
		[LocalizedSummary(nameof(Summaries.MessageRole))]
		[CommandMeta("db524980-4a8e-4933-aa9b-527094d60165", IsEnabled = false)]
		[RequireGenericGuildPermissions]
		public sealed class MessageRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command(
				[NotEveryone, NotMentionable] IRole role,
				[Remainder] string message)
			{
				var text = $"From `{Context.User.Format()}`, {role.Mention}: {message.Substring(0, Math.Min(message.Length, 250))}";
				await role.ModifyAsync(x => x.Mentionable = true, GenerateRequestOptions()).CAF();
				await ReplyAsync(text).CAF();
				await role.ModifyAsync(x => x.Mentionable = false, GenerateRequestOptions()).CAF();
			}
		}

		[Group(nameof(MessageBotOwner)), ModuleInitialismAlias(typeof(MessageBotOwner))]
		[LocalizedSummary(nameof(Summaries.MessageBotOwner))]
		[CommandMeta("3562f937-4d3c-46aa-afda-70e04040be53", IsEnabled = false)]
		[RequireGenericGuildPermissions]
		[RequireAllowedToDmBotOwner]
		public sealed class MessageBotOwner : AdvobotModuleBase
		{
			[Command]
			public async Task Command([Remainder] string message)
			{
				var owner = (await Context.Client.GetApplicationInfoAsync().CAF()).Owner;
				var text = $"From `{Context.User.Format()}` in `{Context.Guild.Format()}`:\n```\n{message.Substring(0, Math.Min(message.Length, 250))}```";
				await owner.SendMessageAsync(text).CAF();
			}
		}

		[Group(nameof(Remind)), ModuleInitialismAlias(typeof(Remind))]
		[LocalizedSummary(nameof(Summaries.Remind))]
		[CommandMeta("3cedf19e-7a4d-47c0-ac2f-1c39a92026ec", IsEnabled = true)]
		public sealed class Remind : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(
				[RemindTime] int minutes,
				[Remainder] string message)
			{
				var time = TimeSpan.FromMinutes(minutes);
				Timers.Add(new TimedMessage(time, Context.User, message));
				return Responses.Misc.Remind(time);
			}
		}

		[Group(nameof(Test)), ModuleInitialismAlias(typeof(Test))]
		[LocalizedSummary(nameof(Summaries.Test))]
		[CommandMeta("6c0b693e-e3ac-421e-910e-3178110d791d", IsEnabled = true)]
		[RequireBotOwner]
		public sealed class Test : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([GuildSettingName] string name)
			{
				//var invite = await Context.Channel.CreateInviteAsync(123, 3, false, false).CAF();
				return AdvobotResult.Success("test test " + name);
			}
		}
	}
}