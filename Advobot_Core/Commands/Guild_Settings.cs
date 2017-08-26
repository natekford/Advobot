using Advobot.Actions;
using Advobot.TypeReaders;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.NonSavedClasses;
using Advobot.Enums;
using Advobot.Attributes;
using System.Reflection;
using Advobot.Interfaces;
using Advobot.SavedClasses;

namespace Advobot
{
	namespace GuildSettings
	{
		[Group(nameof(ModifyGuildPrefix)), Alias("mgdp")]
		[Usage("[New Prefix|Clear]")]
		[Summary("Makes the bot use the given prefix in the guild.")]
		[OtherRequirement(Precondition.GuildOwner)]
		[DefaultEnabled(false)]
		public sealed class ModifyGuildPrefix : MySavingModuleBase
		{
			[Command(nameof(ActionType.Clear)), Priority(1)]
			public async Task Command()
			{
				Context.GuildSettings.Prefix = null;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the guild's prefix.");
			}
			[Command, Priority(0)]
			public async Task Command([VerifyStringLength(Target.Prefix)] string newPrefix)
			{
				Context.GuildSettings.Prefix = newPrefix;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set this guild's prefix to: `{newPrefix}`.");
			}
		}

		[Group(nameof(ModifyCommands)), Alias("modcom", "mcom")]
		[Usage("[Enable|Disable] [Command Name|Category Name|All]")]
		[Summary("Turns a command on or off. Can turn all commands in a category on or off too. Cannot turn off `" + nameof(ModifyCommands) + "` or `" + nameof(Miscellaneous.Help) + "`.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class ModifyCommands : MySavingModuleBase
		{
			private static readonly string[] _CommandsUnableToBeTurnedOff = new[] { nameof(ModifyCommands), nameof(Miscellaneous.Help) };

			[Group(nameof(ActionType.Enable)), Alias("e")]
			public sealed class Enable : MySavingModuleBase
			{
				[Command("all"), Priority(1)]
				public async Task CommandAll()
				{
					//Only grab commands that are already disabled and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => !x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
					await MessageActions.SendChannelMessage(Context, $"Successfully enabled the following commands: `{text}`.");
				}
				[Command, Priority(0)]
				public async Task Command(CommandSwitch command)
				{
					if (command.Value)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("This command is already enabled."));
						return;
					}
					else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Please don't try to edit that command."));
						return;
					}

					command.ToggleEnabled();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully enabled `{command.Name}`.");
				}
				[Command]
				public async Task Command(CommandCategory category)
				{
					//Only grab commands that are already disabled and in the same category and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => !x.Value && x.Category == category && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					await MessageActions.SendChannelMessage(Context, $"Successfully enabled the following commands: `{String.Join("`, `", commands.Select(x => x.Name))}`.");
				}
			}
			[Group(nameof(ActionType.Disable)), Alias("d")]
			public sealed class Disable : MySavingModuleBase
			{
				[Command("all"), Priority(1)]
				public async Task CommandAll()
				{
					//Only grab commands that are already enabled and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Value && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					var text = commands.Any() ? String.Join("`, `", commands.Select(x => x.Name)) : "None";
					await MessageActions.SendChannelMessage(Context, $"Successfully disabled the following commands: `{text}`.");
				}
				[Command, Priority(0)]
				public async Task Command(CommandSwitch command)
				{
					if (!command.Value)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("This command is already disabled."));
						return;
					}
					else if (_CommandsUnableToBeTurnedOff.CaseInsContains(command.Name))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Please don't try to edit that command."));
						return;
					}

					command.ToggleEnabled();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully disabled `{command.Name}`.");
				}
				[Command]
				public async Task Command(CommandCategory category)
				{
					//Only grab commands that are already enabled and in the same category and are able to be changed.
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Value && x.Category == category && !_CommandsUnableToBeTurnedOff.CaseInsContains(x.Name));
					foreach (var command in commands)
					{
						command.ToggleEnabled();
					}
					await MessageActions.SendChannelMessage(Context, $"Successfully disabled the following commands: `{String.Join("`, `", commands.Select(x => x.Name))}`.");
				}
			}
		}

		[Group(nameof(ModifyIgnoredCommandChannels)), Alias("micc")]
		[Usage("[Add|Remove] [Channel] <Command Name|Category Name>")]
		[Summary("The bot will ignore commands said on these channels. If a command is input then the bot will instead ignore only that command on the given channel.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyIgnoredCommandChannels : MySavingModuleBase
		{
			private static readonly string[] _CommandsUnableToBeTurnedOff = new[] { nameof(ModifyIgnoredCommandChannels), nameof(Miscellaneous.Help) };

			[Group(nameof(ActionType.Add)), Alias("a")]
			public sealed class Add : MySavingModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel)
				{
					if (Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"`{channel.FormatChannel()}` is already ignoring commands.");
						return;
					}

					Context.GuildSettings.IgnoredCommandChannels.Add(channel.Id);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully added `{channel.FormatChannel()}` to the ignored command channels list.");
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
				{
					//First remove every command override on that channel for that specific command (so whenever this command is used it forces an update on the status of the list, never have duplicates)
					Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
					Context.GuildSettings.CommandsDisabledOnChannel.Add(new CommandOverride(command.Name, channel.Id));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully started ignoring the command `{command.Name}` on `{channel.FormatChannel()}`.");
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandCategory category)
				{
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Category == category);
					foreach (var command in commands)
					{
						Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
						Context.GuildSettings.CommandsDisabledOnChannel.Add(new CommandOverride(command.Name, channel.Id));
					}

					var cmdNames = String.Join("`, `", commands.Select(x => x.Name));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully started ignoring the following commands on `{channel.FormatChannel()}`: `{cmdNames}`.");
				}
			}
			[Group(nameof(ActionType.Remove)), Alias("r")]
			public sealed class Remove : MySavingModuleBase
			{
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel)
				{
					if (!Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"`{channel.FormatChannel()}` is already allowing commands.");
						return;
					}

					Context.GuildSettings.IgnoredCommandChannels.RemoveAll(x => x == channel.Id);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed `{channel.FormatChannel()}` from the ignored command channels list.");
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandSwitch command)
				{
					//First remove every command override on that channel for that specific command (so whenever this command is used it forces an update on the status of the list, never have duplicates)
					Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully stopped ignoring the command `{command.Name}` on `{channel.FormatChannel()}`.");
				}
				[Command]
				public async Task Command([VerifyChannel(true, ChannelVerification.CanBeRead, ChannelVerification.CanBeEdited)] ITextChannel channel, CommandCategory category)
				{
					var commands = Context.GuildSettings.CommandSwitches.Where(x => x.Category == category);
					foreach (var command in commands)
					{
						Context.GuildSettings.CommandsDisabledOnChannel.RemoveAll(x => x.Id == channel.Id && x.Name.CaseInsEquals(command.Name));
					}

					var cmdNames = String.Join("`, `", commands.Select(x => x.Name));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully stopped ignoring the following commands on `{channel.FormatChannel()}`: `{cmdNames}`.");
				}
			}
		}

		[Group(nameof(ModifyBotUsers)), Alias("mbu")]
		[Usage("[Show|Add|Remove] <User> <Permission/...>")]
		[Summary("Gives a user permissions in the bot but not on Discord itself. Type `" + Constants.BOT_PREFIX + "mbu [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "mbu [Show] [User]` to see the permissions of that user.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyBotUsers : MySavingModuleBase
		{
			[Group(nameof(ActionType.Show)), Alias("s")]
			public sealed class Show : MyModuleBase
			{
				[Command]
				public async Task Command()
				{
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Bot Permission Types", $"`{String.Join("`, `", Constants.GUILD_PERMISSIONS.Select(x => x.Name))}`"));
				}
				[Command]
				public async Task Command(IUser user)
				{
					var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
					if (botUser == null || botUser.Permissions == 0)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("That user has no extra permissions from the bot."));
						return;
					}

					var desc = $"`{String.Join("`, `", GetActions.GetGuildPermissionNames(botUser.Permissions))}`";
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed($"Permissions for {user.FormatUser()}", desc));
				}
			}
			[Group(nameof(ActionType.Add)), Alias("a")]
			public sealed class Add : MySavingModuleBase
			{
				[Command]
				public async Task Command(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
				{
					rawValue |= (Context.User as IGuildUser).GuildPermissions.RawValue;

					var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id) ?? new BotImplementedPermissions(user.Id, rawValue);
					botUser.AddPermissions(rawValue);

					var givenPerms = String.Join("`, `", GetActions.GetGuildPermissionNames(rawValue));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully gave `{user.FormatUser()}` the following bot permissions: `{givenPerms}`.");
				}
			}
			[Group(nameof(ActionType.Remove)), Alias("r")]
			public sealed class Remove : MySavingModuleBase
			{
				[Command]
				public async Task Command(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
				{
					rawValue |= (Context.User as IGuildUser).GuildPermissions.RawValue;

					var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
					if (botUser == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "That user does not have any bot permissions to remove");
						return;
					}
					botUser.RemovePermissions(rawValue);

					var takenPerms = String.Join("`, `", GetActions.GetGuildPermissionNames(rawValue));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the following bot permissions from `{user.FormatUser()}`: `{takenPerms}`.");
				}
			}
		}

		[Group(nameof(ModifyPersistentRoles)), Alias("mpr")]
		[Usage("[Show|Add|Remove] [User] [Role]")]
		[Summary("Gives a user a role that stays even when they leave and rejoin the server.Type `" + Constants.BOT_PREFIX + "mpr [Show]` to see the which users have persistent roles set up. " +
			"Type `" + Constants.BOT_PREFIX + "mpr [Show] [User]` to see the persistent roles of that user.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ModifyPersistentRoles : MySavingModuleBase
		{

		}

		[Group(nameof(ModifyChannelSettings)), Alias("mcs")]
		[Usage("[ImageOnly] <Channel>")]
		[Summary("Image only works solely on attachments. No input channel means it applies to the current channel. Using the command on an already targetted channel turns it off.")]
		[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyChannelSettings : MySavingModuleBase
		{
			[Command(nameof(ChannelSetting.ImageOnly)), Alias("io")]
			public async Task CommandImageOnly([VerifyChannel(true, ChannelVerification.CanBeEdited)] ITextChannel channel)
			{
				if (Context.GuildSettings.ImageOnlyChannels.Contains(channel.Id))
				{
					Context.GuildSettings.ImageOnlyChannels.Remove(channel.Id);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed the channel `{channel.FormatChannel()}` from the image only list.");
				}
				else
				{
					Context.GuildSettings.ImageOnlyChannels.Add(channel.Id);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully added the channel `{channel.FormatChannel()}` to the image only list.");
				}
			}
		}

		[Group(nameof(ModifyGuildNotifs)), Alias("mgn")]
		[Usage("[Welcome|Goodbye] [Channel] <\"Content:string\"> <\"Title:string\"> <\"Desc:string\"> <\"Thumb:string\">")]
		[Summary("The bot send a message to the given channel when the self explantory event happens. `{User}` will be replaced with the formatted user.  `{UserMention}` will be replaced with a mention of the joining user.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyGuildNotifs : MySavingModuleBase
		{
			[Command(nameof(GuildNotificationType.Welcome)), Alias("w")]
			public async Task CommandWelcome([VerifyChannel(true, ChannelVerification.CanModifyPermissions)] ITextChannel channel, [Remainder] string input)
			{
				var inputArgs = input.SplitByCharExceptInQuotes(' ').ToList();
				var content = GetActions.GetVariableAndRemove(inputArgs, "content");
				var title = GetActions.GetVariableAndRemove(inputArgs, "title");
				var desc = GetActions.GetVariableAndRemove(inputArgs, "desc");
				var thumb = GetActions.GetVariableAndRemove(inputArgs, "thumb");

				Context.GuildSettings.WelcomeMessage = new GuildNotification(content, title, desc, thumb, channel.Id);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the welcome message.");
			}
			[Command(nameof(GuildNotificationType.Goodbye)), Alias("g")]
			public async Task CommandGoodbye([VerifyChannel(true, ChannelVerification.CanModifyPermissions)] ITextChannel channel, [Remainder] string input)
			{
				var inputArgs = input.SplitByCharExceptInQuotes(' ').ToList();
				var content = GetActions.GetVariableAndRemove(inputArgs, "content");
				var title = GetActions.GetVariableAndRemove(inputArgs, "title");
				var desc = GetActions.GetVariableAndRemove(inputArgs, "desc");
				var thumb = GetActions.GetVariableAndRemove(inputArgs, "thumb");

				Context.GuildSettings.GoodbyeMessage = new GuildNotification(content, title, desc, thumb, channel.Id);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the goodbye message.");
			}
		}

		[Group(nameof(TestGuildNotifs)), Alias("tgn")]
		[Usage("[Welcome|Goodbye]")]
		[Summary("Sends the given guild notification in order to test it.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class TestGuildNotifs : MyModuleBase
		{
			[Command(nameof(GuildNotificationType.Welcome)), Alias("w")]
			public async Task CommandWelcome()
			{
				var notif = Context.GuildSettings.WelcomeMessage;
				if (notif == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The welcome notification does not exist."));
					return;
				}

				await MessageActions.SendGuildNotification(null, notif);
			}
			[Command(nameof(GuildNotificationType.Goodbye)), Alias("g")]
			public async Task CommandGoodbye()
			{
				var notif = Context.GuildSettings.GoodbyeMessage;
				if (notif == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The goodbye notification does not exist."));
					return;
				}

				await MessageActions.SendGuildNotification(null, notif);
			}
		}

		[Group(nameof(DisplayGuildSettings)), Alias("dgds")]
		[Usage("<All|Setting Name>")]
		[Summary("Displays guild settings. Inputting nothing gives a list of the setting names.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class DisplayGuildSettings : MyModuleBase
		{
			[Command("all"), Priority(1)]
			public async Task CommandAll()
			{
				var text = FormattingActions.FormatAllGuildSettings(Context.Guild, Context.GuildSettings);
				await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "Guild Settings", "Guild Settings");
			}
			[Command, Priority(0)]
			public async Task Command(string setting)
			{
				var currentSetting = GetActions.GetGuildSettings().FirstOrDefault(x => x.Name.CaseInsEquals(setting));
				if (currentSetting == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Unable to find a setting with the supplied name."));
					return;
				}

				var desc = FormattingActions.FormatGuildSettingInfo(Context.Guild as SocketGuild, Context.GuildSettings, currentSetting);
				if (desc.Length <= Constants.MAX_DESCRIPTION_LENGTH)
				{
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(currentSetting.Name, desc));
				}
				else
				{
					await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, desc, currentSetting.Name, currentSetting.Name);
				}
			}
			[Command]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", GetActions.GetGuildSettings().Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Setting Names", desc));
			}
		}

		[Group(nameof(GetFile)), Alias("gf")]
		[Usage("")]
		[Summary("Sends the file containing all the guild's saved bot information.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class GetFile : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				var file = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_SETTINGS_LOCATION);
				if (!file.Exists)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The guild information file does not exist at this time."));
					return;
				}
				await UploadActions.UploadFile(Context.Channel, file);
			}
		}
	}
}
