using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Global Settings commands are commands that work on the bot globally
	[Name("GlobalSettings")]
	public class Advobot_Commands_Administration : ModuleBase
	{
		[Command("displayglobalsettings")]
		[Alias("dgls")]
		[Usage("<All|Setting Name>")]
		[Summary("Displays global settings. Inputting nothing gives a list of the setting names.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task CurrentGlobalSettings([Optional, Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Global Settings", String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(SettingOnBot))))));
				return;
			}

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 1));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];

			if (Actions.CaseInsEquals(settingStr, "all"))
			{
				await Actions.WriteAndUploadTextFile(Context.Guild, Context.Channel, Actions.FormatAllSettings(botInfo), "Current_Global_Settings");
			}
			else if (Enum.TryParse(settingStr, true, out SettingOnBot setting))
			{
				var title = setting.EnumName();
				var desc = Actions.FormatSettingInfo(botInfo, setting);
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, desc));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid setting."));
			}
		}

		[Command("resetglobalsettings")]
		[Alias("rgls")]
		[Usage("")]
		[Summary("Resets all the global settings on the bot.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public async Task GlobalSettingsReset([Optional, Remainder] string input)
		{
			await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared all settings. Restarting now...");
			Actions.ResetSettings();

			try
			{
				//Restart the application and close the current session
				System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
				Environment.Exit(0);
			}
			catch (Exception)
			{
				Actions.WriteLine("Bot is unable to restart.");
			}
		}

		[Command("modifyglobalsettings")]
		[Alias("mgls")]
		[Summary("Modify the given setting on the bot. Inputting help as the second argument gives information about what arguments that setting takes.")]
		[Usage("[Setting Name] [Help|Clear|New Value]")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
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
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the bot owner back to the default value `{0}`.",
							((ulong)botInfo.GetSetting(SettingOnBot.BotOwnerID))));
						break;
					}
					case SettingOnBot.Prefix:
					{
						botInfo.ResetSetting(setting);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the prefix back to the default value `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Prefix))));
						break;
					}
					case SettingOnBot.Game:
					{
						botInfo.ResetSetting(setting);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the game back to the default value `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Game))));
						break;
					}
					case SettingOnBot.Stream:
					{
						botInfo.ResetSetting(setting);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the stream back to the default value `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Stream))));
						break;
					}
					case SettingOnBot.ShardCount:
					{
						botInfo.SetSetting(setting, (Variables.Client.GetGuilds().Count / 2500) + 1);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the shard count to `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.ShardCount))));
						break;
					}
					case SettingOnBot.MessageCacheCount:
					{
						botInfo.ResetSetting(setting);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the message cache size back to the default value `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount))));
						break;
					}
					case SettingOnBot.AlwaysDownloadUsers:
					{
						botInfo.ResetSetting(setting);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the always download users bool back to the default value `{0}`.",
							((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers))));
						break;
					}
					case SettingOnBot.LogLevel:
					{
						botInfo.ResetSetting(setting);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the log level back to the default value `{0}`.",
							((LogSeverity)botInfo.GetSetting(SettingOnBot.LogLevel))));
						break;
					}
					case SettingOnBot.SavePath:
					{
						settings.Path = null;
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the save path back to the default value `{0}`.",
							"NOTHING"));
						break;
					}
					case SettingOnBot.MaxUserGatherCount:
					{
						botInfo.ResetSetting(setting);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the max user gather count back to the default value `{0}`.",
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
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Clear the bot owner instead of trying to set a new one through this.");
						break;
					}
					case SettingOnBot.Prefix:
					{
						if (input.Length > 10)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Keep the prefix to under 10 characters."));
							return;
						}

						botInfo.SetSetting(setting, input);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the bot's prefix to `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Prefix))));
						break;
					}
					case SettingOnBot.Game:
					{
						if (input.Length > Constants.MAX_GAME_LENGTH)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Game name cannot be longer than `{0}` characters or else it doesn't show to other people.",
								Constants.MAX_GAME_LENGTH)));
							return;
						}

						botInfo.SetSetting(setting, input);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Game set to `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Game))));
						break;
					}
					case SettingOnBot.Stream:
					{
						botInfo.SetSetting(setting, input);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the bot's stream to `{0}`.",
							((string)botInfo.GetSetting(SettingOnBot.Stream))));
						break;
					}
					case SettingOnBot.ShardCount:
					{
						if (!int.TryParse(input, out int number))
						{
							Actions.WriteLine("Invalid input for a number.");
							return;
						}

						var curGuilds = Variables.Client.GetGuilds().Count;
						if (curGuilds >= number * 2500)
						{
							var validNum = curGuilds / 2500 + 1;
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("With the current amount of guilds the client has, the minimum shard number is: `{0}`.",
								validNum)));
							return;
						}

						botInfo.SetSetting(setting, number);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the shard amount to `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.ShardCount))));
						break;
					}
					case SettingOnBot.MessageCacheCount:
					{
						if (!int.TryParse(infoStr, out int cacheSize))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for cache size has to be an integer number."));
							return;
						}

						botInfo.SetSetting(setting, cacheSize);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the message cache size to `{0}`.",
							((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount))));
						break;
					}
					case SettingOnBot.AlwaysDownloadUsers:
					{
						if (!bool.TryParse(infoStr, out bool alwaysDLUsers))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for always download users has to be a boolean."));
							return;
						}

						botInfo.SetSetting(setting, alwaysDLUsers);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set always download users to `{0}`.",
							((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers))));
						break;
					}
					case SettingOnBot.LogLevel:
					{
						if (!Enum.TryParse(infoStr, true, out LogSeverity logLevel))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The input for log level has to be one of the following: `{0}`.",
								String.Join("`, `", Enum.GetNames(typeof(LogSeverity))))));
							return;
						}

						botInfo.SetSetting(setting, logLevel);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the log level to `{0}`.",
							((LogSeverity)botInfo.GetSetting(SettingOnBot.LogLevel))));
						break;
					}
					case SettingOnBot.SavePath:
					{
						if (!Directory.Exists(infoStr))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That directory doesn't exist."));
						}

						settings.Path = infoStr;
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the save path to: `{0}`.",
							infoStr));
						break;
					}
					case SettingOnBot.MaxUserGatherCount:
					{
						if (!int.TryParse(infoStr, out int maxUserGatherCount))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for max user gather count has to be an integer number."));
							return;
						}

						botInfo.SetSetting(setting, maxUserGatherCount);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the max user gather count to `{0}`.",
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
			await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the bot key. Restarting now...");
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
				Actions.WriteLine("Bot is unable to restart.");
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
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be more than `{0}` characters.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}
			else if (input.Length < Constants.MIN_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be less than `{0}` characters.", Constants.MIN_NICKNAME_LENGTH)));
				return;
			}

			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = input);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed my username to `{0}`.", input));
		}

		[Command("disconnect")]
		[Alias("dc", "runescapeservers")]
		[Usage("")]
		[Summary("Turns the bot off.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public Task Disconnect()
		{
			Actions.DisconnectBot();
			return Task.CompletedTask;
		}

		[Command("restart")]
		[Alias("res")]
		[Usage("")]
		[Summary("Restarts the bot.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public Task Restart()
		{
			Actions.RestartBot();
			return Task.CompletedTask;
		}
	}
}