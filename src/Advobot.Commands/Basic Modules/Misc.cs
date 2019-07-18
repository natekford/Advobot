﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.SettingValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Advobot.Classes.TypeReaders;
using Advobot.Commands.Localization;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Misc
{
	public sealed class Misc : ModuleBase
	{
		[Group(nameof(Help)), ModuleInitialismAlias(typeof(Help))]
		[Summary("Prints out help information")]
		[EnabledByDefault(true, AbleToToggle = false)]
		public sealed class Help : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IHelpEntryService HelpEntries { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			private const string TEMP_SUMMARY = "Input the name of the module you want to get information for";

			[Command]
			[Summary("Prints out general help information for the bot.")]
			public Task<RuntimeResult> Command()
				=> Responses.Misc.GeneralHelp(Context.GuildSettings.GetPrefix(BotSettings));
			[Command, Priority(1)]
			[Summary("Prints out help information for a specified module.")]
			public Task<RuntimeResult> Command([Summary(TEMP_SUMMARY)] IHelpEntry command)
				=> Responses.Misc.Help(command, Context.GuildSettings);
			[Command, Priority(2)]
			[Summary("Prints out help information for a specific command in a specified module.")]
			public Task<RuntimeResult> Command([Summary(TEMP_SUMMARY)] IHelpEntry command, [ValidatePositiveNumber] int position)
				=> Responses.Misc.Help(command, Context.GuildSettings, position - 1);
			[Command(RunMode = RunMode.Async), Priority(0)]
			[Summary("Attempts to find a help entry with a name similar to the input. This command only gets used if an invalid name is provided.")]
			public async Task<RuntimeResult> Command([Summary(TEMP_SUMMARY), OverrideTypeReader(typeof(CloseHelpEntryTypeReader))] IEnumerable<IHelpEntry> command)
			{
				var entry = await NextItemAtIndexAsync(command.ToArray(), x => x.Name).CAF();
				if (entry != null)
				{
					return Responses.Misc.Help(entry, Context.GuildSettings);
				}
				return AdvobotResult.Ignore;
			}
		}

		[Group(nameof(Commands)), ModuleInitialismAlias(typeof(Commands))]
		[Summary("Prints out the commands in that category of the command list. " +
			"Inputting nothing will list the command categories.")]
		[EnabledByDefault(true)]
		public sealed class Commands : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public IHelpEntryService HelpEntries { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> All()
				=> Responses.Misc.AllCommands(HelpEntries.GetHelpEntries());
			[Command]
			public Task<RuntimeResult> Command([ValidateCommandCategory] string category)
				=> Responses.Misc.CategoryCommands(HelpEntries.GetHelpEntries(category), category);
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Misc.GeneralCommandInfo(HelpEntries.GetCategories(), Context.GuildSettings.GetPrefix(BotSettings));
		}

		[Group(nameof(MakeAnEmbed)), ModuleInitialismAlias(typeof(MakeAnEmbed))]
		[Summary("Makes an embed with the given arguments. Urls need http:// in front of them. " +
			"FieldInfo can have up to 25 arguments supplied. " +
			//TODO: redocument field format
			"Each must be formatted like the following: `temp`.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(true)]
		public sealed class MakeAnEmbed : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([Remainder] CustomEmbed args)
				=> Responses.Misc.MakeAnEmbed(args);
		}

		[Group(nameof(MessageRole)), ModuleInitialismAlias(typeof(MessageRole))]
		[Summary("Mention an unmentionable role with the given message.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(false)]
		public sealed class MessageRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command([NotEveryone, NotMentionable] SocketRole role, [Remainder] string message)
			{
				var text = $"From `{Context.User.Format()}`, {role.Mention}: {message.Substring(0, Math.Min(message.Length, 250))}";
				//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
				await role.ModifyAsync(x => x.Mentionable = true, GenerateRequestOptions()).CAF();
				await ReplyAsync(text).CAF();
				await role.ModifyAsync(x => x.Mentionable = false, GenerateRequestOptions()).CAF();
			}
		}

		[Group(nameof(MessageBotOwner)), ModuleInitialismAlias(typeof(MessageBotOwner))]
		[Summary("Sends a message to the bot owner with the given text. " +
			"Messages will be cut down to 250 characters.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(false)]
		[RequireAllowedToDmBotOwnerAttribute]
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
		[Summary("Sends a message to the person who said the command after the passed in time is up. " +
			"Potentially may take one minute longer than asked for if the command is input at certain times.")]
		[EnabledByDefault(true)]
		public sealed class Remind : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([ValidateRemindTime] int minutes, [Remainder] string message)
			{
				var time = TimeSpan.FromMinutes(minutes);
				Timers.Add(new TimedMessage(time, Context.User, message));
				return Responses.Misc.Remind(time);
			}
		}

		[Group(nameof(Test)), ModuleInitialismAlias(typeof(Test))]
		[LocalizedSummary(nameof(strings.Summary_Test))]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class Test : AdvobotModuleBase
		{
			[Command("steven")]
			public Task<RuntimeResult> Command([ValidatePositiveNumber] int num)
			{
				//var invite = await Context.Channel.CreateInviteAsync(123, 3, false, false).CAF();
				return AdvobotResult.Success("test test " + num.ToString());
			}
		}
	}
}