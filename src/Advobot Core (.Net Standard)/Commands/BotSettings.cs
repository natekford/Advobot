using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Commands.BotSettings
{
	[Group(nameof(ModifyBotSettings)), TopLevelShortAlias(nameof(ModifyBotSettings))]
	[Usage("[Show|Modify|Clear] [Setting Name] <New Value>")]
	[Summary("Modify the given setting on the bot. Show lists the setting names. Clear resets a setting back to default. Cannot modify settings which are lists through this command.")]
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
				var validNum = (await Context.Client.GetGuildsAsync()).Count / 2500 + 1;
				if (shardCount < validNum)
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason($"With the current amount of guilds the client has, the minimum shard number is: `{validNum}`."));
					return;
				}

				Context.BotSettings.ShardCount = (int)shardCount;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the shard amount to `{Context.BotSettings.ShardCount}`.");
			}
			[Command(nameof(IBotSettings.MessageCacheCount)), ShortAlias(nameof(IBotSettings.MessageCacheCount))]
			public async Task CommandMessagecacheCount(uint cacheCount)
			{
				Context.BotSettings.MessageCacheCount = (int)cacheCount;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the message cache count to `{Context.BotSettings.MessageCacheCount}`.");
			}
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task CommandMaxUserGatherCount(uint userGatherCount)
			{
				Context.BotSettings.MaxUserGatherCount = (int)userGatherCount;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max user gather count to `{Context.BotSettings.MaxUserGatherCount}`.");
			}
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task CommandMaxMessageGatherSize(uint messageGatherSize)
			{
				Context.BotSettings.MaxMessageGatherSize = (int)messageGatherSize;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max message gather size to `{Context.BotSettings.MaxMessageGatherSize}`.");
			}
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task CommandPrefix([VerifyStringLength(Target.Prefix)] string prefix)
			{
				Context.BotSettings.Prefix = prefix;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the prefix to `{Context.BotSettings.Prefix}`.");
			}
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task CommandGame([VerifyStringLength(Target.Game)] string game)
			{
				Context.BotSettings.Game = game;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `{Context.BotSettings.Game}`.");
			}
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task CommandStream([VerifyStringLength(Target.Stream)] string stream)
			{
				if (!RegexActions.CheckIfInputIsAValidTwitchName(stream))
				{
					await MessageActions.SendErrorMessage(Context, new ErrorReason($"`{stream}` is not a valid Twitch stream name."));
					return;
				}

				Context.BotSettings.Stream = stream;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `{Context.BotSettings.Stream}`.");
			}
			[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
			public async Task CommandAlwaysDownloadUsers(bool downloadUsers)
			{
				Context.BotSettings.AlwaysDownloadUsers = downloadUsers;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set always download users to `{Context.BotSettings.AlwaysDownloadUsers}`.");
			}
			[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
			public async Task CommandLogLevel(LogSeverity logLevel)
			{
				Context.BotSettings.LogLevel = logLevel;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the log level to `{Context.BotSettings.LogLevel.EnumName()}`.");
			}
		}
		[Group(nameof(Clear)), ShortAlias(nameof(Clear))]
		public sealed class Clear : SavingModuleBase
		{
			[Command(nameof(IBotSettings.ShardCount)), ShortAlias(nameof(IBotSettings.ShardCount))]
			public async Task CommandShardCount()
			{
				Context.BotSettings.ShardCount = (await Context.Client.GetGuildsAsync()).Count / 2500 + 1;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the shard amount to `{Context.BotSettings.ShardCount}`.");
			}
			[Command(nameof(IBotSettings.MessageCacheCount)), ShortAlias(nameof(IBotSettings.MessageCacheCount))]
			public async Task CommandMessagecacheCount()
			{
				Context.BotSettings.MessageCacheCount = -1;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the message cache count to `{Context.BotSettings.MessageCacheCount}`.");
			}
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task CommandMaxUserGatherCount()
			{
				Context.BotSettings.MaxUserGatherCount = -1;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max user gather count to `{Context.BotSettings.MaxUserGatherCount}`.");
			}
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task CommandMaxMessageGatherSize()
			{
				Context.BotSettings.MaxMessageGatherSize = -1;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max message gather size to `{Context.BotSettings.MaxMessageGatherSize}`.");
			}
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task CommandPrefix()
			{
				Context.BotSettings.Prefix = null;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the prefix to `{Context.BotSettings.Prefix}`.");
			}
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task CommandGame()
			{
				Context.BotSettings.Game = null;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `{Context.BotSettings.Game}`.");
			}
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task CommandStream()
			{
				Context.BotSettings.Stream = null;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `{Context.BotSettings.Stream}`.");
			}
			[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
			public async Task CommandAlwaysDownloadUsers()
			{
				Context.BotSettings.AlwaysDownloadUsers = true;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set always download users to `{Context.BotSettings.AlwaysDownloadUsers}`.");
			}
			[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
			public async Task CommandLogLevel()
			{
				Context.BotSettings.LogLevel = LogSeverity.Warning;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the log level to `{Context.BotSettings.LogLevel.EnumName()}`.");
			}
		}
	}

	[Group(nameof(DisplayBotSettings)), TopLevelShortAlias(nameof(DisplayBotSettings))]
	[Usage("[Show|All|Setting Name]")]
	[Summary("Displays global settings. Show gives a list of the setting names.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisplayBotSettings : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var desc = $"`{String.Join("`, `", GetActions.GetBotSettings().Select(x => x.Name))}`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Setting Names", desc));
		}
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All()
		{
			var text = await Context.BotSettings.ToString(Context.Client);
			await MessageActions.SendTextFile(Context.Channel, text, "Bot Settings", "Bot Settings");
		}
		[Command, Priority(0)]
		public async Task Command([OverrideTypeReader(typeof(SettingTypeReader.BotSettingTypeReader))] PropertyInfo settingName)
		{
			var desc = await Context.BotSettings.ToString(Context.Client, settingName);
			if (desc.Length <= Constants.MAX_DESCRIPTION_LENGTH)
			{
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(settingName.Name, desc));
			}
			else
			{
				await MessageActions.SendTextFile(Context.Channel, desc, settingName.Name, settingName.Name);
			}
		}
	}

	[Group(nameof(ModifyBotName)), TopLevelShortAlias(nameof(ModifyBotName))]
	[Usage("[New Name]")]
	[Summary("Changes the bot's name to the given name.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Name)] string newName)
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = newName);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed my username to `{newName}`.");
		}
	}

	[Group(nameof(ModifyBotIcon)), TopLevelShortAlias(nameof(ModifyBotIcon))]
	[Usage("<Attached Image|Embedded Image>")]
	[Summary("Changes the bot's icon to the given image. The image must be smaller than 2.5MB. Inputting nothing removes the bot's icon.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotIcon : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			var attach = Context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
			var embeds = Context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
			var validImages = attach.Concat(embeds);
			if (!validImages.Any())
			{
				await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image());
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the bot's icon.");
				return;
			}
			else if (validImages.Count() > 1)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("Too many attached or embedded images."));
				return;
			}

			var imageUrl = validImages.First();
			if (!GetActions.TryGetFileType(Context, imageUrl, out string fileType, out string errorReason))
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason(errorReason));
				return;
			}

			var fileInfo = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.BOT_ICON_LOCATION + fileType);
			using (var webClient = new WebClient())
			{
				webClient.DownloadFileAsync(new Uri(imageUrl), fileInfo.FullName);
				webClient.DownloadFileCompleted += async (sender, e) =>
				{
					await ClientActions.ModifyBotIconAsync(Context.Client, fileInfo);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully changed the bot's icon.");
					SavingAndLoadingActions.DeleteFile(fileInfo);
				};
			}
		}
	}

	[Group(nameof(ResetBotConfig)), TopLevelShortAlias(nameof(ResetBotConfig))]
	[Usage("")]
	[Summary("Resets bot key, bot Id, save path.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ResetBotConfig : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully reset all properties. Restarting now...");
			Config.Configuration[Config.ConfigKeys.Save_Path] = null;
			Config.Configuration[Config.ConfigKeys.Bot_Key] = null;
			Config.Configuration[Config.ConfigKeys.Bot_Id] = null;
			Config.Save();
			ClientActions.RestartBot();
		}
	}

	[Group(nameof(ResetBotKey)), TopLevelShortAlias(nameof(ResetBotKey))]
	[Usage("")]
	[Summary("Remove's the currently used bot's key so that a different bot can be used instead.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ResetBotKey : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully reset the bot key. Shutting down now...");
			Config.Configuration[Config.ConfigKeys.Bot_Key] = null;
			Config.Save();
			ClientActions.RestartBot();
		}
	}

	[Group(nameof(DisconnectBot)), TopLevelShortAlias(nameof(DisconnectBot), "runescapeservers")]
	[Usage("")]
	[Summary("Turns the bot off.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisconnectBot : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await ClientActions.DisconnectBot(Context.Client);
		}
	}

	[Group(nameof(RestartBot)), TopLevelShortAlias(nameof(RestartBot))]
	[Usage("")]
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