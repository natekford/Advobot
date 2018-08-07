using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.ImageResizing;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.BotSettings
{
	//TODO: this for low level config
	[Group(nameof(ModifyBotSettings)), TopLevelShortAlias(typeof(ModifyBotSettings))]
	[Summary("Modify the given setting on the bot. " +
		"`Show` lists the setting names. " +
		"`Reset` resets a setting back to default. " +
		"For lists, a boolean indicating whether or not to add has to be included before the value.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	[SaveBotSettings]
	public sealed class ModifyBotSettings : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show))]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Setting Names",
				Description = $"`{String.Join("`, `", Context.BotSettings.GetSettings().Keys)}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(Reset)), ShortAlias(nameof(Reset))]
		public async Task Reset(string settingName)
		{
			if (!Context.BotSettings.GetSettings().TryGetValue(settingName, out var field))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{settingName}` is not a valid setting.")).CAF();
				return;
			}

			var resp = $"Successfully reset {settingName.FormatTitle().ToLower()} to `{Context.BotSettings.ResetSetting(field.Name)}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Group(nameof(Modify)), ShortAlias(nameof(Modify))]
		public sealed class Modify : AdvobotModuleBase
		{
			[Command(nameof(IBotSettings.Prefix)), ShortAlias(nameof(IBotSettings.Prefix))]
			public async Task Prefix([VerifyStringLength(Target.Prefix)] string prefix)
				=> await CommandRunner((s) => { s.Prefix = prefix; return s.Prefix; }).CAF();
			[Command(nameof(IBotSettings.Game)), ShortAlias(nameof(IBotSettings.Game))]
			public async Task Game([VerifyStringLength(Target.Game)] string game)
				=> await CommandRunner((s) => { s.Game = game; return s.Game; }).CAF();
			[Command(nameof(IBotSettings.Stream)), ShortAlias(nameof(IBotSettings.Stream))]
			public async Task Stream([VerifyStringLength(Target.Stream)] string stream)
			{
				if (!RegexUtils.IsValidTwitchName(stream))
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{stream}` is not a valid Twitch stream name.")).CAF();
					return;
				}
				await CommandRunner((s) => { s.Stream = stream; return s.Stream; }).CAF();
			}
			[Command(nameof(IBotSettings.MaxUserGatherCount)), ShortAlias(nameof(IBotSettings.MaxUserGatherCount))]
			public async Task MaxUserGatherCount([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxUserGatherCount = (int)count; return s.MaxUserGatherCount; }).CAF();
			[Command(nameof(IBotSettings.MaxMessageGatherSize)), ShortAlias(nameof(IBotSettings.MaxMessageGatherSize))]
			public async Task MaxMessageGatherSize([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxMessageGatherSize = (int)count; return s.MaxMessageGatherSize; }).CAF();
			[Command(nameof(IBotSettings.MaxRuleCategories)), ShortAlias(nameof(IBotSettings.MaxRuleCategories))]
			public async Task MaxRuleCategories([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxRuleCategories = (int)count; return s.MaxRuleCategories; }).CAF();
			[Command(nameof(IBotSettings.MaxRulesPerCategory)), ShortAlias(nameof(IBotSettings.MaxRulesPerCategory))]
			public async Task MaxRulesPerCategory([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxRulesPerCategory = (int)count; return s.MaxRulesPerCategory; }).CAF();
			[Command(nameof(IBotSettings.MaxSelfAssignableRoleGroups)), ShortAlias(nameof(IBotSettings.MaxSelfAssignableRoleGroups))]
			public async Task MaxSelfAssignableRoleGroups([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxSelfAssignableRoleGroups = (int)count; return s.MaxSelfAssignableRoleGroups; }).CAF();
			[Command(nameof(IBotSettings.MaxQuotes)), ShortAlias(nameof(IBotSettings.MaxQuotes))]
			public async Task MaxQuotes([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxQuotes = (int)count; return s.MaxQuotes; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedStrings)), ShortAlias(nameof(IBotSettings.MaxBannedStrings))]
			public async Task MaxBannedStrings([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxBannedStrings = (int)count; return s.MaxBannedStrings; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedRegex)), ShortAlias(nameof(IBotSettings.MaxBannedRegex))]
			public async Task MaxBannedRegex([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxBannedRegex = (int)count; return s.MaxBannedRegex; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedNames)), ShortAlias(nameof(IBotSettings.MaxBannedNames))]
			public async Task MaxBannedNames([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxBannedNames = (int)count; return s.MaxBannedNames; }).CAF();
			[Command(nameof(IBotSettings.MaxBannedPunishments)), ShortAlias(nameof(IBotSettings.MaxBannedPunishments))]
			public async Task MaxBannedPunishments([VerifyNumber(1, int.MaxValue)] uint count)
				=> await CommandRunner((s) => { s.MaxBannedPunishments = (int)count; return s.MaxBannedPunishments; }).CAF();
			[Command(nameof(IBotSettings.TrustedUsers)), ShortAlias(nameof(IBotSettings.TrustedUsers))]
			public async Task TrustedUsers(bool add, ulong value)
				=> await CommandRunner(Context.BotSettings.TrustedUsers, value, add).CAF();
			[Command(nameof(IBotSettings.UsersUnableToDmOwner)), ShortAlias(nameof(IBotSettings.UsersUnableToDmOwner))]
			public async Task UsersUnableToDmOwner(bool add, ulong value)
				=> await CommandRunner(Context.BotSettings.UsersUnableToDmOwner, value, add).CAF();
			[Command(nameof(IBotSettings.UsersIgnoredFromCommands)), ShortAlias(nameof(IBotSettings.UsersIgnoredFromCommands))]
			public async Task UsersIgnoredFromCommands(bool add, ulong value)
				=> await CommandRunner(Context.BotSettings.UsersIgnoredFromCommands, value, add).CAF();

			private async Task CommandRunner<T>(List<T> list, T obj, bool add, [CallerMemberName] string field = "")
			{
				if (add)
				{
					list.Add(obj);
					var resp = $"Successfully added `{obj}` to {field.FormatTitle().ToLower()}.";
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
				}
				else
				{
					list.Remove(obj);
					var resp = $"Successfully removed `{obj}` from {field.FormatTitle().ToLower()}.";
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
				}
			}
			private async Task CommandRunner(Func<IBotSettings, object> func, [CallerMemberName] string field = "")
			{
				var resp = $"Successfully set {field.FormatTitle().ToLower()} to `{func(Context.BotSettings)}`.";
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Setting Names",
				Description = $"`{String.Join("`, `", Context.BotSettings.GetSettings().Keys)}`"
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command(nameof(All)), ShortAlias(nameof(All)), Priority(1)]
		public async Task All()
		{
			var tf = new TextFileInfo
			{
				Name = "Bot_Settings",
				Text = Context.BotSettings.ToString(Context.Client, Context.Guild),
			};
			await MessageUtils.SendMessageAsync(Context.Channel, "**Bot Settings:**", textFile: tf).CAF();
		}
		[Command]
		public async Task Command(string settingName)
		{
			if (!Context.BotSettings.GetSettings().TryGetValue(settingName, out var field))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"`{settingName}` is not a valid setting.")).CAF();
				return;
			}

			var desc = Context.BotSettings.ToString(Context.Client, Context.Guild);
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
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully changed my username to `{newName}`.").CAF();
		}
	}

	[Group(nameof(ModifyBotIcon)), TopLevelShortAlias(typeof(ModifyBotIcon))]
	[Summary("Changes the bot's icon to the given image. " +
		"The image must be smaller than 2.5MB.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class ModifyBotIcon : AdvobotModuleBase
	{
		private static BotIconResizer _Resizer = new BotIconResizer(4);

		[Command]
		public async Task Command(Uri url)
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on the bot icon.")).CAF();
				return;
			}

			_Resizer.EnqueueArguments(Context, new IconResizerArguments(), url, GetRequestOptions());
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in bot icon creation queue: {_Resizer.QueueCount}.").CAF();
			if (_Resizer.CanStart)
			{
				_Resizer.StartProcessing();
			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on the bot icon.")).CAF();
				return;
			}

			await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the bot icon.").CAF();
		}
	}

	[Group(nameof(DisconnectBot)), TopLevelShortAlias(typeof(DisconnectBot), "runescapeservers")]
	[Summary("Turns the bot off.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DisconnectBot : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			await ClientUtils.DisconnectBotAsync(Context.Client).CAF();
		}
	}

	[Group(nameof(RestartBot)), TopLevelShortAlias(typeof(RestartBot))]
	[Summary("Restarts the bot.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class RestartBot : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command()
		{
			await ClientUtils.RestartBotAsync(Context.Config, Context.Client).CAF();
		}
	}
}