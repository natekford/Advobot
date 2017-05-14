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
		[Command(BasicCommandStrings.CSETTINGS)]
		[Alias(BasicCommandStrings.ASETTINGS)]
		[Usage("<All|Setting Name>")]
		[Summary("Displays global settings. Inputting nothing gives a list of the setting names.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task CurrentGlobalSettings([Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (String.IsNullOrWhiteSpace(input))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Global Settings", String.Format("`{0}`", String.Join("`, `", Enum.GetNames(typeof(SettingOnBot))))));
				return;
			}

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 1, 1));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var settingStr = returnedArgs.Arguments[0];

			if (Actions.CaseInsEquals(settingStr, "all"))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Global Settings", Actions.FormatAllSettings(botInfo)));
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

		[Command(BasicCommandStrings.CSETTINGSRESET)]
		[Alias(BasicCommandStrings.ASETTINGSRESET)]
		[Usage("")]
		[Summary("Resets all the global settings on the bot.")]
		[BotOwnerRequirement]
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

		[Command(BasicCommandStrings.COWNER)]
		[Alias(BasicCommandStrings.AOWNER)]
		[Usage("[Clear|Current|New Owner]")]
		[Summary("You must be the current guild owner. The bot will DM you asking for its key. **DO NOT INPUT THE KEY OUTSIDE OF DMS.** If you are experiencing trouble, refresh your bot's key.")]
		[GuildOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetBotOwner([Optional, Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (Enum.TryParse(input, out CCEnum type))
			{
				switch (type)
				{
					case CCEnum.Clear:
					{
						if (botInfo.BotOwner == Context.User.Id)
						{
							botInfo.ResetBotOwner();
							Actions.SaveBotInfo();
							await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the bot owner.");
						}
						else
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, "Only the bot owner can clear their position.");
						}
						return;
					}
					case CCEnum.Current:
					{
						var user = Actions.GetBotOwner();
						if (user != null)
						{
							await Actions.SendChannelMessage(Context, String.Format("The current bot owner is: `{0}`", user.FormatUser()));
						}
						else
						{
							await Actions.SendChannelMessage(Context, "This bot is unowned.");
						}
						return;
					}
				}
			}
			else if (botInfo.BotOwner != 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("There is already a bot owner: `{0}`.", Actions.GetBotOwner().FormatUser()));
				return;
			}
			else
			{
				//Add them to the list of people trying to become bot owner
				Variables.PotentialBotOwners.Add(Context.User.Id);
				await Actions.SendDMMessage(await Context.User.CreateDMChannelAsync(), "What is my key?");
			}
		}

		[Command(BasicCommandStrings.CPATH)]
		[Alias(BasicCommandStrings.APATH)]
		[Usage("[Clear|Current|New Directory]")]
		[Summary("Changes the save path's directory. Windows defaults to User/AppData/Roaming. Other OSes will not work without a save path set. Clearing the savepath means nothing will be able to save.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetSavePath([Remainder] string input)
		{
			var settings = Properties.Settings.Default;
			if (Enum.TryParse(input, out CCEnum type))
			{
				switch (type)
				{
					case CCEnum.Clear:
					{
						settings.Path = null;
						settings.Save();
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the current save path.");
						return;
					}
					case CCEnum.Current:
					{
						if (String.IsNullOrWhiteSpace(settings.Path))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, "There is no save path set.");
						}
						else
						{
							await Actions.SendChannelMessage(Context, String.Format("The current save path is: `{0}`.", settings.Path));
						}
						return;
					}
				}
			}
			else if (!Directory.Exists(input))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That directory doesn't exist."));
			}
			else
			{
				settings.Path = input;
				settings.Save();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the save path to: `{0}`.", input));
			}
		}

		[Command(BasicCommandStrings.CPREFIX)]
		[Alias(BasicCommandStrings.APREFIX)]
		[Usage("[Clear|Current|New Prefix]")]
		[Summary("Changes the bot's prefix to the given string. Clearing the prefix sets it back to `" + Constants.BOT_PREFIX + "`.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetGlobalPrefix([Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (Enum.TryParse(input, out CCEnum type))
			{
				switch (type)
				{
					case CCEnum.Clear:
					{
						botInfo.SetPrefix(Constants.BOT_PREFIX);
						Actions.SaveBotInfo();
						await Actions.UpdateGame();
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully reset the bot's prefix to `{0}`.", Constants.BOT_PREFIX));
						return;
					}
					case CCEnum.Current:
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Then how did you use this command? :thinking:");
						return;
					}
				}
			}
			else
			{
				if (input.Length > 10)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Keep the prefix to under 10 characters."));
					return;
				}

				botInfo.SetPrefix(input);
				Actions.SaveBotInfo();
				await Actions.UpdateGame();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the bot's prefix to `{0}`.", input));
			}
		}

		[Command(BasicCommandStrings.CICON)]
		[Alias(BasicCommandStrings.AICON)]
		[Usage("[Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the bot's icon to the given image. Typing `" + Constants.BOT_PREFIX + "bi remove` will remove the icon. The image must be smaller than 2.5MB.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task BotIcon([Optional, Remainder] string input)
		{
			await Actions.SetPicture(Context, input, true);
		}

		[Command(BasicCommandStrings.CGAME)]
		[Alias(BasicCommandStrings.AGAME)]
		[Usage("[Clear|Current|New Name]")]
		[Summary("Changes the game the bot is currently listed as playing.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetGame([Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (Enum.TryParse(input, out CCEnum type))
			{
				switch (type)
				{
					case CCEnum.Clear:
					{
						botInfo.ResetGame();
						Actions.SaveBotInfo();
						await Actions.UpdateGame();
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Game set to default.");
						return;
					}
					case CCEnum.Current:
					{
						await Actions.SendChannelMessage(Context, String.Format("The current game is `{0}`.", botInfo.Game));
						return;
					}
				}
			}
			else if (input.Length > Constants.MAX_GAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Game name cannot be longer than `{0}` characters or else it doesn't show to other people.", Constants.MAX_GAME_LENGTH)));
				return;
			}
			else
			{
				botInfo.SetGame(input);
				Actions.SaveBotInfo();
				await Actions.UpdateGame();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Game set to `{0}`.", input));
			}
		}

		[Command(BasicCommandStrings.CSTREAM)]
		[Alias(BasicCommandStrings.ASTREAM)]
		[Usage("[Clear|Current|Twitch.TV Account Name]")]
		[Summary("Changes the stream the bot has listed under its name.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task BotStream([Optional, Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (Enum.TryParse(input, out CCEnum type))
			{
				switch (type)
				{
					case CCEnum.Clear:
					{
						input = null;
						botInfo.ResetStream();
						Actions.SaveBotInfo();
						await Actions.UpdateGame();
						await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully reset the bot's stream.");
						return;
					}
					case CCEnum.Current:
					{
						await Actions.SendChannelMessage(Context, String.Format("The bot's stream is `{0}`.", botInfo.Stream));
						return;
					}
				}
			}
			else
			{
				var url = Constants.STREAM_URL + input;
				botInfo.SetStream(url);
				Actions.SaveBotInfo();
				await Actions.UpdateGame();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the bot's stream to `{0}`.", url));
			}
		}

		[Command(BasicCommandStrings.CNAME)]
		[Alias(BasicCommandStrings.ANAME)]
		[Usage("[New Name]")]
		[Summary("Changes the bot's name to the given name.")]
		[BotOwnerRequirement]
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

		[Command(BasicCommandStrings.CDISC)]
		[Alias(BasicCommandStrings.ADISC_1, BasicCommandStrings.ADISC_2)]
		[Usage("")]
		[Summary("Turns the bot off.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public Task Disconnect()
		{
			Environment.Exit(0);
			return Task.CompletedTask;
		}

		[Command(BasicCommandStrings.CRESTART)]
		[Alias(BasicCommandStrings.ARESTART)]
		[Usage("")]
		[Summary("Restarts the bot.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public Task Restart()
		{
			try
			{
				//Create a new instance of the bot and close the old one
				System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
				Environment.Exit(0);
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
			}
			return Task.CompletedTask;
		}

		[Command(BasicCommandStrings.CSHARDS)]
		[Usage("[Number]")]
		[Summary("")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task ModifyShards([Remainder] string input)
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

			Variables.BotInfo.SetShardCount(number);
			Actions.SaveBotInfo();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the shard amount to `{0}`.", number));
		}

		[Command(BasicCommandStrings.CGUILDS)]
		[Alias(BasicCommandStrings.AGUILDS)]
		[Usage("")]
		[Summary("Lists the name, ID, owner, and owner's ID of every guild the bot is on.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task ListGuilds()
		{
			var guilds = Variables.Client.GetGuilds().ToList();
			if (guilds.Count < 10)
			{
				var embed = Actions.MakeNewEmbed("Guilds");
				guilds.ForEach(x =>
				{
					Actions.AddField(embed, x.FormatGuild(), String.Format("**Owner:** `{0}`", x.Owner.FormatUser()));
				});
				await Actions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				var count = 1;
				var guildStrings = guilds.Select(x => String.Format("`{0}.` `{1}` Owner: `{2}`", count++.ToString("00"), x.FormatGuild(), x.Owner.FormatUser()));
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guilds", String.Join("\n", guildStrings)));
			}
		}
	}
}