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
		public sealed class ModifyLogChannels : MySavingModuleBase
		{
			[Group(nameof(LogChannelType.Server)), Alias("s")]
			public sealed class ModifyServerLog : MySavingModuleBase
			{
				private const LogChannelType channelType = LogChannelType.Server;

				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] ITextChannel channel)
				{
					await LogActions.SetChannel(Context, channelType, channel);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await LogActions.RemoveChannel(Context, channelType);
				}
			}

			[Group(nameof(LogChannelType.Mod)), Alias("m")]
			public sealed class ModifyModLog : MySavingModuleBase
			{
				private const LogChannelType channelType = LogChannelType.Mod;

				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] ITextChannel channel)
				{
					await LogActions.SetChannel(Context, channelType, channel);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await LogActions.RemoveChannel(Context, channelType);
				}
			}

			[Group(nameof(LogChannelType.Image)), Alias("i")]
			public sealed class ModifyImageLog : MySavingModuleBase
			{
				private const LogChannelType channelType = LogChannelType.Image;

				[Command]
				public async Task Command([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] ITextChannel channel)
				{
					await LogActions.SetChannel(Context, channelType, channel);
				}
				[Command("off")]
				public async Task CommandOff()
				{
					await LogActions.RemoveChannel(Context, channelType);
				}
			}
		}

		[Group("modifyignoredlogchannels"), Alias("milc")]
		[Usage("[Add|Remove] [Channel]/<Channel>/...")]
		[Summary("Ignores all logging info that would have been gotten from a channel.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyIgnoredLogChannels : MySavingModuleBase
		{
			[Command("add")]
			public async Task CommandAdd([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] params ITextChannel[] channels)
			{
				await CommandRunner(true, channels);
			}
			[Command("remove")]
			public async Task CommandRemove([VerifyChannel(false, ChannelVerification.CanBeRead, ChannelVerification.CanModifyPermissions)] params ITextChannel[] channels)
			{
				await CommandRunner(false, channels);
			}

			private async Task CommandRunner(bool add, ITextChannel[] channels)
			{
				var channelIds = channels.Select(x => x.Id);
				if (add)
				{
					Context.GuildSettings.IgnoredLogChannels.AddRange(channelIds);
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully ignored the following channels: `{0}`.", String.Join("`, `", channels.Select(x => x.FormatChannel()))));
				}
				else
				{
					Context.GuildSettings.IgnoredLogChannels.RemoveAll(x => channelIds.Contains(x));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully unignored the following channels: `{0}`.", String.Join("`, `", channels.Select(x => x.FormatChannel()))));
				}
			}
		}

		[Group("modifylogactions"), Alias("mla")]
		[Usage("[ShowActions|Default|Enable|Disable] <All|Log Action ...>")]
		[Summary("The server log will send messages when these events happen. `Default` overrides the current settings.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifyLogActions : MySavingModuleBase
		{
			[Group("ShowActions"), Alias("show actions", "show")]
			public sealed class ShowActions : MyModuleBase
			{
				[Command]
				public async Task Command()
				{
					await CommandRunner();
				}

				private async Task CommandRunner()
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(LogAction))));
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Log Actions", desc));
				}
			}

			[Group("default"), Alias("def")]
			public sealed class Default : MySavingModuleBase
			{
				[Command]
				public async Task Command()
				{
					await CommandRunner();
				}

				private async Task CommandRunner()
				{
					Context.GuildSettings.LogActions = Constants.DEFAULT_LOG_ACTIONS.ToList();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully set the log actions to the default ones.");
				}
			}

			[Group("enable"), Alias("e")]
			public sealed class Add : MySavingModuleBase
			{
				[Command]
				public async Task Command(params LogAction[] logActions)
				{
					await CommandRunner(false, logActions);
				}
				[Command("all")]
				public async Task CommandAll()
				{
					await CommandRunner(true);
				}

				private async Task CommandRunner(bool all, LogAction[] logActions = null)
				{
					if (all)
					{
						Context.GuildSettings.LogActions = Enum.GetValues(typeof(LogAction)).Cast<LogAction>().ToList();
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully enabled every log action.");
					}
					else
					{
						if (logActions == null)
						{
							logActions = new LogAction[0];
						}

						//Add in logActions that aren't already in there
						Context.GuildSettings.LogActions.AddRange(logActions.Except(Context.GuildSettings.LogActions));
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled the following log actions: `{0}`.", String.Join("`, `", logActions.Select(x => x.EnumName()))));
					}
				}
			}

			[Group("disable"), Alias("d")]
			public sealed class Remove : MySavingModuleBase
			{
				[Command]
				public async Task Command(params LogAction[] logActions)
				{
					await CommandRunner(false, logActions);
				}
				[Command("all")]
				public async Task CommandAll()
				{
					await CommandRunner(true);
				}

				private async Task CommandRunner(bool all, LogAction[] logActions = null)
				{
					if (all)
					{
						Context.GuildSettings.LogActions = new List<LogAction>();
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled every log action.");
					}
					else
					{
						if (logActions == null)
						{
							logActions = new LogAction[0];
						}

						Context.GuildSettings.LogActions.RemoveAll(x => logActions.Contains(x));
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully disabled the following log actions: `{0}`.", String.Join("`, `", logActions.Select(x => x.EnumName()))));
					}
				}
			}
		}
	}
	/*
	[Name("Logs")]
	public class Advobot_Commands_Logs : ModuleBase
	{


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
