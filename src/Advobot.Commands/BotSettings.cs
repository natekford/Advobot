using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Core;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.BotSettings
{
	[Group(nameof(ModifyBotSettings)), TopLevelShortAlias(typeof(ModifyBotSettings))]
	[Summary("Modify the given setting on the bot. " +
		"`Show` lists the setting names. " +
		"`Reset` resets a setting back to default. " +
		"Cannot modify settings through this command if they are lists.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotSettings : BotSettingsSavingModuleBase
	{
		[Group(nameof(Modify)), ShortAlias(nameof(Modify))]
		public sealed class Modify : BotSettingsSavingModuleBase
		{
			[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
			public async Task LogLevel(LogSeverity logLevel)
				=> await CommandRunner(Context, (s) => { s.LogLevel = logLevel; return s.LogLevel; }).CAF();
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task Prefix([VerifyStringLength(Target.Prefix)] string prefix)
				=> await CommandRunner(Context, (s) => { s.Prefix = prefix; return s.Prefix; }).CAF();
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task Game([VerifyStringLength(Target.Game)] string game)
				=> await CommandRunner(Context, (s) => { s.Game = game; return s.Game; }).CAF();
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task Stream([VerifyStringLength(Target.Stream)] string stream)
			{
				if (!RegexUtils.IsValidTwitchName(stream))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{stream}` is not a valid Twitch stream name.")).CAF();
					return;
				}
				await CommandRunner(Context, (s) => { s.Stream = stream; return s.Stream; }).CAF();
			}
			[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
			public async Task AlwaysDownloadUsers(bool downloadUsers)
				=> await CommandRunner(Context, (s) => { s.AlwaysDownloadUsers = downloadUsers; return s.AlwaysDownloadUsers; }).CAF();
			[Command(nameof(IBotSettings.ShardCount)), ShortAlias(nameof(IBotSettings.ShardCount))]
			public async Task ShardCount([VerifyNumber(1, int.MaxValue)] uint count)
			{
				var validNum = (await Context.Client.GetGuildsAsync().CAF()).Count / 2500 + 1;
				if (count < validNum)
				{
					var error = new Error($"With the current amount of guilds the client has, the minimum shard number is: `{validNum}`.");
					await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
					return;
				}
				await CommandRunner(Context, (s) => { s.ShardCount = (int)count; return s.ShardCount; }).CAF();
			}
			[Command(nameof(IBotSettings.MessageCacheCount)), ShortAlias(nameof(IBotSettings.MessageCacheCount))]
			public async Task MessageCacheCount([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner(Context, (s) => { s.MessageCacheCount = (int)count; return s.MessageCacheCount; }).CAF();
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task MaxUserGatherCount([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner(Context, (s) => { s.MaxUserGatherCount = (int)count; return s.MaxUserGatherCount; }).CAF();
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task MaxMessageGatherSize([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxMessageGatherSize = (int)count; return s.MaxMessageGatherSize; }).CAF();
			[Command(nameof(IBotSettings.MaxRuleCategories)), ShortAlias(nameof(IBotSettings.MaxRuleCategories))]
			public async Task MaxRuleCategories([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxRuleCategories = (int)count; return s.MaxRuleCategories; }).CAF();
			[Command(nameof(IBotSettings.MaxRulesPerCategory)), ShortAlias(nameof(IBotSettings.MaxRulesPerCategory))]
			public async Task MaxRulesPerCategory([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxRulesPerCategory = (int)count; return s.MaxRulesPerCategory; }).CAF();
			[Command(nameof(IBotSettings.MaxSelfAssignableRoleGroups)), ShortAlias(nameof(IBotSettings.MaxSelfAssignableRoleGroups))]
			public async Task MaxSelfAssignableRoleGroups([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxSelfAssignableRoleGroups = (int)count; return s.MaxSelfAssignableRoleGroups; }).CAF();
			[Command(nameof(IBotSettings.MaxQuotes)), ShortAlias(nameof(IBotSettings.MaxQuotes))]
			public async Task MaxQuotes([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxQuotes = (int)count; return s.MaxQuotes; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedStrings)), ShortAlias(nameof(IBotSettings.MaxBannedStrings))]
			public async Task MaxBannedStrings([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxBannedStrings = (int)count; return s.MaxBannedStrings; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedRegex)), ShortAlias(nameof(IBotSettings.MaxBannedRegex))]
			public async Task MaxBannedRegex([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxBannedRegex = (int)count; return s.MaxBannedRegex; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedNames)), ShortAlias(nameof(IBotSettings.MaxBannedNames))]
			public async Task MaxBannedNames([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxBannedNames = (int)count; return s.MaxBannedNames; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedPunishments)), ShortAlias(nameof(IBotSettings.MaxBannedPunishments))]
			public async Task MaxBannedPunishments([VerifyNumber(1, int.MaxValue)] uint count) 
				=> await CommandRunner(Context, (s) => { s.MaxBannedPunishments = (int)count; return s.MaxBannedPunishments; }).CAF();

			public static async Task CommandRunner(IAdvobotCommandContext context, Func<IBotSettings, object> func, [CallerMemberName] string property = "")
			{
				var resp = $"Successfully set {property.FormatTitle().ToLower()} to `{func(context.BotSettings)}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(context, resp).CAF();
			}
		}
		[Group(nameof(Reset)), ShortAlias(nameof(Reset))]
		public sealed class Reset : BotSettingsSavingModuleBase
		{
			[Command(nameof(IBotSettings.LogLevel)), ShortAlias(nameof(IBotSettings.LogLevel))]
			public async Task LogLevel() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.LogLevel)));
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task Prefix() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.Prefix)));
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task Game() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.Game)));
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task Stream() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.Stream)));
			[Command(nameof(IBotSettings.AlwaysDownloadUsers)), ShortAlias(nameof(IBotSettings.AlwaysDownloadUsers))]
			public async Task AlwaysDownloadUsers() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.AlwaysDownloadUsers)));
			[Command(nameof(IBotSettings.ShardCount)), ShortAlias(nameof(IBotSettings.ShardCount))]
			public async Task ShardCount() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.ShardCount)));
			[Command(nameof(IBotSettings.MessageCacheCount)), ShortAlias(nameof(IBotSettings.MessageCacheCount))]
			public async Task MessagecacheCount() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MessageCacheCount)));
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task MaxUserGatherCount() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxUserGatherCount)));
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task MaxMessageGatherSize() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxMessageGatherSize)));
			[Command(nameof(IBotSettings.MaxRuleCategories)), ShortAlias(nameof(IBotSettings.MaxRuleCategories))]
			public async Task MaxRuleCategories() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxRuleCategories)));
			[Command(nameof(IBotSettings.MaxRulesPerCategory)), ShortAlias(nameof(IBotSettings.MaxRulesPerCategory))]
			public async Task MaxRulesPerCategory() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxRulesPerCategory)));
			[Command(nameof(IBotSettings.MaxSelfAssignableRoleGroups)), ShortAlias(nameof(IBotSettings.MaxSelfAssignableRoleGroups))]
			public async Task MaxSelfAssignableRoleGroups() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxSelfAssignableRoleGroups)));
			[Command(nameof(IBotSettings.MaxQuotes)), ShortAlias(nameof(IBotSettings.MaxQuotes))]
			public async Task MaxQuotes() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxQuotes)));
			[Command(nameof(IBotSettings.MaxBannedStrings)), ShortAlias(nameof(IBotSettings.MaxBannedStrings))]
			public async Task MaxBannedStrings() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxBannedStrings)));
			[Command(nameof(IBotSettings.MaxBannedRegex)), ShortAlias(nameof(IBotSettings.MaxBannedRegex))]
			public async Task MaxBannedRegex() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxBannedRegex)));
			[Command(nameof(IBotSettings.MaxBannedNames)), ShortAlias(nameof(IBotSettings.MaxBannedNames))]
			public async Task MaxBannedNames() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxBannedNames)));
			[Command(nameof(IBotSettings.MaxBannedPunishments)), ShortAlias(nameof(IBotSettings.MaxBannedPunishments))]
			public async Task MaxBannedPunishments() 
				=> await CommandRunner(Context, (s) => s.ResetSetting(nameof(IBotSettings.MaxBannedPunishments)));

			public static async Task CommandRunner(IAdvobotCommandContext context, Func<IBotSettings, object> func, [CallerMemberName] string property = "")
			{
				var resp = $"Successfully reset {property.FormatTitle().ToLower()} to `{func(context.BotSettings)}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(context, resp).CAF();
			}
		}
	}

	[Group(nameof(DisplayBotSettings)), TopLevelShortAlias(typeof(DisplayBotSettings))]
	[Summary("Displays global settings. " +
		"`Show` gives a list of the setting names.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisplayBotSettings : NonSavingModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Setting Names",
				Description = $"`{String.Join("`, `", SettingsBase.GetSettings(typeof(IBotSettings)).Keys)}`"
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(All)), ShortAlias(nameof(All))]
		public async Task All()
		{
			var text = Context.BotSettings.Format(Context.Client, Context.Guild);
			await MessageUtils.SendTextFileAsync(Context.Channel, text, "Bot Settings", "Bot Settings").CAF();
		}
		[Command, Priority(0)]
		public async Task Command([OverrideTypeReader(typeof(SettingTypeReader.BotSettingTypeReader))] FieldInfo settingName)
		{
			var desc = Context.BotSettings.Format(Context.Client, Context.Guild);
			if (desc.Length <= EmbedBuilder.MaxDescriptionLength)
			{
				var embed = new EmbedWrapper
				{
					Title = settingName.Name,
					Description = desc
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			else
			{
				await MessageUtils.SendTextFileAsync(Context.Channel, desc, settingName.Name, settingName.Name).CAF();
			}
		}
	}

	[Group(nameof(ModifyBotName)), TopLevelShortAlias(typeof(ModifyBotName))]
	[Summary("Changes the bot's name to the given name.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotName : NonSavingModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Name)] string newName)
		{
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = newName).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully changed my username to `{newName}`.").CAF();
		}
	}

	[Group(nameof(ModifyBotIcon)), TopLevelShortAlias(typeof(ModifyBotIcon))]
	[Summary("Changes the bot's icon to the given image. " +
		"The image must be smaller than 2.5MB. " +
		"Inputting nothing removes the bot's icon.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotIcon : NonSavingModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional, Remainder] string url)
		{
			var success = MessageUtils.TryGetImageUrl(Context, url, out var imageUrl, out var error);
			if (!success)
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			if (imageUrl == null)
			{
				await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image()).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the bot's icon.").CAF();
				return;
			}

			var fileInfo = IOUtils.GetServerDirectoryFile(Context.Guild.Id, "BotIcon.png");
			using (var webClient = new WebClient())
			{
				webClient.DownloadFileAsync(imageUrl, fileInfo.FullName);
				webClient.DownloadFileCompleted += async (sender, e) =>
				{
					await ClientUtils.ModifyBotIconAsync(Context.Client, fileInfo).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully changed the bot's icon.").CAF();
					IOUtils.DeleteFile(fileInfo);
				};
			}
		}
	}

	[Group(nameof(ResetBotConfig)), TopLevelShortAlias(typeof(ResetBotConfig))]
	[Summary("Resets bot key, bot id, save path.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ResetBotConfig : NonSavingModuleBase
	{
		[Command]
		public async Task Command()
		{
			Config.Configuration[Config.ConfigDict.ConfigKey.SavePath] = null;
			Config.Configuration[Config.ConfigDict.ConfigKey.BotKey] = null;
			Config.Configuration[Config.ConfigDict.ConfigKey.BotId] = null;
			Config.Save();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully reset all properties. Restarting now...").CAF();
			ClientUtils.RestartBot();
		}
	}

	[Group(nameof(ResetBotKey)), TopLevelShortAlias(typeof(ResetBotKey))]
	[Summary("Removes the currently used bot's key so that a different bot can be used instead.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ResetBotKey : NonSavingModuleBase
	{
		[Command]
		public async Task Command()
		{
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully reset the bot key. Shutting down now...").CAF();
			Config.Configuration[Config.ConfigDict.ConfigKey.BotKey] = null;
			Config.Save();
			ClientUtils.RestartBot();
		}
	}

	[Group(nameof(DisconnectBot)), TopLevelShortAlias(typeof(DisconnectBot), "runescapeservers")]
	[Summary("Turns the bot off.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisconnectBot : NonSavingModuleBase
	{
		[Command]
		public Task Command()
		{
			ClientUtils.DisconnectBot(Context.Client);
			return Task.FromResult(0);
		}
	}

	[Group(nameof(RestartBot)), TopLevelShortAlias(typeof(RestartBot))]
	[Summary("Restarts the bot.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class RestartBot : NonSavingModuleBase
	{
		[Command]
		public Task Command()
		{
			ClientUtils.RestartBot();
			return Task.FromResult(0);
		}
	}
}