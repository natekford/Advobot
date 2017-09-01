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
using Advobot.Interfaces;
using Advobot.TypeReaders;
using System.Reflection;

namespace Advobot
{
	namespace BotSettings
	{
		[Group(nameof(ModifyBotSettings)), Alias("mbs")]
		[Usage("[Show|Clear|Set] [Setting Name] <New Value>")]
		[Summary("Modify the given setting on the bot. Show lists the setting names. Clear resets a setting back to default. Cannot modify settings which are lists through this command.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class ModifyBotSettings : MySavingModuleBase
		{
			[Command(nameof(ActionType.Show)), Alias("sh")]
			public async Task CommandShow()
			{
				var desc = $"`{String.Join("`, `", GetActions.GetBotSettingsThatArentIEnumerables().Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Bot Settings", desc));
			}
			[Command("set"), Alias("s")]
			public async Task CommandSet([OverrideTypeReader(typeof(BotSettingNonIEnumerableTypeReader))] PropertyInfo setting, [Remainder] string newValue)
			{
				switch (setting.Name)
				{
					case nameof(IBotSettings.ShardCount):
					{
						if (!uint.TryParse(newValue, out uint number))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Invalid number input."));
							return;
						}

						var validNum = (await Context.Client.GetGuildsAsync()).Count / 2500 + 1;
						if (number < validNum)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"With the current amount of guilds the client has, the minimum shard number is: `{validNum}`."));
							return;
						}

						Context.BotSettings.ShardCount = number;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the shard amount to `{number}`.");
						break;
					}
					case nameof(IBotSettings.MessageCacheCount):
					{
						if (!uint.TryParse(newValue, out uint number))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Invalid number input."));
							return;
						}

						Context.BotSettings.MessageCacheCount = number;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the message cache count to `{number}`.");
						break;
					}
					case nameof(IBotSettings.MaxUserGatherCount):
					{
						if (!uint.TryParse(newValue, out uint number))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Invalid number input."));
							return;
						}

						Context.BotSettings.MaxUserGatherCount = number;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max user gather count to `{number}`.");
						break;
					}
					case nameof(IBotSettings.MaxMessageGatherSize):
					{
						if (!uint.TryParse(newValue, out uint number))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Invalid number input."));
							return;
						}

						Context.BotSettings.MaxMessageGatherSize = number;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max message gather size to `{number}`.");
						break;
					}
					case nameof(IBotSettings.Prefix):
					{
						if (newValue.Length > Constants.MAX_PREFIX_LENGTH)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"Prefix cannot be longer than `{Constants.MAX_PREFIX_LENGTH}` characters."));
							return;
						}

						Context.BotSettings.Prefix = newValue;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the prefix to `{newValue}`.");
						break;
					}
					case nameof(IBotSettings.Game):
					{
						if (newValue.Length > Constants.MAX_GAME_LENGTH)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"Game name cannot be longer than `{Constants.MAX_GAME_LENGTH}` characters or else it doesn't show to other people."));
							return;
						}

						Context.BotSettings.Game = newValue;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `{newValue}`.");
						break;
					}
					case nameof(IBotSettings.Stream):
					{
						if (newValue.Length > Constants.MAX_STREAM_LENGTH)
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"Stream names cannot be longer than `{Constants.MAX_STREAM_LENGTH}` characters."));
							return;
						}
						else if (!MiscActions.MakeSureInputIsValidTwitchAccountName(newValue))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"`{newValue}` is not a valid Twitch stream name."));
							return;
						}

						Context.BotSettings.Game = newValue;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `{newValue}`.");
						break;
					}
					case nameof(IBotSettings.AlwaysDownloadUsers):
					{
						if (!bool.TryParse(newValue, out bool alwaysDLUsers))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The input for always download users has to be a boolean."));
							return;
						}

						Context.BotSettings.AlwaysDownloadUsers = alwaysDLUsers;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set always download users to `{alwaysDLUsers}`.");
						break;
					}
					case nameof(IBotSettings.LogLevel):
					{
						if (!Enum.TryParse(newValue, true, out LogSeverity logLevel))
						{
							await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"The input for log level has to be one of the following: `{String.Join("`, `", Enum.GetNames(typeof(LogSeverity)))}`."));
							return;
						}

						Context.BotSettings.LogLevel = logLevel;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the log level to `{logLevel.EnumName()}`.");
						break;
					}
					default:
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"The setting `{setting.Name}` cannot be set through this command.");
						return;
					}
				}
			}
			[Command(nameof(ActionType.Clear)), Alias("c")]
			public async Task CommandClear([OverrideTypeReader(typeof(BotSettingNonIEnumerableTypeReader))] PropertyInfo setting)
			{
				switch (setting.Name)
				{
					//Some of these need to be accessed before their default value actually gets set (cause the getter does some checking, like if messagecachecount > 1 _Messagecachecount = x)
					case nameof(IBotSettings.MessageCacheCount):
					{
						Context.BotSettings.MessageCacheCount = 0;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the message cache count to `{Context.BotSettings.MessageCacheCount}`.");
						break;
					}
					case nameof(IBotSettings.MaxUserGatherCount):
					{
						Context.BotSettings.MaxUserGatherCount = 0;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max user gather count to `{Context.BotSettings.MaxUserGatherCount}`.");
						break;
					}
					case nameof(IBotSettings.MaxMessageGatherSize):
					{
						Context.BotSettings.MaxMessageGatherSize = 0;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the max message gather size to `{Context.BotSettings.MaxMessageGatherSize}`.");
						break;
					}
					case nameof(IBotSettings.Prefix):
					{
						Context.BotSettings.Prefix = null;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the prefix to `{Context.BotSettings.Prefix}`.");
						break;
					}
					case nameof(IBotSettings.Game):
					{
						Context.BotSettings.Game = null;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `{Context.BotSettings.Game}`.");
						break;
					}
					case nameof(IBotSettings.Stream):
					{
						Context.BotSettings.Game = null;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the game to `Nothing`.");
						break;
					}
					case nameof(IBotSettings.AlwaysDownloadUsers):
					{
						Context.BotSettings.AlwaysDownloadUsers = true;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set always download users to `{Context.BotSettings.AlwaysDownloadUsers}`.");
						break;
					}
					case nameof(IBotSettings.LogLevel):
					{
						Context.BotSettings.LogLevel = LogSeverity.Warning;
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the log level to `{Context.BotSettings.LogLevel.EnumName()}`.");
						break;
					}
					default:
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"The setting `{setting.Name}` cannot be cleared through this command.");
						return;
					}
				}
			}
		}

		[Group(nameof(DisplayBotSettings)), Alias("dgls")]
		[Usage("[Show|All|Setting Name]")]
		[Summary("Displays global settings. Show gives a list of the setting names.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class DisplayBotSettings : MyModuleBase
		{
			[Command(nameof(ActionType.Show)), Alias("s"), Priority(1)]
			public async Task Command()
			{
				var desc = $"`{String.Join("`, `", GetActions.GetBotSettings().Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Setting Names", desc));
			}
			[Command("all"), Alias("a"), Priority(1)]
			public async Task CommandAll()
			{
				var text = await FormattingActions.FormatAllBotSettings(Context.Client, Context.BotSettings);
				await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, text, "Bot Settings", "Bot Settings");
			}
			[Command, Priority(0)]
			public async Task Command([OverrideTypeReader(typeof(BotSettingTypeReader))] PropertyInfo setting)
			{
				var desc = await FormattingActions.FormatBotSettingInfo(Context.Client, Context.BotSettings, setting);
				if (desc.Length <= Constants.MAX_DESCRIPTION_LENGTH)
				{
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(setting.Name, desc));
				}
				else
				{
					await UploadActions.WriteAndUploadTextFile(Context.Guild, Context.Channel, desc, setting.Name, setting.Name);
				}
			}
		}

		[Group(nameof(ModifyBotName)), Alias("mbn")]
		[Usage("[New Name]")]
		[Summary("Changes the bot's name to the given name.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class ModifyBotName : MyModuleBase
		{
			[Command]
			public async Task Command([Remainder, VerifyStringLength(Target.Name)] string newName)
			{
				await Context.Client.CurrentUser.ModifyAsync(x => x.Username = newName);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed my username to `{newName}`.");
			}
		}

		[Group(nameof(ModifyBotIcon)), Alias("mbi")]
		[Usage("<Attached Image|Embedded Image>")]
		[Summary("Changes the bot's icon to the given image. The image must be smaller than 2.5MB. Inputting nothing removes the bot's icon.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class ModifyBotIcon : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command()
			{
				var attach = Context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
				var embeds = Context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
				var validImages = attach.Concat(embeds);
				if (validImages.Count() == 0)
				{
					await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image());
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the bot's icon.");
					return;
				}
				else if (validImages.Count() > 1)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Too many attached or embedded images."));
					return;
				}

				var imageURL = validImages.First();
				var fileType = await UploadActions.GetFileTypeOrSayErrors(Context, imageURL);
				if (fileType == null)
					return;

				var fileInfo = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.BOT_ICON_LOCATION + fileType);
				using (var webClient = new System.Net.WebClient())
				{
					webClient.DownloadFileAsync(new Uri(imageURL), fileInfo.FullName);
					webClient.DownloadFileCompleted += async (sender, e) => await UploadActions.SetIcon(sender, e, Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(fileInfo.FullName)), Context, fileInfo);
				}
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
	}
	*/
}