using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Misc
{
	[Category(typeof(Help)), Group(nameof(Help)), TopLevelShortAlias(typeof(Help))]
	[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
		"If left blank will provide general help.")]
	[DefaultEnabled(true, AbleToToggle = false)]
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
		public async Task Command()
		{
			await ReplyEmbedAsync(new EmbedWrapper
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
			}).CAF();
		}
		[Command, Priority(1)]
		public async Task Command([Remainder] IHelpEntry command)
			=> await SendHelp(command);
		[Command(RunMode = RunMode.Async), Priority(0)]
		public async Task Command([Remainder, OverrideTypeReader(typeof(CloseHelpEntryTypeReader))] IEnumerable<IHelpEntry> command)
		{
			var entry = await NextItemAtIndexAsync(command.ToArray(), x => x.Name).CAF();
			if (entry != null)
			{
				await SendHelp(entry).CAF();
			}
		}

		private async Task SendHelp(IHelpEntry entry)
		{
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = entry.Name,
				Description = entry.ToString(Context.GuildSettings.CommandSettings).Replace(Constants.PREFIX, GetPrefix()),
				Footer = new EmbedFooterBuilder { Text = "Help", },
			}).CAF();
		}
	}

#warning redo category command with typereader
	[Category(typeof(Commands)), Group(nameof(Commands)), TopLevelShortAlias(typeof(Commands))]
	[Summary("Prints out the commands in that category of the command list. " +
		"Inputting nothing will list the command categories.")]
	[DefaultEnabled(true)]
	public sealed class Commands : AdvobotModuleBase
	{
		public IHelpEntryService HelpEntries { get; set; }

		[Command(nameof(All)), ShortAlias(nameof(All))]
		public async Task All()
		{
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "All Commands",
				Description = $"`{HelpEntries.GetHelpEntries().Join("`, `", x => x.Name)}`",
			}).CAF();
		}
		[Command]
		public async Task Command(string category)
		{
			if (!HelpEntries.GetCategories().CaseInsContains(category))
			{
				await ReplyErrorAsync(new Error($"`{category}` is not a valid category.")).CAF();
				return;
			}
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = category,
				Description = $"`{HelpEntries.GetHelpEntries(category).Join("`, `", x => x.Name)}`",
			}).CAF();
		}
		[Command]
		public async Task Command()
		{
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Categories",
				Description = $"Type `{GetPrefix()}{nameof(Commands)} [Category]` for commands from that category.\n\n" +
					$"`{HelpEntries.GetCategories().Join("`, `")}`",
			}).CAF();
		}
	}

	[Category(typeof(MakeAnEmbed)), Group(nameof(MakeAnEmbed)), TopLevelShortAlias(typeof(MakeAnEmbed))]
	[Summary("Makes an embed with the given arguments. Urls need http:// in front of them. " +
		"FieldInfo can have up to 25 arguments supplied. " +
		//TODO: redocument field format
		"Each must be formatted like the following: `temp`.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class MakeAnEmbed : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] CustomEmbed args)
			=> await ReplyEmbedAsync(args.BuildWrapper()).CAF();
	}

	[Category(typeof(MessageRole)), Group(nameof(MessageRole)), TopLevelShortAlias(typeof(MessageRole))]
	[Summary("Mention an unmentionable role with the given message.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(false)]
	public sealed class MessageRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([NotEveryone] SocketRole role, [Remainder] string message)
		{
			if (role.IsMentionable)
			{
				await ReplyErrorAsync(new Error("You can already mention this role.")).CAF();
				return;
			}
			var text = $"From `{Context.User.Format()}`, {role.Mention}: {message.Substring(0, Math.Min(message.Length, 250))}";
			//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
			await role.ModifyAsync(x => x.Mentionable = true, GenerateRequestOptions()).CAF();
			await ReplyAsync(text).CAF();
			await role.ModifyAsync(x => x.Mentionable = false, GenerateRequestOptions()).CAF();
		}
	}

	[Category(typeof(MessageBotOwner)), Group(nameof(MessageBotOwner)), TopLevelShortAlias(typeof(MessageBotOwner))]
	[Summary("Sends a message to the bot owner with the given text. " +
		"Messages will be cut down to 250 characters.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(false)]
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
			await ReplyErrorAsync(new Error("The owner is unable to be gotten.")).CAF();
		}
	}

	[Category(typeof(Remind)), Group(nameof(Remind)), TopLevelShortAlias(typeof(Remind))]
	[Summary("Sends a message to the person who said the command after the passed in time is up. " +
		"Potentially may take one minute longer than asked for if the command is input at certain times.")]
	[DefaultEnabled(true)]
	public sealed class Remind : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateRemindTime] int minutes, [Remainder] string message)
		{
			await Timers.AddAsync(new TimedMessage(TimeSpan.FromMinutes(minutes), Context.User, message)).CAF();
			await ReplyTimedAsync($"Will remind in `{minutes}` minute(s).").CAF();
		}
	}

	[Category(typeof(Test)), Group(nameof(Test)), TopLevelShortAlias(typeof(Test))]
	[Summary("Mostly just makes the bot say test.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class Test : AdvobotModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user)
			=> await ReplyAsync("test").CAF();
	}
}