﻿using Advobot.Core.Actions;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.BotSettings
{
	[Group(nameof(ModifyBotSettings)), TopLevelShortAlias(typeof(ModifyBotSettings))]
	[Summary("Modify the given setting on the bot. " +
		"`Show` lists the setting names. " +
		"`Clear` resets a setting back to default. " +
		"Cannot modify settings through this command if they are lists.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotSettings : SavingModuleBase
	{
		[Group(nameof(Modify)), ShortAlias(nameof(Modify))]
		public sealed class Modify : SavingModuleBase
		{
			[Command(nameof(IBotSettings.ShardCount)), ShortAlias(nameof(IBotSettings.ShardCount))]
			public async Task CommandShardCount(uint shardCount)
			{
				var guilds = await Context.Client.GetGuildsAsync().CAF();
				var validNum = guilds.Count / 2500 + 1;
				if (shardCount < validNum)
				{
					var error = new ErrorReason($"With the current amount of guilds the client has, the minimum shard number is: `{validNum}`.");
					await MessageActions.SendErrorMessageAsync(Context, error).CAF();
					return;
				}

				Context.BotSettings.ShardCount = (int)shardCount;
				var resp = $"Successfully set the shard amount to `{Context.BotSettings.ShardCount}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.MessageCacheCount)), ShortAlias(nameof(IBotSettings.MessageCacheCount))]
			public async Task CommandMessagecacheCount(uint cacheCount)
			{
				Context.BotSettings.MessageCacheCount = (int)cacheCount;
				var resp = $"Successfully set the message cache count to `{Context.BotSettings.MessageCacheCount}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task CommandMaxUserGatherCount(uint userGatherCount)
			{
				Context.BotSettings.MaxUserGatherCount = (int)userGatherCount;
				var resp = $"Successfully set the max user gather count to `{Context.BotSettings.MaxUserGatherCount}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task CommandMaxMessageGatherSize(uint messageGatherSize)
			{
				Context.BotSettings.MaxMessageGatherSize = (int)messageGatherSize;
				var resp = $"Successfully set the max message gather size to `{Context.BotSettings.MaxMessageGatherSize}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task CommandPrefix([VerifyStringLength(Target.Prefix)] string prefix)
			{
				Context.BotSettings.Prefix = prefix;
				var resp = $"Successfully set the prefix to `{Context.BotSettings.Prefix}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task CommandGame([VerifyStringLength(Target.Game)] string game)
			{
				Context.BotSettings.Game = game;
				var resp = $"Successfully set the game to `{Context.BotSettings.Game}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task CommandStream([VerifyStringLength(Target.Stream)] string stream)
			{
				if (!RegexActions.CheckIfInputIsAValidTwitchName(stream))
				{
					await MessageActions.SendErrorMessageAsync(Context, new ErrorReason($"`{stream}` is not a valid Twitch stream name.")).CAF();
					return;
				}

				Context.BotSettings.Stream = stream;
				var resp = $"Successfully set the game to `{Context.BotSettings.Stream}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
			public async Task CommandAlwaysDownloadUsers(bool downloadUsers)
			{
				Context.BotSettings.AlwaysDownloadUsers = downloadUsers;
				var resp = $"Successfully set always download users to `{Context.BotSettings.AlwaysDownloadUsers}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
			public async Task CommandLogLevel(LogSeverity logLevel)
			{
				Context.BotSettings.LogLevel = logLevel;
				var resp = $"Successfully set the log level to `{Context.BotSettings.LogLevel.EnumName()}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
		[Group(nameof(Clear)), ShortAlias(nameof(Clear))]
		public sealed class Clear : SavingModuleBase
		{
			[Command(nameof(IBotSettings.ShardCount)), ShortAlias(nameof(IBotSettings.ShardCount))]
			public async Task CommandShardCount()
			{
				var guilds = await Context.Client.GetGuildsAsync().CAF();
				Context.BotSettings.ShardCount = guilds.Count / 2500 + 1;
				var resp = $"Successfully set the shard amount to `{Context.BotSettings.ShardCount}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.MessageCacheCount)), ShortAlias(nameof(IBotSettings.MessageCacheCount))]
			public async Task CommandMessagecacheCount()
			{
				Context.BotSettings.MessageCacheCount = -1;
				var resp = $"Successfully set the message cache count to `{Context.BotSettings.MessageCacheCount}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task CommandMaxUserGatherCount()
			{
				Context.BotSettings.MaxUserGatherCount = -1;
				var resp = $"Successfully set the max user gather count to `{Context.BotSettings.MaxUserGatherCount}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task CommandMaxMessageGatherSize()
			{
				Context.BotSettings.MaxMessageGatherSize = -1;
				var resp = $"Successfully set the max message gather size to `{Context.BotSettings.MaxMessageGatherSize}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task CommandPrefix()
			{
				Context.BotSettings.Prefix = null;
				var resp = $"Successfully set the prefix to `{Context.BotSettings.Prefix}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task CommandGame()
			{
				Context.BotSettings.Game = null;
				var resp = $"Successfully set the game to `{Context.BotSettings.Game}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task CommandStream()
			{
				Context.BotSettings.Stream = null;
				var resp = $"Successfully set the game to `{Context.BotSettings.Stream}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
			public async Task CommandAlwaysDownloadUsers()
			{
				Context.BotSettings.AlwaysDownloadUsers = true;
				var resp = $"Successfully set always download users to `{Context.BotSettings.AlwaysDownloadUsers}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
			[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
			public async Task CommandLogLevel()
			{
				Context.BotSettings.LogLevel = LogSeverity.Warning;
				var resp = $"Successfully set the log level to `{Context.BotSettings.LogLevel.EnumName()}`.";
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
			}
		}
	}

	[Group(nameof(DisplayBotSettings)), TopLevelShortAlias(typeof(DisplayBotSettings))]
	[Summary("Displays global settings. " +
		"`Show` gives a list of the setting names.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisplayBotSettings : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", GetActions.GetBotSettings().Select(x => x.Name))}`";
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Setting Names", desc)).CAF();
		}
		[Command(nameof(All)), ShortAlias(nameof(All))]
		public async Task All()
		{
			var text = await Context.BotSettings.Format(Context.Client).CAF();
			await MessageActions.SendTextFileAsync(Context.Channel, text, "Bot Settings", "Bot Settings").CAF();
		}
		[Command, Priority(0)]
		public async Task Command([OverrideTypeReader(typeof(SettingTypeReader.BotSettingTypeReader))] PropertyInfo settingName)
		{
			var desc = await Context.BotSettings.Format(Context.Client, settingName).CAF();
			if (desc.Length <= Constants.MAX_DESCRIPTION_LENGTH)
			{
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed(settingName.Name, desc)).CAF();
			}
			else
			{
				await MessageActions.SendTextFileAsync(Context.Channel, desc, settingName.Name, settingName.Name).CAF();
			}
		}
	}

	[Group(nameof(ModifyBotName)), TopLevelShortAlias(typeof(ModifyBotName))]
	[Summary("Changes the bot's name to the given name.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Name)] string newName)
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = newName).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully changed my username to `{newName}`.").CAF();
		}
	}

	[Group(nameof(ModifyBotIcon)), TopLevelShortAlias(typeof(ModifyBotIcon))]
	[Summary("Changes the bot's icon to the given image. " +
		"The image must be smaller than 2.5MB. " +
		"Inputting nothing removes the bot's icon.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotIcon : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional, Remainder] string url)
		{
			var imageUrl = new ImageUrl(Context, url);
			if (imageUrl.HasErrors)
			{
				await MessageActions.SendErrorMessageAsync(Context, imageUrl.ErrorReason).CAF();
				return;
			}
			else if (imageUrl.Url == null)
			{
				await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image()).CAF();
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the bot's icon.").CAF();
				return;
			}

			var fileInfo = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.BOT_ICON_LOCATION + imageUrl.FileType);
			using (var webClient = new WebClient())
			{
				webClient.DownloadFileAsync(imageUrl.Url, fileInfo.FullName);
				webClient.DownloadFileCompleted += async (sender, e) =>
				{
					await ClientActions.ModifyBotIconAsync(Context.Client, fileInfo).CAF();
					await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully changed the bot's icon.").CAF();
					SavingAndLoadingActions.DeleteFile(fileInfo);
				};
			}
		}
	}

	[Group(nameof(ResetBotConfig)), TopLevelShortAlias(typeof(ResetBotConfig))]
	[Summary("Resets bot key, bot id, save path.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ResetBotConfig : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			Config.Configuration[ConfigKey.SavePath] = null;
			Config.Configuration[ConfigKey.BotKey] = null;
			Config.Configuration[ConfigKey.BotId] = null;
			Config.Save();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully reset all properties. Restarting now...").CAF();
			ClientActions.RestartBot();
		}
	}

	[Group(nameof(ResetBotKey)), TopLevelShortAlias(typeof(ResetBotKey))]
	[Summary("Removes the currently used bot's key so that a different bot can be used instead.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ResetBotKey : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully reset the bot key. Shutting down now...").CAF();
			Config.Configuration[ConfigKey.BotKey] = null;
			Config.Save();
			ClientActions.RestartBot();
		}
	}

	[Group(nameof(DisconnectBot)), TopLevelShortAlias(typeof(DisconnectBot), "runescapeservers")]
	[Summary("Turns the bot off.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisconnectBot : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await ClientActions.DisconnectBotAsync(Context.Client).CAF();
		}
	}

	[Group(nameof(RestartBot)), TopLevelShortAlias(typeof(RestartBot))]
	[Summary("Restarts the bot.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class RestartBot : AdvobotModuleBase
	{
		[Command]
		public Task Command()
		{
			ClientActions.RestartBot();
			return Task.FromResult(0);
		}
	}
}