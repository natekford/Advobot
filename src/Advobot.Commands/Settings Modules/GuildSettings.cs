using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Commands.Misc;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Commands.GuildSettings
{
	[Category(typeof(ModifyGuildPrefix)), Group(nameof(ModifyGuildPrefix)), TopLevelShortAlias(typeof(ModifyGuildPrefix))]
	[Summary("Makes the bot use the given prefix in the guild.")]
	[OtherRequirement(Precondition.GuildOwner)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyGuildPrefix : AdvobotModuleBase
	{
		[Command(nameof(Clear)), Priority(1)]
		public async Task Clear()
		{
			Context.GuildSettings.Prefix = null;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully cleared the guild's prefix.").CAF();
		}
		[Command]
		public async Task Command([ValidateString(Target.Prefix)] string newPrefix)
		{
			Context.GuildSettings.Prefix = newPrefix;
			var resp = $"Successfully set this guild's prefix to: `{Context.GuildSettings.Prefix}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyCommands)), Group(nameof(ModifyCommands)), TopLevelShortAlias(typeof(ModifyCommands))]
	[Summary("Turns a command on or off. " +
		"Can turn all commands in a category on or off too. " +
		"Cannot turn off `" + nameof(ModifyCommands) + "` or `" + nameof(Help) + "`.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true, false)]
	[SaveGuildSettings]
	[RequiredServices(typeof(IHelpEntryService))]
	public sealed class ModifyCommands : AdvobotModuleBase
	{
		[Group(nameof(Enable)), ShortAlias(nameof(Enable))]
		public sealed class Enable : AdvobotModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
			public async Task All()
			{
				var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
				var values = helpEntries.Select(x => new ValueToModify(x, true));
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
				var text = commands.Any() ? string.Join("`, `", commands) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully enabled the following commands: `{text}`.").CAF();
			}
			[Command(nameof(Category)), ShortAlias(nameof(Category)), Priority(1)]
			public async Task Category(string category)
			{
				var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
				if (!helpEntries.GetCategories().CaseInsContains(category))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
					return;
				}
				//Only grab commands that are already disabled and in the same category and are able to be changed.
				var values = helpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, true));
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
				var text = commands.Any() ? string.Join("`, `", commands.Select(x => x)) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully enabled the following commands: `{text}`.").CAF();
			}
			[Command]
			public async Task Command(IHelpEntry command)
			{
				if (!command.AbleToBeToggled)
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"{command.Name} cannot be edited.")).CAF();
					return;
				}
				if (!Context.GuildSettings.CommandSettings.ModifyCommandValue(new ValueToModify(command, true)))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"{command.Name} is already enabled.")).CAF();
					return;
				}
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully enabled `{command.Name}`.").CAF();
			}
		}
		[Group(nameof(Disable)), ShortAlias(nameof(Disable))]
		public sealed class Disable : AdvobotModuleBase
		{
			[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
			public async Task All()
			{
				//Only grab commands that are already enabled and are able to be changed.
				var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
				var values = helpEntries.Select(x => new ValueToModify(x, false));
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
				var text = commands.Any() ? string.Join("`, `", commands.Select(x => x)) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully disabled the following commands: `{text}`.").CAF();
			}
			[Command(nameof(Category)), ShortAlias(nameof(Category)), Priority(1)]
			public async Task Category(string category)
			{
				var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
				if (!helpEntries.GetCategories().CaseInsContains(category))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
					return;
				}
				//Only grab commands that are already enabled and in the same category and are able to be changed.
				var values = helpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, false));
				var commands = Context.GuildSettings.CommandSettings.ModifyCommandValues(values);
				var text = commands.Any() ? string.Join("`, `", commands.Select(x => x)) : "None";
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully disabled the following commands: `{text}`.").CAF();
			}
			[Command]
			public async Task Command(IHelpEntry command)
			{
				if (!command.AbleToBeToggled)
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"{command.Name} cannot be edited.")).CAF();
					return;
				}
				if (!Context.GuildSettings.CommandSettings.ModifyCommandValue(new ValueToModify(command, false)))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"{command.Name} is already disabled.")).CAF();
					return;
				}
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully disabled `{command.Name}`.").CAF();
			}
		}
	}

	[Category(typeof(ModifyIgnoredCommandChannels)), Group(nameof(ModifyIgnoredCommandChannels)), TopLevelShortAlias(typeof(ModifyIgnoredCommandChannels))]
	[Summary("The bot will ignore commands said on these channels. " +
		"If a command is input then the bot will instead ignore only that command on the given channel.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true, false)]
	[SaveGuildSettings]
	public sealed class ModifyIgnoredCommandChannels : AdvobotModuleBase
	{
		[Group(nameof(Enable)), ShortAlias(nameof(Enable))]
		public sealed class Enable : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeEdited)] ITextChannel channel)
			{
				if (!Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
				{
					var error = new Error($"`{channel.Format()}` is already allowing commands.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.IgnoredCommandChannels.RemoveAll(x => x == channel.Id);
				var resp = $"Successfully removed `{channel.Format()}` from the ignored command channels list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(Category)), ShortAlias(nameof(Category)), Priority(1)]
			[RequiredServices(typeof(IHelpEntryService))]
			public async Task Category([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeEdited)] ITextChannel channel, string category)
			{
				var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
				if (!helpEntries.GetCategories().CaseInsContains(category))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
					return;
				}
				var values = helpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, true));
				var commands = Context.GuildSettings.CommandSettings.ModifyOverrides(values, channel);
				var resp = $"Successfully stopped ignoring the following commands on `{channel.Format()}`: `{string.Join("`, `", commands)}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			[RequiredServices(typeof(IHelpEntryService))]
			public async Task Command([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeEdited)] ITextChannel channel, IHelpEntry helpEntry)
			{
				if (!Context.GuildSettings.CommandSettings.ModifyOverride(new ValueToModify(helpEntry, true), channel))
				{
					var error = new Error($"`{helpEntry.Name}` is already unignored on `{channel.Format()}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}
				var resp = $"Successfully stopped ignoring the command `{helpEntry.Name}` on `{channel.Format()}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		[Group(nameof(Disable)), ShortAlias(nameof(Disable))]
		public sealed class Disable : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeEdited)] ITextChannel channel)
			{
				if (Context.GuildSettings.IgnoredCommandChannels.Contains(channel.Id))
				{
					var error = new Error($"`{channel.Format()}` is already ignoring commands.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.IgnoredCommandChannels.Add(channel.Id);
				var resp = $"Successfully added `{channel.Format()}` to the ignored command channels list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(Category)), ShortAlias(nameof(Category)), Priority(1)]
			[RequiredServices(typeof(IHelpEntryService))]
			public async Task Category([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeEdited)] ITextChannel channel, string category)
			{
				var helpEntries = Context.Provider.GetRequiredService<IHelpEntryService>();
				if (!helpEntries.GetCategories().CaseInsContains(category))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{category}` is not a valid category.")).CAF();
					return;
				}
				var values = helpEntries.GetHelpEntries(category).Select(x => new ValueToModify(x, false));
				var commands = Context.GuildSettings.CommandSettings.ModifyOverrides(values, channel);
				var resp = $"Successfully started disabled the following commands on `{channel.Format()}`: `{string.Join("`, `", commands)}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command]
			[RequiredServices(typeof(IHelpEntryService))]
			public async Task Command([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeEdited)] ITextChannel channel, IHelpEntry helpEntry)
			{
				if (!Context.GuildSettings.CommandSettings.ModifyOverride(new ValueToModify(helpEntry, false), channel))
				{
					var error = new Error($"`{helpEntry.Name}` is already ignored on `{channel.Format()}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}
				var resp = $"Successfully started ignoring the command `{helpEntry.Name}` on `{channel.Format()}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Category(typeof(ModifyBotUsers)), Group(nameof(ModifyBotUsers)), TopLevelShortAlias(typeof(ModifyBotUsers))]
	[Summary("Gives a user permissions in the bot but not on Discord itself. " +
		"Type `" + nameof(ModifyBotUsers) + " [" + nameof(Show) + "]` to see the available permissions. " +
		"Type `" + nameof(ModifyBotUsers) + " [" + nameof(Show) + "] [User]` to see the permissions of that user.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyBotUsers : AdvobotModuleBase
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var embed = new EmbedWrapper
				{
					Title = "Bot Permissions",
					Description = $"`{string.Join("`, `", Enum.GetNames(typeof(GuildPermission)))}`"
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			[Command]
			public async Task Command(IUser user)
			{
				var botUser = Context.GuildSettings.BotUsers.SingleOrDefault(x => x.UserId == user.Id);
				if (botUser == null || botUser.Permissions == 0)
				{
					var error = new Error("That user has no extra permissions from the bot.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = $"Permissions for {user.Format()}",
					Description = $"`{string.Join("`, `", EnumUtils.GetFlagNames((GuildPermission)botUser.Permissions))}`"
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
		}
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var botUser = Context.GuildSettings.BotUsers.SingleOrDefault(x => x.UserId == user.Id);
			if (botUser == null)
			{
				Context.GuildSettings.BotUsers.Add(botUser = new BotImplementedPermissions(user.Id, permissions));
			}

			permissions |= ((IGuildUser)Context.User).GuildPermissions.RawValue;
			botUser.AddPermissions(permissions);

			var givenPerms = string.Join("`, `", EnumUtils.GetFlagNames((GuildPermission)permissions));
			var resp = $"Successfully gave `{user.Format()}` the following bot permissions: `{givenPerms}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(IUser user, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			permissions |= ((IGuildUser)Context.User).GuildPermissions.RawValue;

			var botUser = Context.GuildSettings.BotUsers.FirstOrDefault(x => x.UserId == user.Id);
			if (botUser == null)
			{
				var error = new Error($"`{user.Format()}` does not have any bot permissions to remove");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			botUser.RemovePermissions(permissions);

			var takenPerms = string.Join("`, `", EnumUtils.GetFlagNames((GuildPermission)permissions));
			var resp = $"Successfully removed the following bot permissions from `{user.Format()}`: `{takenPerms}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyPersistentRoles)), Group(nameof(ModifyPersistentRoles)), TopLevelShortAlias(typeof(ModifyPersistentRoles))]
	[Summary("Gives a user a role that stays even when they leave and rejoin the server. " +
		"Type `" + nameof(ModifyPersistentRoles) + " [" + nameof(Show) + "]`` to see the which users have persistent roles set up. " +
		"Type `" + nameof(ModifyPersistentRoles) + " [" + nameof(Show) + "]` [User]` to see the persistent roles of that user.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	[SaveGuildSettings]
	public sealed class ModifyPersistentRoles : AdvobotModuleBase
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var roles = Context.GuildSettings.PersistentRoles;
				if (!roles.Any())
				{
					var error = new Error("The guild does not have any persistent roles.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = "Persistent Roles",
					Description = roles.FormatNumberedList(x => x.ToString(Context.Guild as SocketGuild))
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			[Command]
			public async Task Command(IUser user)
			{
				var roles = Context.GuildSettings.PersistentRoles.Where(x => x.UserId == user.Id).ToList();
				if (!roles.Any())
				{
					var error = new Error($"The user `{user.Format()}` does not have any persistent roles.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				var embed = new EmbedWrapper
				{
					Title = "Persistent Roles",
					Description = roles.FormatNumberedList(x => x.ToString(Context.Guild as SocketGuild))
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
		}
		[Group(nameof(Add)), ShortAlias(nameof(Add))]
		public sealed class Add : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command(
				[ValidateObject(false, Verif.CanBeEdited)] IUser user,
				[ValidateObject(false, Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(user.Id, role).CAF();
			[Command] //Should go into the above one if a valid user, so should be fine to not check this one for permission
			public async Task Command(ulong userId, [ValidateObject(false, Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(userId, role).CAF();

			private async Task CommandRunner(ulong userId, IRole role)
			{
				var match = Context.GuildSettings.PersistentRoles.SingleOrDefault(x => x.UserId == userId && x.RoleId == role.Id);
				if (match != null)
				{
					var error = new Error($"A persistent role already exists for the user id {userId} with the role {role.Format()}.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.PersistentRoles.Add(new PersistentRole(userId, role));
				var resp = $"Successfully added a persistent role for the user id {userId} with the role {role.Format()}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		[Group(nameof(Remove)), ShortAlias(nameof(Remove))]
		public sealed class Remove : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command(
				[ValidateObject(false, Verif.CanBeEdited)] IUser user,
				[ValidateObject(false, Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(user.Id, role).CAF();
			[Command] //Should go into the above one if a valid user, so should be fine to not check this one for permission
			public async Task Command(ulong userId, [ValidateObject(false, Verif.CanBeEdited)] IRole role)
				=> await CommandRunner(userId, role).CAF();

			private async Task CommandRunner(ulong userId, IRole role)
			{
				var match = Context.GuildSettings.PersistentRoles.SingleOrDefault(x => x.UserId == userId && x.RoleId == role.Id);
				if (match == null)
				{
					var error = new Error($"No persistent role exists for the user id {userId} with the role {role.Format()}.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.GuildSettings.PersistentRoles.Remove(match);
				var resp = $"Successfully removed the persistent role for the user id {userId} with the role {role.Format()}.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Category(typeof(ModifyChannelSettings)), Group(nameof(ModifyChannelSettings)), TopLevelShortAlias(typeof(ModifyChannelSettings))]
	[Summary("Image only works solely on attachments. " +
		"Using the command on an already targetted channel turns it off.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyChannelSettings : AdvobotModuleBase
	{
		[Command(nameof(ImageOnly)), ShortAlias(nameof(ImageOnly))]
		public async Task ImageOnly([ValidateObject(true, Verif.CanBeEdited)] ITextChannel channel)
		{
			if (Context.GuildSettings.ImageOnlyChannels.Contains(channel.Id))
			{
				Context.GuildSettings.ImageOnlyChannels.Remove(channel.Id);
				var resp = $"Successfully removed the channel `{channel.Format()}` from the image only list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			else
			{
				Context.GuildSettings.ImageOnlyChannels.Add(channel.Id);
				var resp = $"Successfully added the channel `{channel.Format()}` to the image only list.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Category(typeof(ModifyGuildNotifs)), Group(nameof(ModifyGuildNotifs)), TopLevelShortAlias(typeof(ModifyGuildNotifs))]
	[Summary("The bot send a message to the given channel when the self explantory event happens. " +
		"`" + GuildNotification.USER_MENTION + "` will be replaced with the formatted user. " +
		"`" + GuildNotification.USER_STRING + "` will be replaced with a mention of the joining user.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	[SaveGuildSettings]
	public sealed class ModifyGuildNotifs : AdvobotModuleBase
	{
		[Command(nameof(Welcome)), ShortAlias(nameof(Welcome))]
		public async Task Welcome([ValidateObject(true, Verif.CanModifyPermissions)] ITextChannel channel, [Remainder] NamedArguments<GuildNotification> args)
		{
			if (!args.TryCreateObject(new object[] { channel }, out var obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			Context.GuildSettings.WelcomeMessage = obj;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully set the welcome message.").CAF();
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye([ValidateObject(true, Verif.CanModifyPermissions)] ITextChannel channel, [Remainder] NamedArguments<GuildNotification> args)
		{
			if (!args.TryCreateObject(new object[] { channel }, out var obj, out var error))
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			Context.GuildSettings.GoodbyeMessage = obj;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully set the goodbye message.").CAF();
		}
	}

	[Category(typeof(TestGuildNotifs)), Group(nameof(TestGuildNotifs)), TopLevelShortAlias(typeof(TestGuildNotifs))]
	[Summary("Sends the given guild notification in order to test it.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class TestGuildNotifs : AdvobotModuleBase
	{
		[Command(nameof(Welcome)), ShortAlias(nameof(Welcome))]
		public async Task Welcome()
		{
			var notif = Context.GuildSettings.WelcomeMessage;
			if (notif == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The welcome notification does not exist.")).CAF();
				return;
			}

			await notif.SendAsync(Context.Guild as SocketGuild, null).CAF();
		}
		[Command(nameof(Goodbye)), ShortAlias(nameof(Goodbye))]
		public async Task Goodbye()
		{
			var notif = Context.GuildSettings.GoodbyeMessage;
			if (notif == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The goodbye notification does not exist.")).CAF();
				return;
			}

			await notif.SendAsync(Context.Guild as SocketGuild, null).CAF();
		}
	}

	[Category(typeof(DisplayGuildSettings)), Group(nameof(DisplayGuildSettings)), TopLevelShortAlias(typeof(DisplayGuildSettings))]
	[Summary("Displays guild settings.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayGuildSettings : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Setting Names",
				Description = $"`{string.Join("`, `", ((ISettingsBase)Context.GuildSettings).GetSettings().Keys)}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All()
		{
			var tf = new TextFileInfo
			{
				Name = "Guild_Settings",
				Text = Context.GuildSettings.ToString(Context.Client, Context.Guild).RemoveAllMarkdown(),
			};
			await MessageUtils.SendMessageAsync(Context.Channel, "**Guild Settings:**", textFile: tf).CAF();
		}
		[Command]
		public async Task Command(string settingName)
		{
			if (!((ISettingsBase)Context.GuildSettings).GetSettings().TryGetValue(settingName, out var field))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{settingName}` is not a valid setting.")).CAF();
				return;
			}

			var desc = Context.GuildSettings.ToString(Context.Client, Context.Guild, field.Name);
			if (desc.Length <= EmbedBuilder.MaxDescriptionLength)
			{
				var embed = new EmbedWrapper
				{
					Title = settingName,
					Description = desc
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			else
			{
				var tf = new TextFileInfo
				{
					Name = settingName,
					Text = desc,
				};
				await MessageUtils.SendMessageAsync(Context.Channel, $"**{settingName.FormatTitle()}:**", textFile: tf).CAF();
			}
		}
	}

	[Category(typeof(GetFile)), Group(nameof(GetFile)), TopLevelShortAlias(typeof(GetFile))]
	[Summary("Sends the file containing all the guild's settings.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class GetFile : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var file = Context.BotSettings.GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{Context.Guild.Id}.json"));
			if (!file.Exists)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The guild settings file does not exist.")).CAF();
				return;
			}
			await Context.Channel.SendFileAsync(file.FullName).CAF();
		}
	}
}
