using Advobot.Actions;
using Advobot.NonSavedClasses;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Enums;

namespace Advobot
{
	namespace Logs
	{
		[Group("modifylogchannels"), Alias("mlc")]
		[Usage("[Server|Mod|Image] [Channel|Off]")]
		[Summary("Puts the serverlog on the specified channel. Serverlog is a log of users joining/leaving, editing messages, and deleting messages.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyLogChannels : MyModuleBase
		{
			[Group(nameof(LogChannelType.Server)), Alias("s")]
			public sealed class ModifyServerLog : MySavingModuleBase
			{
				[Command]
				public async Task Command(ITextChannel channel)
				{
					ModifyLogChannelsHelperFunctions.SetChannel(Context, LogChannelType.Server, channel);
				}
				[Command("off")]
				public async Task CommandOff()
				{

				}
			}

			[Group(nameof(LogChannelType.Mod)), Alias("m")]
			public sealed class ModifyModLog : MySavingModuleBase
			{

			}

			[Group(nameof(LogChannelType.Image)), Alias("i")]
			public sealed class ModifyImageLog : MySavingModuleBase
			{

			}

			private static class ModifyLogChannelsHelperFunctions
			{
				public static void SetChannel(MyCommandContext context, LogChannelType channelType, ITextChannel channel)
				{
					//context.GuildSettings
				}
				public static void RemoveChannel(MyCommandContext context, LogChannelType channelType)
				{

				}
			}
		}
	}
	/*
	[Name("Logs")]
	public class Advobot_Commands_Logs : ModuleBase
	{

		public async Task Serverlog([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var typeStr = returnedArgs.Arguments[0].ToLower();
			var chanStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetEnum(typeStr, new[] { LogChannelType.Server, LogChannelType.Mod, LogChannelType.Image });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var type = returnedType.Object;

			ITextChannel channel = null;
			if (!Actions.CaseInsEquals(chanStr, "off"))
			{
				var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions, ObjectVerification.IsText }, true, chanStr);
				if (returnedChannel.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}
				channel = returnedChannel.Object as ITextChannel;

				ulong currID = 0;
				switch (type)
				{
					case LogChannelType.Server:
					{
						currID = ((DiscordObjectWithId<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ServerLog))?.ID ?? 0;
						break;
					}
					case LogChannelType.Mod:
					{
						currID = ((DiscordObjectWithId<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ModLog))?.ID ?? 0;
						break;
					}
					case LogChannelType.Image:
					{
						currID = ((DiscordObjectWithId<ITextChannel>)guildInfo.GetSetting(SettingOnGuild.ImageLog))?.ID ?? 0;
						break;
					}
				}

				if (currID == channel.Id)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("The given channel is already the current {0} log.", typeStr)));
					return;
				}
			}

			var success = false;
			switch (type)
			{
				case LogChannelType.Server:
				{
					success = guildSettings.SetSetting(SettingOnGuild.ServerLog, new DiscordObjectWithId<ITextChannel>(channel));
					break;
				}
				case LogChannelType.Mod:
				{
					success = guildSettings.SetSetting(SettingOnGuild.ModLog, new DiscordObjectWithId<ITextChannel>(channel));
					break;
				}
				case LogChannelType.Image:
				{
					success = guildSettings.SetSetting(SettingOnGuild.ImageLog, new DiscordObjectWithId<ITextChannel>(channel));
					break;
				}
			}

			if (success)
			{
				if (channel != null)
				{
					await MessageActions.SendChannelMessage(Context, String.Format("The {0} log has been set on `{1}`.", typeStr, channel.FormatChannel()));
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully disabled the {0} log.", typeStr));
				}
			}
			else
			{
				await MessageActions.SendChannelMessage(Context, Formatting.ERROR(String.Format("Failed to set the {0} log.", typeStr)));
			}
		}

		[Command("modifyignoredlogchannels")]
		[Alias("milc")]
		[Usage("[Add|Remove] [Channel]/<Channel>/...")]
		[Summary("Ignores all logging info that would have been gotten from a channel. Only works on text channels that you and the bot have the ability to see.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task IgnoreChannel([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split the input and determine whether to add or remove
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			//Get the channels
			var evaluatedChannels = Actions.GetValidEditChannels(Context);
			if (!evaluatedChannels.HasValue)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			var success = evaluatedChannels.Value.Success;
			var failure = evaluatedChannels.Value.Failure;

			//Make sure stuff isn't already ignored
			var ignoredLogChannels = ((List<ulong>)guildInfo.GetSetting(SettingOnGuild.IgnoredLogChannels));
			var alreadyAction = new List<IGuildChannel>();
			var add = false;
			switch (action)
			{
				case ActionType.Add:
				{
					success.ForEach(x =>
					{
						if (ignoredLogChannels.Contains(x.Id))
						{
							alreadyAction.Add(x);
						}
						else
						{
							ignoredLogChannels.Add(x.Id);
						}
					});
					add = true;
					break;
				}
				case ActionType.Remove:
				{
					success.ForEach(x =>
					{
						if (!ignoredLogChannels.Contains(x.Id))
						{
							alreadyAction.Add(x);
						}
						else
						{
							ignoredLogChannels.Remove(x.Id);
						}
					});
					break;
				}
			}

			//Format the response message
			var pastTense = add ? "ignored" : "unignored";
			var presentTense = add ? "ignore" : "unignore";
			var output = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "channel", pastTense, presentTense);
			var alreadyActionOutput = "";
			if (alreadyAction.Any())
			{
				alreadyActionOutput += String.Format("The following channel{0} were already {1}: `{2}`. ",
					Actions.GetPlural(alreadyAction.Count),
					pastTense,
					String.Join("`, `", alreadyAction.Select(x => x.FormatChannel())));
			}

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, output + alreadyActionOutput);
		}

		[Command("modifylogactions")]
		[Alias("mla")]
		[Usage("<Add|Remove|Default> <All|Log Action/...>")]
		[Summary("The server log will send messages when these events happen. `Default` overrides the current settings. Inputting nothing gives a list of the log actions.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public async Task SwitchLogActions([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);
			var logActions = (List<LogAction>)guildInfo.GetSetting(SettingOnGuild.LogActions);

			//Check if the person wants to only see the types
			if (String.IsNullOrWhiteSpace(input))
			{
				await MessageActions.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed("Log Actions", String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(LogAction))))));
				return;
			}
			else if (Actions.CaseInsEquals(input, "default"))
			{
				if (guildInfo.SetSetting(SettingOnGuild.LogActions, Constants.DEFAULT_LOG_ACTIONS.ToList()))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully restored the default log actions.");
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Failed to save the default log actions."));
				}
				return;
			}

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var logActStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			//Get all the targetted log actions
			var newLogActions = new List<LogAction>();
			if (Actions.CaseInsEquals(logActStr, "all"))
			{
				newLogActions = Enum.GetValues(typeof(LogAction)).Cast<LogAction>().ToList();
			}
			else
			{
				logActStr.Split('/').ToList().ForEach(x =>
				{
					if (Enum.TryParse(x, true, out LogAction temp))
					{
						newLogActions.Add(temp);
					}
				});
			}
			if (!newLogActions.Any())
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No valid log actions were able to be gotten."));
				return;
			}

			var responseStr = "";
			switch (action)
			{
				case ActionType.Add:
				{
					logActions.AddRange(newLogActions);
					responseStr = "enabled";
					break;
				}
				case ActionType.Remove:
				{
					logActions = logActions.Except(newLogActions).ToList();
					responseStr = "disabled";
					break;
				}
			}

			if (guildInfo.SetSetting(SettingOnGuild.LogActions, logActions.Distinct()))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the following log action{1}: `{2}`.",
					responseStr,
					Actions.GetPlural(newLogActions.Count),
					String.Join("`, `", newLogActions.Select(x => x.EnumName()))));
			}
			else
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Failed to save the log actions."));
				return;
			}
		}
	}
	*/
}
