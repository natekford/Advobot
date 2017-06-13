using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Global Settings commands are commands that work on the bot globally
	[Name("Global_Settings")]
	public class Advobot_Commands_Administration : ModuleBase
	{
		[Command("displayglobalsettings")]
		[Alias("dgls")]
		[Usage("<All|Setting Name>")]
		[Summary("Displays global settings. Inputting nothing gives a list of the setting names.")]
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public async Task CurrentGlobalSettings([Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Global Settings", String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(SettingOnBot))))));
				return;
			}

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 1));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];

			if (Actions.CaseInsEquals(settingStr, "all"))
			{
				var embed = Actions.MakeNewEmbed("Current Global Settings");
				await Actions.SendEmbedMessage(Context.Channel, Actions.FormatAllSettings(embed, botInfo));
			}
			else if (Enum.TryParse(settingStr, true, out SettingOnBot setting))
			{
				var embed = Actions.FormatSettingInfo(Context, botInfo, setting);
				await Actions.SendEmbedMessage(Context.Channel, embed);
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
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public async Task GlobalSettingsReset([Remainder] string input)
		{
			Actions.ResetSettings();
			await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared all settings. Restarting now...");

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
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public async Task GlobalSettingsModify([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];
			var infoStr = returnedArgs.Arguments[1];

			var returnedType = Actions.GetType(settingStr, null, new[] { SettingOnBot.TrustedUsers });
			if (returnedType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedType);
				return;
			}
			var setting = returnedType.Type;

			var clear = Actions.CaseInsEquals(infoStr, "clear");

			var botInfo = Variables.BotInfo;
			var settings = Properties.Settings.Default;
			switch (setting)
			{
				case SettingOnBot.BotOwner:
				{
					if (clear)
					{
						botInfo.ResetBotOwner();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the bot owner back to the default value `{0}`.", botInfo.BotOwnerID));
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Clear the bot owner instead of trying to set a new one through this.");
					}
					break;
				}
				case SettingOnBot.Prefix:
				{
					if (clear)
					{
						botInfo.ResetPrefix();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the prefix back to the default value `{0}`.", botInfo.Prefix));
					}
					else
					{
						if (input.Length > 10)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Keep the prefix to under 10 characters."));
							return;
						}

						botInfo.SetPrefix(input);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the bot's prefix to `{0}`.", input));
					}
					break;
				}
				case SettingOnBot.Game:
				{
					if (clear)
					{
						botInfo.ResetGame();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the game back to the default value `{0}`.", botInfo.Game));
					}
					else
					{
						if (input.Length > Constants.MAX_GAME_LENGTH)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Game name cannot be longer than `{0}` characters or else it doesn't show to other people.", Constants.MAX_GAME_LENGTH)));
							return;
						}
						else
						{
							botInfo.SetGame(input);
							await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Game set to `{0}`.", input));
						}
					}
					break;
				}
				case SettingOnBot.Stream:
				{
					if (clear)
					{
						botInfo.ResetStream();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the stream back to the default value `{0}`.", botInfo.Stream));
					}
					else
					{
						botInfo.SetStream(input);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the bot's stream to `{0}`.", input));
					}
					break;
				}
				case SettingOnBot.ShardCount:
				{
					if (clear)
					{
						var minShardCount = (Variables.Client.GetGuilds().Count / 2500) + 1;
						botInfo.SetShardCount(minShardCount);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the shard count to `{0}`.", minShardCount));
					}
					else
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
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("With the current amount of guilds the client has, the minimum shard number is: `{0}`.", validNum)));
							return;
						}

						botInfo.SetShardCount(number);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the shard amount to `{0}`.", number));
					}
					break;
				}
				case SettingOnBot.MessageCacheSize:
				{
					if (clear)
					{
						botInfo.ResetCacheSize();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the message cache size back to the default value `{0}`.", botInfo.MessageCacheSize));
					}
					else
					{
						if (!int.TryParse(infoStr, out int cacheSize))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for cache size has to be an integer number."));
							return;
						}

						botInfo.SetCacheSize(cacheSize);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the message cache size to `{0}`.", cacheSize));
					}
					break;
				}
				case SettingOnBot.AlwaysDownloadUsers:
				{
					if (clear)
					{
						botInfo.ResetAlwaysDownloadUsers();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the always download users bool back to the default value `{0}`.", botInfo.AlwaysDownloadUsers));
					}
					else
					{
						if (!bool.TryParse(infoStr, out bool alwaysDLUsers))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for always download users has to be a boolean."));
							return;
						}

						botInfo.SetAlwaysDownloadUsers(alwaysDLUsers);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set always download users to `{0}`.", alwaysDLUsers));
					}
					break;
				}
				case SettingOnBot.LogLevel:
				{
					if (clear)
					{
						botInfo.ResetLogLevel();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the log level back to the default value `{0}`.", Enum.GetName(typeof(LogSeverity), botInfo.LogLevel)));
					}
					else
					{
						if (!Enum.TryParse(infoStr, true, out LogSeverity logLevel))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The input for log level has to be one of the following: `{0}`.",
								String.Join("`, `", Enum.GetNames(typeof(LogSeverity))))));
							return;
						}

						botInfo.SetLogLevel(logLevel);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the log level to `{0}`.", Enum.GetName(typeof(LogSeverity), logLevel)));
					}
					break;
				}
				case SettingOnBot.SavePath:
				{
					if (clear)
					{
						settings.Path = null;
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the save path back to the default value `{0}`.", "NOTHING"));
					}
					else
					{
						if (!Directory.Exists(infoStr))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That directory doesn't exist."));
						}
						else
						{
							settings.Path = infoStr;
							await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the save path to: `{0}`.", infoStr));
						}
					}
					break;
				}
				case SettingOnBot.MaxUserGatherCount:
				{
					if (clear)
					{
						botInfo.ResetMaxUserGatherCount();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the max user gather count back to the default value `{0}`.", botInfo.MaxUserGatherCount));
					}
					else
					{
						if (!int.TryParse(infoStr, out int maxUserGatherCount))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for max user gather count has to be an integer number."));
							return;
						}

						botInfo.SetMaxUserGatherCount(maxUserGatherCount);
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the max user gather count to `{0}`.", maxUserGatherCount));
					}
					break;
				}
			}

			settings.Save();
			Actions.SaveBotInfo(Variables.BotInfo);
			await Actions.UpdateGame();
		}

		[Command("changeboticon")]
		[Alias("cbi")]
		[Usage("[Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the bot's icon to the given image. Typing `" + Constants.BOT_PREFIX + "bi remove` will remove the icon. The image must be smaller than 2.5MB.")]
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public async Task BotIcon([Optional, Remainder] string input)
		{
			await Actions.SetPicture(Context, input, true);
		}

		[Command("changebotname")]
		[Alias("cbn")]
		[Usage("[New Name]")]
		[Summary("Changes the bot's name to the given name.")]
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
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
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
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
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public Task Restart()
		{
			Actions.RestartBot();
			return Task.CompletedTask;
		}
	}
}