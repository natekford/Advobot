using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace GuildModeration
	{
		[Group(nameof(LeaveGuild))]
		[Usage("<Guild ID>")]
		[Summary("Makes the bot leave the guild. Settings and preferences will be preserved.")]
		[OtherRequirement(Precondition.GuildOwner | Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class LeaveGuild : MyModuleBase
		{
			[Command]
			public async Task Command([Optional] ulong guildId)
			{
				if (guildId == 0)
				{
					await Context.Guild.LeaveAsync();
					return;
				}

				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id == (await UserActions.GetBotOwner(Context.Client)).Id)
				{
					var guild = await GuildActions.GetGuild(Context.Client, guildId);
					if (guild == null)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Invalid server supplied."));
						return;
					}

					await guild.LeaveAsync();
					if (Context.Guild.Id != guildId)
					{
						await MessageActions.SendChannelMessage(Context, $"Successfully left the server `{0}` with an ID `{1}`.", guild.Name, guild.Id));
					}
				}
				else if (Context.Guild.Id == guildId)
				{
					await Context.Guild.LeaveAsync();
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Only the bot owner can use this command targetting other guilds."));
				}
			}
		}

		[Group(nameof(ChangeGuildName)), Alias("cgn")]
		[Usage("[Name]")]
		[Summary("Change the name of the guild to the given name.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildName : MyModuleBase
		{
			[Command]
			public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
			{
				await GuildActions.ModifyGuildName(Context.Guild, name, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the guild name to `{0}`.", name));
			}
		}

		[Group(nameof(ChangeGuildRegion)), Alias("cgr")]
		[Usage("<Current|Region ID>")]
		[Summary("Shows or changes the guild's server region. Inputting nothing lists all valid region IDs.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildRegion : MyModuleBase
		{
			private static readonly string[] _ValidRegionIDs =
			{
				"brazil",
				"eu-central",
				"eu-west",
				"hongkong",
				"russia",
				"singapore",
				"sydney",
				"us-east",
				"us-central",
				"us-south",
				"us-west",
			};
			private static readonly string[] _VIPRegionIDs =
			{
				"vip-amsterdam",
				"vip-us-east",
				"vip-us-west",
			};

			private static readonly string _BaseRegions = String.Join("\n", _BaseRegions);
			private static readonly string _VIPRegions = String.Join("\n", _VIPRegionIDs);
			private static readonly string _AllRegions = _BaseRegions + "\n" + _VIPRegions;

			[Command(nameof(ActionType.Current)), Priority(1)]
			public async Task CommandCurrent()
			{
				await MessageActions.SendChannelMessage(Context, $"The guild's current server region is `{0}`.", Context.Guild.VoiceRegionId));
			}
			[Command, Priority(0)]
			public async Task Command(string region)
			{
				if (!(_ValidRegionIDs.CaseInsContains(region) || (Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && _VIPRegionIDs.CaseInsContains(region))))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("No valid region ID was input."));
					return;
				}

				var beforeRegion = Context.Guild.VoiceRegionId;
				await GuildActions.ModifyGuildRegion(Context.Guild, region, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the server region of the guild from `{0}` to `{1}`.", beforeRegion, region));
			}
			[Command]
			public async Task Command()
			{
				var desc = Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? _AllRegions : _BaseRegions;
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Region IDs", desc));
			}
		}

		[Group(nameof(ChangeGuildAFKTimer)), Alias("cgafkt")]
		[Usage("[Number]")]
		[Summary("Updates the guild's AFK timeout.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildAFKTimer : MyModuleBase
		{
			private static readonly uint[] _AFKTimes = { 60, 300, 900, 1800, 3600 };

			[Command]
			public async Task Command(uint time)
			{
				if (!_AFKTimes.Contains(time))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR($"Invalid time input, must be one of the following: `{0}`.", String.Join("`, `", _AFKTimes))));
					return;
				}

				await GuildActions.ModifyGuildAFKTime(Context.Guild, (int)time, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild's AFK timeout to `{0}`.", time));
			}
		}

		[Group(nameof(ChangeGuildAFKChannel)), Alias("cgafkc")]
		[Usage("[Channel]")]
		[Summary("Updates the guild's AFK channel.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildAFKChannel : MyModuleBase
		{
			[Command]
			public async Task Command(IVoiceChannel channel)
			{
				await GuildActions.ModifyGuildAFKChannel(Context.Guild, channel, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild's AFK channel to `{0}`.", channel.FormatChannel()));
			}
		}

		[Group(nameof(ChangeGuildMsgNotif)), Alias("cgmn")]
		[Usage("[AllMessages|MentionsOnly]")]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildMsgNotif : MyModuleBase
		{
			[Command]
			public async Task Command(DefaultMessageNotifications msgNotifs)
			{
				await GuildActions.ModifyGuildDefaultMsgNotifications(Context.Guild, msgNotifs, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the default message notification setting to `{0}`.", msgNotifs.EnumName()));
			}
		}

		[Group(nameof(ChangeGuildVerif)), Alias("cgv")]
		[Usage("[None|Low|Medium|High|Extreme]")]
		[Summary("Changes the verification level. None is the most lenient (no requirements to type), high is the harshest (10 minutes in the guild before new members can type).")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildVerif : MyModuleBase
		{
			[Command]
			public async Task Command(VerificationLevel verif)
			{
				await GuildActions.ModifyGuildVerificationLevel(Context.Guild, verif, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild verification level as `{0}`.", verif.EnumName()));
			}
		}

		[Group(nameof(ChangeGuildIcon)), Alias("cgi")]
		[Usage("<Attached Image|Embedded Image>")]
		[Summary("Changes the guild's icon to the given image. The image must be smaller than 2.5MB. Inputting nothing removes the guild's icon.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildIcon : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command()
			{
				var attach = Context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
				var embeds = Context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
				var validImages = attach.Concat(embeds);
				if (validImages.Count() == 0)
				{
					await Context.Guild.ModifyAsync(x => x.Icon = new Image());
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the guild's icon.");
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

				var fileInfo = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_ICON_LOCATION + fileType);
				using (var webClient = new System.Net.WebClient())
				{
					webClient.DownloadFileAsync(new Uri(imageURL), fileInfo.FullName);
					webClient.DownloadFileCompleted += async (sender, e) => await UploadActions.SetIcon(sender, e, GuildActions.ModifyGuildIcon(Context.Guild, fileInfo, FormattingActions.FormatUserReason(Context.User)), Context, fileInfo);
				}
			}
		}

		[Group(nameof(CreateGuild)), Alias("cg")]
		[Usage("[Name]")]
		[Summary("Creates a guild with the bot as the owner.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class CreateGuild : MyModuleBase
		{
			[Command]
			public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
			{
				var optimalVoiceRegion = await Context.Client.GetOptimalVoiceRegionAsync();
				var guild = await Context.Client.CreateGuildAsync(name, optimalVoiceRegion);

				var defaultChannel = await guild.GetDefaultChannelAsync();
				var invite = await defaultChannel.CreateInviteAsync();
				var DMChannel = await Context.User.GetOrCreateDMChannelAsync();
				await MessageActions.SendDMMessage(DMChannel, invite.Url);
			}
		}

		[Group(nameof(ChangeGuildOwner)), Alias("cgo")]
		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class ChangeGuildOwner : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
				{
					await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"{0} is now the owner.", Context.User.Mention));
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The bot is not the owner of the guild."));
			}
		}

		[Group(nameof(DeleteGuild)), Alias("dg")]
		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public sealed class DeleteGuild : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
				{
					await Context.Guild.DeleteAsync();
					return;
				}

				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("The bot is not the owner of the guild and thus cannot delete it."));
			}
		}
	}
}
