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
		#region Settings
		[Command(BasicCommandStrings.COWNER)]
		[Alias(BasicCommandStrings.AOWNER)]
		[Usage("<Clear|Current>")]
		[Summary("You must be the current guild owner. The bot will DM you asking for its key. **DO NOT INPUT THE KEY OUTSIDE OF DMS.** If you are experiencing trouble, refresh your bot's key.")]
		[GuildOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetBotOwner([Optional, Remainder] string input)
		{
			//Check if it's current
			if (Actions.CaseInsEquals(input, "current"))
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

			//Everything past here requires the user to be the current guild owner
			if (Context.Guild.OwnerId != Context.User.Id)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You must be the guild owner to set yourself as the bot owner."));
				return;
			}

			//Check if it's clear
			var botInfo = Variables.BotInfo;
			if (Actions.CaseInsEquals(input, "clear"))
			{
				//Only let the current bot owner to clear
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

			//Check if there's already a bot owner
			if (botInfo.BotOwner != 0)
			{
				//Get the bot owner
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("There is already a bot owner: `{0}`.", Actions.GetBotOwner().FormatUser()));
				return;
			}

			//Add them to the list of people trying to become bot owner
			Variables.PotentialBotOwners.Add(Context.User.Id);
			await Actions.SendDMMessage(await Context.User.CreateDMChannelAsync(), "What is my key?");
		}

		[Command(BasicCommandStrings.CPATH)]
		[Alias(BasicCommandStrings.APATH)]
		[Usage("[Clear|Current|New Directory]")]
		[Summary("Changes the save path's directory. Windows defaults to User/AppData/Roaming. Other OSes will not work without a save path set. Clearing the savepath means nothing will be able to save.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetSavePath([Remainder] string input)
		{
			//Check if it's current
			if (Actions.CaseInsEquals(input, "current"))
			{
				//Check if the path is empty
				if (String.IsNullOrWhiteSpace(Properties.Settings.Default.Path))
				{
					//If windows then default to appdata
					if (Variables.Windows)
					{
						await Actions.SendChannelMessage(Context, String.Format("The current save path is: `{0}`.",
							Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.SERVER_FOLDER)));
					}
					//If not windows then there's no folder
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, "There is no save path set.");
					}
				}
				else
				{
					await Actions.SendChannelMessage(Context, "The current save path is: `" + Properties.Settings.Default.Path + "`.");
				}
				return;
			}

			//See if clear
			if (Actions.CaseInsEquals(input, "clear"))
			{
				Properties.Settings.Default.Path = null;
				Properties.Settings.Default.Save();
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully cleared the current save path.", 5000);
				return;
			}

			//See if the directory exists
			if (!Directory.Exists(input))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That directory doesn't exist."));
				return;
			}

			//Set the path
			Properties.Settings.Default.Path = input;
			Properties.Settings.Default.Save();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the save path to: `{0}`.", input), 10000);
		}

		[Command(BasicCommandStrings.CPREFIX)]
		[Alias(BasicCommandStrings.APREFIX)]
		[Usage("[Clear|Current|New Prefix]")]
		[Summary("Changes the bot's prefix to the given string. Clearing the prefix sets it back to `" + Constants.BOT_PREFIX + "`.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetGlobalPrefix([Remainder] string input)
		{
			if (Actions.CaseInsEquals(input, "clear"))
			{
				Variables.BotInfo.SetPrefix(Constants.BOT_PREFIX);
				await Actions.SendChannelMessage(Context, String.Format("Successfully reset the bot's prefix to `{0}`.", Constants.BOT_PREFIX));
			}
			else if (Actions.CaseInsEquals(input, "current"))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Then how did you use this command? :thinking:");
			}
			else
			{
				if (input.Length > 10)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Keep the prefix to under 10 characters."));
					return;
				}

				Variables.BotInfo.SetPrefix(input);
				await Actions.SendChannelMessage(Context, String.Format("Successfully changed the bot's prefix to `{0}`.", input));
			}

			Actions.SaveBotInfo();
			await Actions.UpdateGame();
		}

		[Command(BasicCommandStrings.CSETTINGS)]
		[Alias(BasicCommandStrings.ASETTINGS)]
		[Usage("[Clear|Current]")]
		[Summary("Shows all the settings on the bot aside from the bot's key. When clearing all settings the bot will have to be manually restarted if it reconnects.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task CurrentGlobalSettings([Remainder] string input)
		{
			//Check if current
			var botInfo = Variables.BotInfo;
			if (Actions.CaseInsEquals(input, "current"))
			{
				var description = "";
				description += String.Format("**Prefix:** `{0}`\n", String.IsNullOrWhiteSpace(botInfo.Prefix) ? "N/A" : botInfo.Prefix);
				description += String.Format("**Shards:** `{0}`\n", botInfo.ShardCount);
				description += String.Format("**Save Path:** `{0}`\n", String.IsNullOrWhiteSpace(Properties.Settings.Default.Path) ? "N/A" : Properties.Settings.Default.Path);
				description += String.Format("**Bot Owner ID:** `{0}`\n", String.IsNullOrWhiteSpace(botInfo.BotOwner.ToString()) ? "N/A" : botInfo.BotOwner.ToString());
				description += String.Format("**Stream:** `{0}`\n", String.IsNullOrWhiteSpace(botInfo.Stream) ? "N/A" : botInfo.Stream);
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Global Bot Settings", description));
			}
			//Check if clear
			else if (Actions.CaseInsEquals(input, "clear"))
			{
				//Send a success message first instead of after due to the bot losing its ability to do so
				await Actions.SendChannelMessage(Context, "Successfully cleared all settings. Restarting now...");
				//Reset the settings
				Actions.ResetSettings();
				//Restart the bot
				try
				{
					//Restart the application
					System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
					//Close the previous version
					Environment.Exit(0);
				}
				catch (Exception)
				{
					Actions.WriteLine("Bot is unable to restart.");
				}
			}
			//Else give action error
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
		}
		#endregion

		#region Bot Changes
		[Command(BasicCommandStrings.CICON)]
		[Alias(BasicCommandStrings.AICON)]
		[Usage("[Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the bot's icon.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task BotIcon([Optional, Remainder] string input)
		{
			await Actions.SetPicture(Context, input, true);
		}

		[Command(BasicCommandStrings.CGAME)]
		[Alias(BasicCommandStrings.AGAME)]
		[Usage("[Clear|New Name]")]
		[Summary("Changes the game the bot is currently listed as playing.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task SetGame([Remainder] string input)
		{
			if (input.Length > Constants.MAX_GAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context,Actions.ERROR(String.Format("Game name cannot be longer than `{0}` characters or else it doesn't show to other people.", Constants.MAX_GAME_LENGTH)));
				return;
			}

			var botInfo = Variables.BotInfo;
			if (Actions.CaseInsEquals(input, "clear"))
			{
				botInfo.ResetGame();
				await Actions.SendChannelMessage(Context, "Game set to default.");
			}
			else
			{
				botInfo.SetGame(input);
				await Actions.SendChannelMessage(Context, String.Format("Game set to `{0}`.", input));
			}

			Actions.SaveBotInfo();
			await Actions.UpdateGame();
		}

		[Command(BasicCommandStrings.CSTREAM)]
		[Alias(BasicCommandStrings.ASTREAM)]
		[Usage("[Clear|Twitch.TV Account Name]")]
		[Summary("Changes the stream the bot has listed under its name.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task BotStream([Optional, Remainder] string input)
		{
			var botInfo = Variables.BotInfo;
			if (Actions.CaseInsEquals(input, "clear"))
			{
				botInfo.ResetStream();
			}
			else
			{
				botInfo.SetStream(Constants.STREAM_URL + input);
			}
			Actions.SaveBotInfo();

			await Actions.UpdateGame();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} the bot's stream{1}.", input == null ? "reset" : "set", input == null ? "" : " to `" + input + "`"));
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

			//Change the bots name to it
			await Context.Client.CurrentUser.ModifyAsync(x => x.Username = input);

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed my username to `{0}`.", input));
		}
		#endregion

		#region Misc
		[Command(BasicCommandStrings.CDISC)]
		[Alias(BasicCommandStrings.ADISC_1, BasicCommandStrings.ADISC_2)]
		[Usage("")]
		[Summary("Turns the bot off.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task Disconnect()
		{
			if (Context.User.Id == Variables.BotInfo.BotOwner || Constants.DISCONNECT)
			{
				Environment.Exit(0);
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
		}

		[Command(BasicCommandStrings.CRESTART)]
		[Alias(BasicCommandStrings.ARESTART)]
		[Usage("")]
		[Summary("Restarts the bot.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task Restart()
		{
			if (Context.User.Id == Variables.BotInfo.BotOwner || Constants.DISCONNECT)
			{
				try
				{
					//Create a new instance of the bot
					System.Windows.Forms.Application.Restart();
					//Close the old one
					Environment.Exit(0);
				}
				catch (Exception)
				{
					Actions.WriteLine("Bot is unable to restart.");
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Disconnection is turned off for everyone but the bot owner currently.");
			}
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
				Actions.WriteLine("Invalid input for number.");
				return;
			}

			var curGuilds = Variables.Client.GetGuilds().Count;
			if (curGuilds >= number * 2500)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("With the current amount of guilds the client has, the minimum shard number is: " + curGuilds / 2500 + 1));
				return;
			}

			Variables.BotInfo.SetShardCount(number);
			Actions.SaveBotInfo();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the shard amount to {0}.", number));
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
		#endregion
	}
}