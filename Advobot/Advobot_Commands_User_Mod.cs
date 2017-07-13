using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace UserModeration
	{
		[Group("unban")]
		[Alias("ub")]
		[Usage("<User ID|\"Username#Discriminator\"> <True|False>")]
		[Summary("Unbans the user from the guild. If the reason argument is true it only says the reason without unbanning.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public class Unban : MyModuleBase
		{
			[Command]
			public async Task Command(IBan ban, [Optional] bool reason)
			{
				await CommandRunner(ban, reason);
			}

			private async Task CommandRunner(IBan ban, bool reason)
			{
				if (reason)
				{
					await Actions.SendChannelMessage(Context, String.Format("`{0}`'s ban reason is `{1}`.", ban.User.FormatUser(), ban.Reason ?? "Nothing"));
				}
				else
				{
					await Context.Guild.RemoveBanAsync(ban.User);
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully unbanned `{0}`", ban.User.FormatUser()));
				}
			}
		}

		[Group("removemessages")]
		[Alias("rm")]
		[Usage("[Number of Messages] <User> <Channel>")]
		[Summary("Removes the selected number of messages from either the user, the channel, both, or, if neither is input, the current channel. These arguments need to be mentions to work.")]
		[PermissionRequirement(new[] { GuildPermission.ManageMessages }, null)]
		[DefaultEnabled(true)]
		public class RemoveMessages : MyModuleBase
		{
			[Command]
			public async Task Command(uint requestCount, [Optional] IGuildUser user, [Optional, VerifyObject(ObjectVerification.CanDeleteMessages)] ITextChannel channel)
			{
				await CommandRunner((int)requestCount, user, channel);
			}
			[Command]
			public async Task Command(uint requestCount, [Optional, VerifyObject(ObjectVerification.CanDeleteMessages)] ITextChannel channel, [Optional] IGuildUser user)
			{
				await CommandRunner((int)requestCount, user, channel);
			}

			private async Task CommandRunner(int requestCount, IGuildUser user, ITextChannel channel)
			{
				if (channel == null)
				{
					//Default to channel command was said on if no channel was specified
					var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanDeleteMessages }, Context.Channel as ITextChannel);
					if (returnedChannel.Reason != FailureReason.NotFailure)
					{
						await Actions.HandleObjectGettingErrors(Context, returnedChannel);
						return;
					}
					channel = returnedChannel.Object as ITextChannel;
				}
				
				var serverLog = ((DiscordObjectWithID<ITextChannel>)Context.GuildInfo.GetSetting(SettingOnGuild.ServerLog))?.ID == channel.Id;
				var modLog = ((DiscordObjectWithID<ITextChannel>)Context.GuildInfo.GetSetting(SettingOnGuild.ModLog))?.ID == channel.Id;
				var imageLog = ((DiscordObjectWithID<ITextChannel>)Context.GuildInfo.GetSetting(SettingOnGuild.ImageLog))?.ID == channel.Id;
				if (Context.User.Id != Context.Guild.OwnerId && (serverLog || modLog || imageLog))
				{
					var DMChannel = await Context.Guild.Owner.GetOrCreateDMChannelAsync();
					await Actions.SendDMMessage(DMChannel, String.Format("`{0}` is trying to delete stuff from a log channel: `{1}`.", Context.User.FormatUser(), channel.FormatChannel()));
					return;
				}

				var response = String.Format("Successfully deleted `{0}` message{1}", await Actions.RemoveMessages(channel, Context.Message, user, requestCount), Actions.GetPlural(requestCount));
				var userResp = user != null ? String.Format(" from `{0}`", user.FormatUser()) : null;
				var chanResp = channel != null ? String.Format(" on `{0}`", channel.FormatChannel()) : null;
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.JoinNonNullStrings(" ", response, userResp, chanResp) + ".");
			}
		}
	}
	/*
	//User Moderation commands are commands that affect the users of a guild
	[Name("UserModeration")]
	public class Advobot_Commands_User_Mod : ModuleBase
	{
		[Command("mute")]
		[Alias("m")]
		[Usage("[User] <Time>")]
		[Summary("Prevents a user from typing and speaking and doing much else in the server. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles, GuildPermission.ManageMessages }, null)]
		[DefaultEnabled(true)]
		public async Task FullMute([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var timeStr = returnedArgs.Arguments[1];

			//Get the time
			var time = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time."));
					return;
				}
			}

			//Get the user
			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			await Actions.MuteUser(guildInfo, user, time);

			var response = String.Format("Successfully muted `{0}`.", user.FormatUser());
			if (time != 0)
			{
				response += String.Format("\nThe mute will last for `{0}` minute{1}.", time, Actions.GetPlural(time));
			}
			await Actions.MakeAndDeleteSecondaryMessage(Context, response);
		}

		[Command("voicemute")]
		[Alias("vm")]
		[Usage("[User] <Time")]
		[Summary("Prevents a user from speaking. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(new[] { GuildPermission.MuteMembers }, null)]
		[DefaultEnabled(true)]
		public async Task Mute([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var timeStr = returnedArgs.Arguments[1];

			//Get the time
			var time = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time."));
					return;
				}
			}

			//Get the user
			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			//See if it should mute or unmute
			if (!user.IsMuted)
			{
				await user.ModifyAsync(x => x.Mute = true);
				var response = String.Format("Successfully muted `{0}`.", user.FormatUser());
				if (time != 0)
				{
					Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, user.Id, PunishmentType.Mute, DateTime.UtcNow.AddMinutes(time)));
					response += String.Format("The mute will last for `{0}` minute{1}.", time, Actions.GetPlural(time));
				}
				await Actions.MakeAndDeleteSecondaryMessage(Context, response);
			}
			else
			{
				await user.ModifyAsync(x => x.Mute = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully unmuted `{0}`.", user.FormatUser()));
			}
		}

		[Command("deafen")]
		[Alias("dfn", "d")]
		[Usage("[User] <Time>")]
		[Summary("Prevents a user from hearing. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(new[] { GuildPermission.DeafenMembers }, null)]
		[DefaultEnabled(true)]
		public async Task Deafen([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var timeStr = returnedArgs.Arguments[1];

			//Get the time
			var time = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time."));
					return;
				}
			}

			//Get the user
			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			//See if it should deafen or undeafen
			if (!user.IsMuted)
			{
				await user.ModifyAsync(x => x.Deaf = true);
				var response = String.Format("Successfully deafened `{0}`.", user.FormatUser());
				if (time != 0)
				{
					Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, user.Id, PunishmentType.Deafen, DateTime.UtcNow.AddMinutes(time)));
					response += String.Format("The deafen will last for `{0}` minute{1}.", time, Actions.GetPlural(time));
				}
				await Actions.MakeAndDeleteSecondaryMessage(Context, response);
			}
			else
			{
				await user.ModifyAsync(x => x.Deaf = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully undeafened `{0}`.", user.FormatUser()));
			}
		}

		[Command("moveuser")]
		[Alias("mu")]
		[Usage("[User] [Channel]")]
		[Summary("Moves the user to the given voice channel.")]
		[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
		[DefaultEnabled(true)]
		public async Task MoveUser([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];

			//Check if valid user and that they're in a voice channel
			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.CanBeMovedFromChannel }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			var userChan = user.VoiceChannel;
			if (userChan == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is not in a voice channel."));
				return;
			}

			//Check if valid channel that the user can edit
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanMoveUsers, ObjectVerification.IsVoice }, false, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object as IVoiceChannel;

			//See if trying to put user in the exact same channel
			if (userChan == channel)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is already in that channel"));
				return;
			}

			await user.ModifyAsync(x => x.Channel = Optional.Create(channel));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully moved `{0}` to `{1}`.", user.FormatUser(), channel.Name));
		}

		[Command("moveusers")]
		[Alias("mus")]
		[Usage("[Channel] [Channel]")]
		[Summary("Moves all users from one channel to another.")]
		[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
		[DefaultEnabled(true)]
		public async Task MoveUsers([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var inputChanStr = returnedArgs.Arguments[0];
			var outputChanStr = returnedArgs.Arguments[1];

			//Get input channel
			var returnedInputChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanMoveUsers, ObjectVerification.IsVoice }, false, inputChanStr);
			if (returnedInputChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedInputChannel);
				return;
			}
			var inputChannel = returnedInputChannel.Object as IVoiceChannel;

			//Get output channel
			var returnedOutputChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanMoveUsers, ObjectVerification.IsVoice }, false, outputChanStr);
			if (returnedOutputChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedOutputChannel);
				return;
			}
			var outputChannel = returnedOutputChannel.Object as IVoiceChannel;

			//Grab all of the users in the input channel
			var users = (await inputChannel.GetUsersAsync().Flatten()).ToList();

			//Have the bot stay in the typing state and have a message that can be updated
			Actions.DontWaitForResultOfBigUnimportantFunction(Context.Channel, async () =>
			{
				var msg = await Actions.SendChannelMessage(Context, String.Format("Attempting to move `{0}` user{1}.", users.Count, Actions.GetPlural(users.Count))) as IUserMessage;

				//Move them all
				var count = 0;
				await users.ForEachAsync(async x =>
				{
					++count;
					if (count % 10 == 0)
					{
						await msg.ModifyAsync(y => y.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((users.Count - count) * 1.2)));
					}

					await x.ModifyAsync(y => y.Channel = Optional.Create(outputChannel));
				});

				//Send a success message
				var desc = String.Format("Successfully moved `{0}` user{1} from `{2}` to `{3}`.", users.Count, Actions.GetPlural(users.Count), inputChannel.FormatChannel(), outputChannel.FormatChannel());
				await Actions.MakeAndDeleteSecondaryMessage(Context, desc);
			});
		}

		[Command("prunemembers")]
		[Alias("pmems")]
		[Usage("[1|7|30] [True|False]")]
		[Summary("Removes users who have no roles and have not been seen in the past given amount of days. True means an actual prune, otherwise this returns the number of users that would have been pruned.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public async Task PruneMembers([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var dayStr = returnedArgs.Arguments[0];
			var simStr = returnedArgs.Arguments[1];

			if (String.IsNullOrWhiteSpace(dayStr))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Days has to be input."));
				return;
			}
			if (!int.TryParse(dayStr, out int amountOfDays))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for days is not a number."));
				return;
			}
			else if (!new[] { 1, 7, 30 }.Contains(amountOfDays))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for days is not a valid number."));
				return;
			}

			if (String.IsNullOrWhiteSpace(simStr))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The bool for simulate has to be input."));
				return;
			}
			if (!bool.TryParse(simStr, out bool simulate))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for simulate is not a bool."));
				return;
			}

			var amount = await Context.Guild.PruneUsersAsync(amountOfDays, simulate);
			if (simulate)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` members would have been pruned with a prune period of `{1}` days.", amount, amountOfDays));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` members have been pruned with a prune period of `{1}` days.", amount, amountOfDays));
			}
		}

		[Command("softban")]
		[Alias("sb")]
		[Usage("[User] <Reason>")]
		[Summary("Bans then unbans a user from the guild. Removes all recent messages from them.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public async Task SoftBan([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var reasonStr = returnedArgs.Arguments[1];

			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.CanBeEdited }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			await Actions.UserBanUser(Context, user.Id, 1, reasonStr);
			await Context.Guild.RemoveBanAsync(user);

			var response = String.Format("Successfully softbanned `{0}`.", user.FormatUser());
			if (!String.IsNullOrWhiteSpace(reasonStr))
			{
				response += String.Format(" The given reason for softbanning is: `{0}`.", reasonStr);
			}
			await Actions.MakeAndDeleteSecondaryMessage(Context, response);
		}

		[Command("ban")]
		[Alias("b")]
		[Usage("[User] <Reason> <Days:int> <Time:int>")]
		[Summary("Bans the user from the guild. Days specifies how many days worth of messages to delete. Time specifies how long and is in minutes.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public async Task Ban([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 3), new[] { "days", "time" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var reasonStr = returnedArgs.Arguments[1];
			var daysStr = returnedArgs.GetSpecifiedArg("days");
			var timeStr = returnedArgs.GetSpecifiedArg("time");

			if (int.TryParse(daysStr, out int pruneDays))
			{
				if (pruneDays > 7 || pruneDays < 0)
				{
					pruneDays = 1;
				}
			}
			else
			{
				pruneDays = 1;
			}

			if (int.TryParse(timeStr, out int timeForBan))
			{
				if (timeForBan < 0)
				{
					timeForBan = 0;
				}
			}
			else
			{
				timeForBan = 0;
			}

			//First try to get the ID out. If the ID is not on the guild then no need to check if the user and bot have the permission to ban the person since they won't have perms
			IGuildUser user = null;
			if ((ulong.TryParse(userStr, out ulong banID) || MentionUtils.TryParseUser(userStr, out banID)) && !(Context.Guild as Discord.WebSocket.SocketGuild).Users.Select(x => x.Id).Contains(banID))
			{
			}
			else
			{
				var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.CanBeEdited }, true, userStr);
				if (returnedUser.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedUser);
					return;
				}
				user = returnedUser.Object;
				banID = user.Id;
			}

			//Make sure not banning already banned person
			var ban = (await Context.Guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == banID);
			if (ban != null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The user `{0}` is already banned from the server.", ban.User.FormatUser())));
				return;
			}

			await Actions.UserBanUser(Context, banID, pruneDays, reasonStr);
			if (timeForBan != 0)
			{
				Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, banID, PunishmentType.Ban, DateTime.UtcNow.AddMinutes(timeForBan)));
			}

			var response = String.Format("Successfully banned `{0}`.", (await Context.Guild.GetBansAsync()).FirstOrDefault(x => x.User.Id == banID).User.FormatUser());
			if (pruneDays != 0)
			{
				response += String.Format(" Also deleted `{0}` day{1} worth of messages.", pruneDays, Actions.GetPlural(pruneDays));
			}
			if (timeForBan != 0)
			{
				response += String.Format(" The user will be unbanned in `{0}` minute{1}.", timeForBan, Actions.GetPlural(timeForBan));
			}
			if (!String.IsNullOrWhiteSpace(reasonStr))
			{
				response += String.Format(" The given reason for banning is: `{0}`.", reasonStr);
			}
			await Actions.SendChannelMessage(Context, response);
		}

		[Command("kick")]
		[Alias("k")]
		[Usage("[User] <Reason>")]
		[Summary("Kicks the user from the guild.")]
		[PermissionRequirement(new[] { GuildPermission.KickMembers }, null)]
		[DefaultEnabled(true)]
		public async Task Kick([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var reasonStr = returnedArgs.Arguments[1];

			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.CanBeEdited }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			await Actions.UserKickUser(Context, user, reasonStr);

			var response = String.Format("Successfully kicked `{0}`.", user.FormatUser());
			if (!String.IsNullOrWhiteSpace(reasonStr))
			{
				response += String.Format(" The given reason for kicking is: `{0}`.", reasonStr);
			}
			await Actions.MakeAndDeleteSecondaryMessage(Context, response);
		}

		[Command("displaycurrentbanlist")]
		[Alias("dcbl")]
		[Usage("")]
		[Summary("Displays all the bans on the guild.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public async Task CurrentBanList()
		{
			var bans = (await Context.Guild.GetBansAsync()).ToList();
			if (!bans.Any())
			{
				await Actions.SendChannelMessage(Context, "This guild has no bans.");
				return;
			}


			var str = bans.FormatNumberedList("`{0}`", x => x.User.FormatUser());
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Bans", str));
		}

		

		[Command("modifyslowmode")]
		[Alias("msm")]
		[Usage("<\"Roles:.../.../\"> <Messages:1 to 5> <Time:1 to 30> <Guild:Yes> | [Off] [Guild|Channel|All]")]
		[Summary("The first argument is the roles that get ignored by slowmode, the second is the amount of messages, and the third is the time period. Default is: none, 1, 5." +
			"Bots are unaffected by slowmode. Any users who are immune due to roles stay immune even if they lose said role until a new slowmode is started.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public async Task SlowMode([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 4), new[] { "roles", "messages", "time", "guild" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var roleStr = returnedArgs.GetSpecifiedArg("roles");
			var msgStr = returnedArgs.GetSpecifiedArg("messages");
			var timeStr = returnedArgs.GetSpecifiedArg("time");
			var guildStr = returnedArgs.GetSpecifiedArg("guild");

			if (Actions.CaseInsEquals(returnedArgs.Arguments[0], "off"))
			{
				var targStr = returnedArgs.Arguments[1];
				if (returnedArgs.ArgCount != 2)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				}
				else if (Actions.CaseInsEquals(targStr, "guild"))
				{
					guildInfo.SetSetting(SettingOnGuild.SlowmodeGuild, null, false);
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the guild.");
				}
				else if (Actions.CaseInsEquals(targStr, "channel"))
				{
					((List<SlowmodeChannel>)guildInfo.GetSetting(SettingOnGuild.SlowmodeChannels)).ThreadSafeRemoveAll(x => x.ChannelID == Context.Channel.Id);
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the channel.");
				}
				else if (Actions.CaseInsEquals(targStr, "all"))
				{
					guildInfo.SetSetting(SettingOnGuild.SlowmodeGuild, null, false);
					((List<SlowmodeChannel>)guildInfo.GetSetting(SettingOnGuild.SlowmodeChannels)).Clear();
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed all slowmodes on the guild and its channels.");
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("With off, the second argument must be either Guild, Channel, or All."));
				}
				return;
			}

			//Check if the target is already in either dictionary
			var guild = !String.IsNullOrWhiteSpace(guildStr);
			if (guild)
			{
				var smGuild = ((SlowmodeGuild)guildInfo.GetSetting(SettingOnGuild.SlowmodeGuild));
				if (smGuild != null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Guild already is in slowmode.");
					return;
				}
			}
			else
			{
				var smChannel = ((List<SlowmodeChannel>)guildInfo.GetSetting(SettingOnGuild.SlowmodeChannels)).FirstOrDefault(x => x.ChannelID == Context.Channel.Id);
				if (smChannel != null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Channel already is in slowmode.");
					return;
				}
			}

			//Get the roles
			var roles = new List<IRole>();
			if (!String.IsNullOrWhiteSpace(roleStr))
			{
				roleStr.Split('/').ToList().ForEach(x =>
				{
					var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.None }, false, x);
					if (returnedRole.Reason == FailureReason.NotFailure)
					{
						roles.Add(returnedRole.Object);
					}
				});
			}
			roles = roles.Distinct().ToList();
			var roleNames = roles.Select(x => x.Name);
			var roleIDs = roles.Select(x => x.Id);

			//Get the messages limit
			var msgsLimit = 1;
			if (!String.IsNullOrWhiteSpace(msgStr))
			{
				if (int.TryParse(msgStr, out msgsLimit))
				{
					if (msgsLimit > 5 || msgsLimit < 1)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Message limit must be between 1 and 5 inclusive."));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for messages was not a number. Remember: no space after the colon."));
					return;
				}
			}

			//Get the time limit
			var timeLimit = 5;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (int.TryParse(timeStr, out timeLimit))
				{
					if (timeLimit > 30 || timeLimit < 1)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Time must be between 1 and 10 inclusive."));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for time was not a number. Remember: no space after the colon."));
					return;
				}
			}

			var slowmodeUsers = (await Context.Guild.GetUsersAsync()).Where(x => !x.RoleIds.Intersect(roleIDs).Any()).Select(x => new SlowmodeUser(x, msgsLimit, timeLimit)).ToList();
			if (guild)
			{
				guildInfo.SetSetting(SettingOnGuild.SlowmodeGuild, new SlowmodeGuild(msgsLimit, timeLimit, slowmodeUsers), false);
			}
			else
			{
				guildInfo.SetSetting(SettingOnGuild.SlowmodeChannels, new SlowmodeGuild(msgsLimit, timeLimit, slowmodeUsers), false);
			}

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled slowmode on `{0}` with a message limit of `{1}` and time interval of `{2}` seconds.{3}",
				guild ? Context.Guild.FormatGuild() : Context.Channel.FormatChannel(),
				msgsLimit,
				timeLimit,
				roleNames.Any() ? String.Format("\nImmune roles: `{0}`.", String.Join("`, `", roleNames)) : ""));
		}

		//TODO: Split this up into separate commands
		/*
		[Command("forallwithrole")]
		[Alias("fawr")]
		[Usage("[Give_Role|GR|Take_Role|TR|Give_Nickname|GNN|Take_Nickname|TNN] [\"Role\"] <\"Role\"|\"Nickname\"> <" + Constants.BYPASS_STRING + ">")]
		[Summary("Max is 100 users per use unless the bypass string is said. All actions but `Take_Nickame` require the output role/nickname.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task ForAllWithRole([Remainder] string input)
		{
			//Split arguments
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 4));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var inputStr = returnedArgs.Arguments[1];
			var outputStr = returnedArgs.Arguments[2];

			if (!Enum.TryParse(actionStr, true, out FAWRType action))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
			action = Actions.ClarifyFAWRType(action);

			if (action != FAWRType.Take_Nickname)
			{
				if (returnedArgs.ArgCount < 3)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
			}

			//Input role
			var returnedInputRole = Actions.GetRole(Context, new[] { RoleCheck.None }, false, inputStr);
			if (returnedInputRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedInputRole);
				return;
			}
			var inputRole = returnedInputRole.Object;

			switch (action)
			{
				case FAWRType.Give_Role:
				{
					if (Actions.CaseInsEquals(inputStr, outputStr))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot give the same role that is being gathered."));
						return;
					}
					break;
				}
				case FAWRType.Give_Nickname:
				{
					if (outputStr.Length > Constants.MAX_NICKNAME_LENGTH)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be longer than `{0}` charaters.", Constants.MAX_NICKNAME_LENGTH)));
						return;
					}
					else if (outputStr.Length < Constants.MIN_NICKNAME_LENGTH)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be less than `{0}` characters.", Constants.MIN_NICKNAME_LENGTH)));
						return;
					}
					break;
				}
			}

			//Get the amount of users allowed
			var len = Actions.GetMaxNumOfUsersToGather(Context, returnedArgs.Arguments);
			var users = (await Actions.GetUsersTheBotAndUserCanEdit(Context, (x => x.RoleIds.Contains(inputRole.Id)))).GetUpToAndIncludingMinNum(len);
			var userCount = users.Count;
			if (userCount == 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to find any users with the input role that could be modified."));
				return;
			}

			//Nickname stuff
			switch (action)
			{
				case FAWRType.Give_Nickname:
				{
					Actions.RenicknameALotOfPeople(Context, users, outputStr).Forget();
					return;
				}
				case FAWRType.Take_Nickname:
				{
					Actions.RenicknameALotOfPeople(Context, users, null).Forget();
					return;
				}
			}

			//Output role
			var returnedOutputRole = Actions.GetRole(Context, new[] { RoleCheck.CanBeEdited, RoleCheck.IsEveryone }, false, outputStr);
			if (returnedOutputRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedOutputRole);
				return;
			}
			var outputRole = returnedOutputRole.Object;

			//Make sure the users trying to give role to don't have it and trying to take from do have it.
			switch (action)
			{
				case FAWRType.Give_Role:
				{
					users = users.Where(x => !x.RoleIds.Contains(outputRole.Id)).ToList();
					break;
				}
				case FAWRType.Take_Role:
				{
					users = users.Where(x => x.RoleIds.Contains(outputRole.Id)).ToList();
					break;
				}
			}

			var msg = await Actions.SendChannelMessage(Context, String.Format("Attempted to edit `{0}` user{1}.", userCount, Actions.GetPlural(userCount))) as IUserMessage;
			var typing = Context.Channel.EnterTypingState();
			var count = 0;

			Task.Run(async () =>
			{
				switch (action)
				{
					case FAWRType.Give_Role:
					{
						foreach (var user in users)
						{
							++count;
							if (count % 10 == 0)
							{
								await msg.ModifyAsync(x => x.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
								if (Context.Guild.GetRole(outputRole.Id) == null)
								{
									await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.GiveRole(user, outputRole);
						}

						await Actions.SendChannelMessage(Context, String.Format("Successfully gave the role `{0}` to `{1}` users.", outputRole.FormatRole(), count));
						break;
					}
					case FAWRType.Take_Role:
					{
						foreach (var user in users)
						{
							++count;
							if (count % 10 == 0)
							{
								await msg.ModifyAsync(x => x.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
								if (Context.Guild.GetRole(outputRole.Id) == null)
								{
									await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.TakeRole(user, outputRole);
						}

						await Actions.SendChannelMessage(Context, String.Format("Successfully took the role `{0}` from `{1}` users.", outputRole.FormatRole(), count));
						break;
					}
				}
				typing.Dispose();
				await msg.DeleteAsync();
			}).Forget();
		}
	}*/
}
