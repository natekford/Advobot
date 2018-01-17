﻿using Advobot.Core;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Classes.NamedArguments;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Commands.Miscellaneous
{
	[Group(nameof(Help)), TopLevelShortAlias(typeof(Help))]
	[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
		"If left blank will provide general help.")]
	[DefaultEnabled(true)]
	public sealed class Help : AdvobotModuleBase
	{
		private static string _GeneralHelp =
			$"Type `{Constants.PLACEHOLDER_PREFIX}{nameof(Commands)}` for the list of commands.\n" +
			$"Type `{Constants.PLACEHOLDER_PREFIX}{nameof(Help)} [Command]` for help with a command.";
		private static string _BasicSyntax =
			"`[]` means required.\n" +
			"`<>` means optional.\n" +
			"`|` means or.";
		private static string _MentionSyntax =
			"`User` means `@User|\"Username\"`.\n" +
			"`Role` means `@Role|\"Role Name\"`.\n" +
			"`Channel` means `#Channel|\"Channel Name\"`.";
		private static string _Links =
			$"[GitHub Repository]({Constants.REPO})\n" +
			$"[Discord Server]({Constants.DISCORD_INV})";

		[Command]
		public async Task Command()
		{
			var embed = new EmbedWrapper
			{
				Title = "General Help",
				Description = _GeneralHelp.Replace(Constants.PLACEHOLDER_PREFIX, Context.GetPrefix()),
			};
			embed.TryAddField("Basic Syntax", _BasicSyntax, true, out var firstFieldErrors);
			embed.TryAddField("Mention Syntax", _MentionSyntax, true, out var secondFieldErrors);
			embed.TryAddField("Links", _Links, false, out var thirdFieldErrors);
			embed.TryAddFooter("Help", null, out var footerErrors);
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command]
		public async Task Command(string commandName)
		{
			var helpEntry = Constants.HELP_ENTRIES[commandName];
			if (helpEntry != null)
			{
				var embed = new EmbedWrapper
				{
					Title = helpEntry.Name,
					Description = helpEntry.ToString().Replace(Constants.PLACEHOLDER_PREFIX, Context.GetPrefix()),
				};
				embed.TryAddFooter("Help", null, out var footerErrors);
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
				return;
			}

			var closeHelps = new CloseHelpEntries(commandName);
			if (closeHelps.List.Any())
			{
				var text = $"Did you mean any of the following:\n{closeHelps.List.FormatNumberedList("{0}", x => x.Word.Name)}";
				var msg = await MessageUtils.SendMessageAsync(Context.Channel, text).CAF();
				await Context.Timers.AddActiveCloseHelp(Context.User as IGuildUser, msg, closeHelps).CAF();
				return;
			}

			await MessageUtils.SendErrorMessageAsync(Context, new Error("Nonexistent command.")).CAF();
		}
	}

	[Group(nameof(Commands)), TopLevelShortAlias(typeof(Commands))]
	[Summary("Prints out the commands in that category of the command list. " +
		"Inputting nothing will list the command categories.")]
	[DefaultEnabled(true)]
	public sealed class Commands : AdvobotModuleBase
	{
		private static string _Command = $"Type `{Constants.PLACEHOLDER_PREFIX}{nameof(Commands)} [Category]` for commands from that category.\n\n";
		private static string _Categories = $"`{String.Join("`, `", Enum.GetNames(typeof(CommandCategory)))}`";
		private static string _CommandCategories = _Command + _Categories;

		[Command(nameof(All)), ShortAlias(nameof(All))]
		public async Task All()
		{
			var embed = new EmbedWrapper
			{
				Title = "All Commands",
				Description = $"`{String.Join("`, `", Constants.HELP_ENTRIES.GetCommandNames())}`",
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command]
		public async Task Command(CommandCategory category)
		{
			var embed = new EmbedWrapper
			{
				Title = category.EnumName(),
				Description = $"`{String.Join("`, `", Constants.HELP_ENTRIES[category].Select(x => x.Name))}`",
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command]
		public async Task Command()
		{
			var embed = new EmbedWrapper
			{
				Title = "Categories",
				Description = _CommandCategories.Replace(Constants.PLACEHOLDER_PREFIX, Context.GetPrefix()),
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(MakeAnEmbed)), TopLevelShortAlias(typeof(MakeAnEmbed))]
	[Summary("Makes an embed with the given arguments. Urls need http:// in front of them. " +
		"FieldInfo can have up to 25 arguments supplied. " +
		"Each must be formatted like the following: `" + CustomEmbed.FORMAT + "`.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class MakeAnEmbed : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] NamedArguments<CustomEmbed> arguments)
		{
			var embed = arguments.CreateObject().Embed;
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(MentionRole)), TopLevelShortAlias(typeof(MentionRole))]
	[Summary("Mention an unmentionable role with the given message.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class MentionRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Remainder] string message)
		{
			if (role.IsMentionable)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("You can already mention this role.")).CAF();
			}
			else
			{
				var cutText = $"From `{Context.User.Format()}`, {role.Mention}: {message.Substring(0, Math.Min(message.Length, 250))}";
				//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
				await role.ModifyAsync(x => x.Mentionable = true, new ModerationReason(Context.User, null).CreateRequestOptions()).CAF();
				await MessageUtils.SendMessageAsync(Context.Channel, cutText).CAF();
				await role.ModifyAsync(x => x.Mentionable = false, new ModerationReason(Context.User, null).CreateRequestOptions()).CAF();
			}
		}
	}

	[Group(nameof(MessageBotOwner)), TopLevelShortAlias(typeof(MessageBotOwner))]
	[Summary("Sends a message to the bot owner with the given text. " +
		"Messages will be cut down to 250 characters.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(false)]
	public sealed class MessageBotOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] string message)
		{
			var cut = message.Substring(0, Math.Min(message.Length, 250));
			var newMsg = $"From `{Context.User.Format()}` in `{Context.Guild.Format()}`:\n```\n{cut}```";

			var owner = await ClientUtils.GetBotOwnerAsync(Context.Client).CAF();
			if (owner != null)
			{
				await owner.SendMessageAsync(newMsg).CAF();
			}
			else
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The owner is unable to be gotten.")).CAF();
			}
		}
	}

	[Group(nameof(Test)), TopLevelShortAlias(typeof(Test))]
	[Summary("Mostly just makes the bot say test.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class Test : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await MessageUtils.SendMessageAsync(Context.Channel, "test").CAF();
		}
	}
}