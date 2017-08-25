using Advobot.Actions;
using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using System.Linq;
using Discord.WebSocket;

namespace Advobot
{
	namespace BotSettings
	{
		[Group(nameof(ModifyBotSettings)), Alias("mgls")]
		[Summary("Modify the given setting on the bot. Inputting help as the second argument gives information about what arguments that setting takes.")]
		[Usage("[Clear|Set] [Setting Name] <New Value>")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class ModifyBotSettings : MySavingModuleBase
		{
			[Command("set"), Alias("s")]
			public async Task CommandSet(string commandName)
			{

			}
			[Command(nameof(ActionType.Clear)), Alias("c")]
			public async Task CommandClear(string commandName)
			{

			}
		}

		[Group(nameof(DisplayBotSettings)), Alias("dgls")]
		[Usage("<All|Setting Name>")]
		[Summary("Displays global settings. Inputting nothing gives a list of the setting names.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class DisplayBotSettings : MyModuleBase
		{
			[Command("all"), Priority(1)]
			public async Task CommandAll()
			{
				var text = await FormattingActions.FormatAllBotSettings(Context.Client, Context.BotSettings);
				await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "Bot Settings", "Bot Settings");
			}
			[Command, Priority(0)]
			public async Task Command(string setting)
			{
				var currentSetting = GetActions.GetBotSettings().FirstOrDefault(x => x.Name.CaseInsEquals(setting));
				if (currentSetting == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Unable to find a setting with the supplied name."));
					return;
				}

				var desc = await FormattingActions.FormatBotSettingInfo(Context.Client, Context.BotSettings, currentSetting);
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
				var settingNames = GetActions.GetBotSettings().Select(x => x.Name);

				var desc = String.Format("`{0}`", String.Join("`, `", settingNames));
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Setting Names", desc));
			}
		}

		[Group(nameof(Disconnect)), Alias("dc", "runescapeservers")]
		[Usage("")]
		[Summary("Turns the bot off.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class Disconnect : MyModuleBase
		{
			[Command]
			public Task Command()
			{
				ClientActions.DisconnectBot();
				return Task.FromResult(0);
			}
		}

		[Group(nameof(Restart)), Alias("res")]
		[Usage("")]
		[Summary("Restarts the bot.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class Restart : MyModuleBase
		{
			[Command]
			public Task Command()
			{
				ClientActions.RestartBot();
				return Task.FromResult(0);
			}
		}
	}
	/*
	//Global Settings commands are commands that work on the bot globally
	[Name("GlobalSettings")]
	public class Advobot_Commands_Administration : ModuleBase
	{

		[Command("resetglobalsettings")]
		[Alias("rgls")]
		[Usage("")]
		[Summary("Resets all the global settings on the bot.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task GlobalSettingsReset([Optional, Remainder] string input)
		{
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared all settings. Restarting now...");
			Actions.ResetSettings();

			try
			{
				//Restart the application and close the current session
				System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
				Environment.Exit(0);
			}
			catch (Exception)
			{
				Messages.WriteLine("Bot is unable to restart.");
			}
		}


		public async Task GlobalSettingsModify([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];
			var infoStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetEnum(settingStr, null, new[] { SettingOnBot.TrustedUsers });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var setting = returnedType.Object;

			var botInfo = Variables.BotInfo;
			var settings = Properties.Settings.Default;
			if (Actions.CaseInsEquals(infoStr, "clear"))
			{
				switch (setting)
				{
					case SettingOnBot.BotOwnerID:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the bot owner back to the default value `{0}`.",
							((ulong)botInfo.GetSetting(SettingOnBot.BotOwnerID))));
						break;
					}
					case SettingOnBot.Prefix:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the prefix back to the default value `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Prefix))));
						break;
					}
					case SettingOnBot.Game:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the game back to the default value `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Game))));
						break;
					}
					case SettingOnBot.Stream:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the stream back to the default value `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Stream))));
						break;
					}
					case SettingOnBot.ShardCount:
					{
						botInfo.SetSetting(setting, (Variables.Client.GetGuilds().Count / 2500) + 1);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the shard count to `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.ShardCount))));
						break;
					}
					case SettingOnBot.MessageCacheCount:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the message cache size back to the default value `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount))));
						break;
					}
					case SettingOnBot.AlwaysDownloadUsers:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the always download users bool back to the default value `{0}`.",
							((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers))));
						break;
					}
					case SettingOnBot.LogLevel:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the log level back to the default value `{0}`.",
							((LogSeverity)botInfo.GetSetting(SettingOnBot.LogLevel))));
						break;
					}
					case SettingOnBot.SavePath:
					{
						settings.Path = null;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the save path back to the default value `{0}`.",
							"NOTHING"));
						break;
					}
					case SettingOnBot.MaxUserGatherCount:
					{
						botInfo.ResetSetting(setting);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the max user gather count back to the default value `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.MaxUserGatherCount))));
						break;
					}
				}
			}
			else
			{
				switch (setting)
				{
					case SettingOnBot.BotOwnerID:
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Clear the bot owner instead of trying to set a new one through this.");
						break;
					}
					case SettingOnBot.Prefix:
					{
						if (input.Length > 10)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Keep the prefix to under 10 characters."));
							return;
						}

						botInfo.SetSetting(setting, input);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the bot's prefix to `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Prefix))));
						break;
					}
					case SettingOnBot.Game:
					{
						if (input.Length > Constants.MAX_GAME_LENGTH)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Game name cannot be longer than `{0}` characters or else it doesn't show to other people.",
								Constants.MAX_GAME_LENGTH)));
							return;
						}

						botInfo.SetSetting(setting, input);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Game set to `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Game))));
						break;
					}
					case SettingOnBot.Stream:
					{
						botInfo.SetSetting(setting, input);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the bot's stream to `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Stream))));
						break;
					}
					case SettingOnBot.ShardCount:
					{
						if (!int.TryParse(input, out int number))
						{
							Messages.WriteLine("Invalid input for a number.");
							return;
						}

						var curGuilds = Variables.Client.GetGuilds().Count;
						if (curGuilds >= number * 2500)
						{
							var validNum = curGuilds / 2500 + 1;
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("With the current amount of guilds the client has, the minimum shard number is: `{0}`.",
								validNum)));
							return;
						}

						botInfo.SetSetting(setting, number);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the shard amount to `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.ShardCount))));
						break;
					}
					case SettingOnBot.MessageCacheCount:
					{
						if (!int.TryParse(infoStr, out int cacheSize))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The input for cache size has to be an integer number."));
							return;
						}

						botInfo.SetSetting(setting, cacheSize);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the message cache size to `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount))));
						break;
					}
					case SettingOnBot.AlwaysDownloadUsers:
					{
						if (!bool.TryParse(infoStr, out bool alwaysDLUsers))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The input for always download users has to be a boolean."));
							return;
						}

						botInfo.SetSetting(setting, alwaysDLUsers);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set always download users to `{0}`.",
							((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers))));
						break;
					}
					case SettingOnBot.LogLevel:
					{
						if (!Enum.TryParse(infoStr, true, out LogSeverity logLevel))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("The input for log level has to be one of the following: `{0}`.",
								String.Join("`, `", Enum.GetNames(typeof(LogSeverity))))));
							return;
						}

						botInfo.SetSetting(setting, logLevel);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the log level to `{0}`.",
							((LogSeverity)botInfo.GetSetting(SettingOnBot.LogLevel))));
						break;
					}
					case SettingOnBot.SavePath:
					{
						if (!Directory.Exists(infoStr))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("That directory doesn't exist."));
						}

						settings.Path = infoStr;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the save path to: `{0}`.",
							infoStr));
						break;
					}
					case SettingOnBot.MaxUserGatherCount:
					{
						if (!int.TryParse(infoStr, out int maxUserGatherCount))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The input for max user gather count has to be an integer number."));
							return;
						}

						botInfo.SetSetting(setting, maxUserGatherCount);
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the max user gather count to `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.MaxUserGatherCount))));
						break;
					}
				}
			}

			settings.Save();
			Variables.BotInfo.SaveInfo();
			await Actions.UpdateGame();
		}

		[Command("stopusingbot")]
		[Alias("sub")]
		[Usage("")]
		[Summary("Remove's the currently used bot's key so that a different bot can be used instead.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task StopUsingBot([Optional, Remainder] string input)
		{
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the bot key. Restarting now...");
			var settings = Properties.Settings.Default;
			settings.BotKey = null;
			settings.Save();

			try
			{
				//Restart the application and close the current session
				System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
				Environment.Exit(0);
			}
			catch (Exception)
			{
				Messages.WriteLine("Bot is unable to restart.");
			}
		}

		[Command("changeboticon")]
		[Alias("cbi")]
		[Usage("[Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the bot's icon to the given image. Typing `" + Constants.BOT_PREFIX + "bi remove` will remove the icon. The image must be smaller than 2.5MB.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task BotIcon([Optional, Remainder] string input)
		{
			//await Actions.SetPicture(Context, input, true);
		}

		[Command("changebotname")]
		[Alias("cbn")]
		[Usage("[New Name]")]
		[Summary("Changes the bot's name to the given name.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task BotName([Remainder] string input)
		{
			//Names have the same length requirements as nicknames
			if (input.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Name cannot be more than `{0}` characters.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}
			else if (input.Length < Constants.MIN_NICKNAME_LENGTH)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Name cannot be less than `{0}` characters.", Constants.MIN_NICKNAME_LENGTH)));
				return;
			}

			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = input);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed my username to `{0}`.", input));
		}


	}
	*/
}