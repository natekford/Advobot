using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.CloseWords;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Commands.Misc
{
	[Category(typeof(Help)), Group(nameof(Help)), TopLevelShortAlias(typeof(Help))]
	[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. " +
		"If left blank will provide general help.")]
	[DefaultEnabled(true, false)]
	public sealed class Help : AdvobotModuleBase
	{
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
			var embed = new EmbedWrapper
			{
				Title = "General Help",
				Description = _GeneralHelp.Replace(Constants.PREFIX, Context.GetPrefix())
			};
			embed.TryAddField("Basic Syntax", _BasicSyntax, true, out _);
			embed.TryAddField("Mention Syntax", _MentionSyntax, true, out _);
			embed.TryAddField("Links", _Links, false, out _);
			embed.TryAddFooter("Help", null, out _);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command, Priority(1)]
		[RequiredServices(typeof(IHelpEntryService))]
		public async Task Command([Remainder] IHelpEntry command)
		{
			var embed = new EmbedWrapper
			{
				Title = command.Name,
				Description = command.ToString(Context.GuildSettings.CommandSettings).Replace(Constants.PREFIX, Context.GetPrefix())
			};
			embed.TryAddFooter("Help", null, out _);
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command, Priority(0)]
		[RequiredServices(typeof(IHelpEntryService), typeof(ITimerService))]
		public async Task Command([Remainder] string command)
		{
			var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
			var timers = Context.Provider.GetRequiredService<ITimerService>();
			var closeHelps = new CloseHelpEntries(default, Context, helpEntries, Context.GuildSettings.CommandSettings, command);
			if (closeHelps.List.Any())
			{
				await closeHelps.SendBotMessageAsync(Context.Channel).CAF();
				await timers.AddAsync(closeHelps).CAF();
				return;
			}
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Unable to find any commands similar to `{command}`.").CAF();
		}
	}

	[Category(typeof(Commands)), Group(nameof(Commands)), TopLevelShortAlias(typeof(Commands))]
	[Summary("Prints out the commands in that category of the command list. " +
		"Inputting nothing will list the command categories.")]
	[DefaultEnabled(true)]
	[RequiredServices(typeof(IHelpEntryService))]
	public sealed class Commands : AdvobotModuleBase
	{
		[Command(nameof(All)), ShortAlias(nameof(All))]
		public async Task All()
		{
			var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
			var embed = new EmbedWrapper
			{
				Title = "All Commands",
				Description = $"`{String.Join("`, `", helpEntries.GetHelpEntries().Select(x => x.Name))}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command]
		public async Task Command(string category)
		{
			var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
			if (!helpEntries.GetCategories().CaseInsContains(category))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
				return;
			}
			var embed = new EmbedWrapper
			{
				Title = category,
				Description = $"`{String.Join("`, `", helpEntries.GetHelpEntries(category).Select(x => x.Name))}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command]
		public async Task Command()
		{
			var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
			var embed = new EmbedWrapper
			{
				Title = "Categories",
				Description = $"Type `{Context.GetPrefix()}{nameof(Commands)} [Category]` for commands from that category.\n\n" +
					$"`{String.Join("`, `", helpEntries.GetCategories())}`",
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
	}

	[Category(typeof(MakeAnEmbed)), Group(nameof(MakeAnEmbed)), TopLevelShortAlias(typeof(MakeAnEmbed))]
	[Summary("Makes an embed with the given arguments. Urls need http:// in front of them. " +
		"FieldInfo can have up to 25 arguments supplied. " +
		"Each must be formatted like the following: `" + CustomEmbed.FIELD_FORMAT + "`.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class MakeAnEmbed : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] NamedArguments<CustomEmbed> args)
		{
			if (!args.TryCreateObject(new object[0], out var obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			await MessageUtils.SendMessageAsync(Context.Channel, null, obj.Embed).CAF();
		}
	}

	[Category(typeof(MessageRole)), Group(nameof(MessageRole)), TopLevelShortAlias(typeof(MessageRole))]
	[Summary("Mention an unmentionable role with the given message.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(false)]
	public sealed class MessageRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone)] IRole role, [Remainder] string message)
		{
			if (role.IsMentionable)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("You can already mention this role.")).CAF();
			}
			else
			{
				var cutText = $"From `{Context.User.Format()}`, {role.Mention}: {message.Substring(0, Math.Min(message.Length, 250))}";
				//I don't think I can pass this through to RoleActions.ModifyRoleMentionability because the context won't update in time for this to work correctly
				await role.ModifyAsync(x => x.Mentionable = true, GetRequestOptions()).CAF();
				await MessageUtils.SendMessageAsync(Context.Channel, cutText).CAF();
				await role.ModifyAsync(x => x.Mentionable = false, GetRequestOptions()).CAF();
			}
		}
	}

	[Category(typeof(MessageBotOwner)), Group(nameof(MessageBotOwner)), TopLevelShortAlias(typeof(MessageBotOwner))]
	[Summary("Sends a message to the bot owner with the given text. " +
		"Messages will be cut down to 250 characters.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(false)]
	public sealed class MessageBotOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder] string message)
		{
			if (Context.BotSettings.UsersUnableToDmOwner.Contains(Context.User.Id))
			{
				return;
			}
			if ((await Context.Client.GetApplicationInfoAsync().CAF()).Owner is IUser owner)
			{
				var cut = message.Substring(0, Math.Min(message.Length, 250));
				await owner.SendMessageAsync($"From `{Context.User.Format()}` in `{Context.Guild.Format()}`:\n```\n{cut}```").CAF();
			}
			else
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The owner is unable to be gotten.")).CAF();
			}
		}
	}

	[Category(typeof(Remind)), Group(nameof(Remind)), TopLevelShortAlias(typeof(Remind))]
	[Summary("Sends a message to the person who said the command after the passed in time is up. " +
		"Potentially may take one minute longer than asked for if the command is input at certain times.")]
	[DefaultEnabled(true)]
	[RequiredServices(typeof(ITimerService))]
	public sealed class Remind : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateNumber(1, 10000)] uint minutes, [Remainder] string message)
		{
			var timers = Context.Provider.GetRequiredService<ITimerService>();
			await timers.AddAsync(new TimedMessage(TimeSpan.FromMinutes(minutes), Context.User, message)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, $"Will remind in `{minutes}` minute(s).").CAF();
		}
	}

	[Category(typeof(Test)), Group(nameof(Test)), TopLevelShortAlias(typeof(Test))]
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