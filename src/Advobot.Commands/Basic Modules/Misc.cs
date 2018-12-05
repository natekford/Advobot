﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
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
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
			"If left blank will provide general help.")]
		[EnabledByDefault(true, AbleToToggle = false)]
		public sealed class Help : AdvobotModuleBase
		{
			public IHelpEntryService HelpEntries { get; set; }

			private static readonly string _GeneralHelp =
				$"Type `{Constants.PREFIX}{nameof(Commands)}` for the list of commands.\n" +
				$"Type `{Constants.PREFIX}{nameof(Help)} [Command]` for help with a command.";
			private static readonly string _BasicSyntax =
				"`[]` means required.\n" +
				"`<>` means optional.\n" +
				"`|` means or.";
			private static readonly string _MentionSyntax =
				"`User` means `@User|\"Username\"`.\n" +
				"`Role` means `@Role|\"Role Name\"`.\n" +
				"`Channel` means `#Channel|\"Channel Name\"`.";
			private static readonly string _Links =
				$"[GitHub Repository]({Constants.REPO})\n" +
				$"[Discord Server]({Constants.DISCORD_INV})";

			[Command]
			public Task Command()
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "General Help",
					Description = _GeneralHelp.Replace(Constants.PREFIX, GetPrefix()),
					Footer = new EmbedFooterBuilder { Text = "Help" },
					Fields = new List<EmbedFieldBuilder>
					{
						new EmbedFieldBuilder { Name = "Basic Syntax", Value = _BasicSyntax, IsInline = true, },
						new EmbedFieldBuilder { Name = "Mention Syntax", Value = _MentionSyntax, IsInline = true, },
						new EmbedFieldBuilder { Name = "Links", Value = _Links, IsInline = false, },
					},
				});
			}
			[Command, Priority(1)]
			public Task Command([Remainder] IHelpEntry command)
				=> SendHelp(command);
			[Command(RunMode = RunMode.Async), Priority(0)]
			public async Task Command([Remainder, OverrideTypeReader(typeof(CloseHelpEntryTypeReader))] IEnumerable<IHelpEntry> command)
			{
				var entry = await NextItemAtIndexAsync(command.ToArray(), x => x.Name).CAF();
				if (entry != null)
				{
					await SendHelp(entry).CAF();
				}
			}

			private Task SendHelp(IHelpEntry entry)
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = entry.Name,
					Description = entry.ToString(Context.GuildSettings.CommandSettings).Replace(Constants.PREFIX, GetPrefix()),
					Footer = new EmbedFooterBuilder { Text = "Help", },
				});
			}
		}

		[Group(nameof(Commands)), ModuleInitialismAlias(typeof(Commands))]
		[Summary("Prints out the commands in that category of the command list. " +
			"Inputting nothing will list the command categories.")]
		[EnabledByDefault(true)]
		public sealed class Commands : AdvobotModuleBase
		{
			public IHelpEntryService HelpEntries { get; set; }

			[ImplicitCommand, ImplicitAlias]
			public Task All()
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "All Commands",
					Description = $"`{HelpEntries.GetHelpEntries().Join("`, `", x => x.Name)}`",
				});
			}
			[Command]
			public Task Command(string category)
			{
				if (!HelpEntries.GetCategories().CaseInsContains(category))
				{
					return ReplyErrorAsync($"`{category}` is not a valid category.");
				}
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = category.FormatTitle(),
					Description = $"`{HelpEntries.GetHelpEntries(category).Join("`, `", x => x.Name)}`",
				});
			}
			[Command]
			public Task Command()
			{
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "Categories",
					Description = $"Type `{GetPrefix()}{nameof(Commands)} [Category]` for commands from a category.\n\n" +
						$"`{HelpEntries.GetCategories().Join("`, `")}`",
				});
			}
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
			public Task Command([Remainder] CustomEmbed args)
				=> ReplyEmbedAsync(args.BuildWrapper());
		}

		[Group(nameof(MessageRole)), ModuleInitialismAlias(typeof(MessageRole))]
		[Summary("Mention an unmentionable role with the given message.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(false)]
		public sealed class MessageRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command([NotEveryone] SocketRole role, [Remainder] string message)
			{
				if (role.IsMentionable)
				{
					await ReplyErrorAsync("You can already mention this role.").CAF();
					return;
				}
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
		public sealed class MessageBotOwner : AdvobotModuleBase
		{
			[Command]
			public async Task Command([Remainder] string message)
			{
				if (BotSettings.UsersUnableToDmOwner.Contains(Context.User.Id))
				{
					return;
				}
				if ((await Context.Client.GetApplicationInfoAsync().CAF()).Owner is IUser owner)
				{
					var cut = message.Substring(0, Math.Min(message.Length, 250));
					await owner.SendMessageAsync($"From `{Context.User.Format()}` in `{Context.Guild.Format()}`:\n```\n{cut}```").CAF();
					return;
				}
				await ReplyErrorAsync("The owner is unable to be gotten.").CAF();
			}
		}

		[Group(nameof(Remind)), ModuleInitialismAlias(typeof(Remind))]
		[Summary("Sends a message to the person who said the command after the passed in time is up. " +
			"Potentially may take one minute longer than asked for if the command is input at certain times.")]
		[EnabledByDefault(true)]
		public sealed class Remind : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateRemindTime] int minutes, [Remainder] string message)
			{
				await Timers.AddAsync(new TimedMessage(TimeSpan.FromMinutes(minutes), Context.User, message)).CAF();
				await ReplyTimedAsync($"Will remind in `{minutes}` minute(s).").CAF();
			}
		}

		[Group(nameof(Test)), ModuleInitialismAlias(typeof(Test))]
		[Summary("Mostly just makes the bot say test.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class Test : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Runtime([Optional] string input)
			{
				if (input == null)
				{
					return AdvobotResult.FromFailure("Invalid input.");
				}
				return AdvobotResult.FromSuccess("Success.");
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> BigWait()
			{
				await Task.Delay(2500);
				return AdvobotResult.FromSuccess("Big wait.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task NotRuntime([Optional] string input)
			{
				if (input == null)
				{
					return ReplyAsync("Invalid input.");
				}
				return ReplyAsync("Success.");
			}
		}
	}
}