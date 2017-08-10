using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using Advobot.SavedClasses;
using Advobot.TypeReaders;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace UserModeration
	{
		[Group("mute"), Alias("m")]
		[Usage("[User] <Number>")]
		[Summary("Prevents a user from typing and speaking in the guild. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles, GuildPermission.ManageMessages }, null)]
		[DefaultEnabled(true)]
		public sealed class Mute : MyModuleBase
		{
			[Command]
			public async Task Command(IGuildUser user, [Optional] uint time)
			{
				await CommandRunner(user, time);
			}

			private async Task CommandRunner(IGuildUser user, uint time)
			{
				var muteRole = await RoleActions.GetMuteRole(Context.GuildSettings, user.Guild, user);
				if (user.RoleIds.Contains(muteRole.Id))
				{
					await PunishmentActions.ManualRoleUnmuteUser(user, muteRole, FormattingActions.FormatUserReason(Context.User), Context.Timers);

					var response = String.Format("Successfully unmuted `{0}`.", user.FormatUser());
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
				}
				else
				{
					await PunishmentActions.ManualRoleMuteUser(user, muteRole, FormattingActions.FormatUserReason(Context.User), time, Context.Timers);

					var response = String.Format("Successfully muted `{0}`.", user.FormatUser());
					if (time != 0)
					{
						response += String.Format("\nThe mute will last for `{0}` minute{1}.", time, GetActions.GetPlural(time));
					}
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
				}
			}
		}

		[Group("voicemute"), Alias("vm")]
		[Usage("[User] <Time")]
		[Summary("Prevents a user from speaking. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(new[] { GuildPermission.MuteMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class VoiceMute : MyModuleBase
		{
			[Command]
			public async Task Command(IGuildUser user, [Optional] uint time)
			{
				await CommandRunner(user, time);
			}

			private async Task CommandRunner(IGuildUser user, uint time)
			{
				if (user.IsMuted)
				{
					await PunishmentActions.ManualVoiceUnmuteUser(user, FormattingActions.FormatUserReason(Context.User), Context.Timers);

					var response = String.Format("Successfully unvoicemuted `{0}`.", user.FormatUser());
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
				}
				else
				{
					await PunishmentActions.ManualVoiceMuteUser(user, FormattingActions.FormatUserReason(Context.User), time, Context.Timers);

					var response = String.Format("Successfully voicemuted `{0}`.", user.FormatUser());
					if (time != 0)
					{
						response += String.Format("\nThe voicemute will last for `{0}` minute{1}.", time, GetActions.GetPlural(time));
					}
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
				}
			}
		}

		[Group("deafen"), Alias("dfn", "d")]
		[Usage("[User] <Time>")]
		[Summary("Prevents a user from hearing. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(new[] { GuildPermission.DeafenMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class Deafen : MyModuleBase
		{
			[Command]
			public async Task Command(IGuildUser user, [Optional] uint time)
			{
				await CommandRunner(user, time);
			}

			private async Task CommandRunner(IGuildUser user, uint time)
			{
				if (user.IsDeafened)
				{
					await PunishmentActions.ManualUndeafenUser(user, FormattingActions.FormatUserReason(Context.User), Context.Timers);

					var response = String.Format("Successfully undeafened `{0}`.", user.FormatUser());
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
				}
				else
				{
					await PunishmentActions.ManualDeafenUser(user, FormattingActions.FormatUserReason(Context.User), time, Context.Timers);

					var response = String.Format("Successfully deafened `{0}`.", user.FormatUser());
					if (time != 0)
					{
						response += String.Format("\nThe deafen will last for `{0}` minute{1}.", time, GetActions.GetPlural(time));
					}
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
				}
			}
		}

		[Group("moveuser"), Alias("mu")]
		[Usage("[User] [Channel]")]
		[Summary("Moves the user to the given voice channel.")]
		[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class MoveUser : MyModuleBase
		{
			[Command]
			public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel channel)
			{
				await CommandRunner(user, channel);
			}

			private async Task CommandRunner(IGuildUser user, IVoiceChannel channel)
			{
				if (user.VoiceChannel == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("User is not in a voice channel."));
					return;
				}
				else if (user.VoiceChannel == channel)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("User is already in that channel."));
					return;
				}

				await UserActions.MoveUser(user, channel, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully moved `{0}` to `{1}`.", user.FormatUser(), channel.FormatChannel()));
			}
		}

		//TODO: put in cancel tokens for the commands that user bypass strings in case people need to cancel
		[Group("moveusers"), Alias("mus")]
		[Usage("[Channel] [Channel] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Moves all users from one channel to another. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class MoveUsers : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command([VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel inputChannel,
									  [VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel outputChannel,
									  [OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				await CommandRunner(inputChannel, outputChannel, bypass);
			}

			private async Task CommandRunner(IVoiceChannel inputChannel, IVoiceChannel outputChannel, bool bypass)
			{
				var userAmt = GetActions.GetMaxAmountOfUsersToGather(Context.BotSettings, bypass);
				var users = (await inputChannel.GetUsersAsync().Flatten()).ToList().GetUpToAndIncludingMinNum(userAmt);

				await UserActions.MoveManyUsers(Context, users, outputChannel, FormattingActions.FormatUserReason(Context.User));
			}
		}

		[Group("pruneusers"), Alias("pu")]
		[Usage("[1|7|30] <" + ACTUAL_PRUNE_STRING + ">")]
		[Summary("Removes users who have no roles and have not been seen in the given amount of days. If the optional argument is not typed exactly, then the bot will only give a number of how many people will be kicked.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class PruneUsers : MyModuleBase
		{
			[Command]
			public async Task Command(uint days, string pruneStr)
			{
				await CommandRunner(days, pruneStr);
			}

			private const string ACTUAL_PRUNE_STRING = "ActualPrune";
			private static readonly uint[] _Days = { 1, 7, 30 };

			private async Task CommandRunner(uint days, string pruneStr)
			{
				if (_Days.Contains(days))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("Invalid days supplied, must be one of the following: `{0}`", String.Join("`, `", _Days))));
					return;
				}

				var simulate = !ACTUAL_PRUNE_STRING.Equals(pruneStr);
				var amt = await GuildActions.PruneUsers(Context.Guild, (int)days, simulate, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` members{1} have been pruned with a prune period of `{2}` days.", amt, (simulate ? " would" : ""), days));
			}
		}

		[Group("softban"), Alias("sb")]
		[Usage("[User]")]
		[Summary("Bans then unbans a user, which removes all recent messages from them.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class SoftBan : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user)
			{
				await CommandRunner(user);
			}

			private async Task CommandRunner(IGuildUser user)
			{
				await PunishmentActions.ManualSoftban(Context.Guild, user.Id, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.SendChannelMessage(Context, String.Format("Successfully softbanned `{0}`.", user.FormatUser()));
			}
		}

		[Group("ban"), Alias("b")]
		[Usage("[User] <Time> <Days>")]
		[Summary("Bans the user from the guild. Days specifies how many days worth of messages to delete. Time specifies how long and is in minutes.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class Ban : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IUser user, [Optional] uint time, [Optional] uint days)
			{
				await CommandRunner(user, time, days);
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IUser user, [Optional] uint time)
			{
				await CommandRunner(user, time, 0);
			}

			private static readonly uint _MaxDays = 7;

			private async Task CommandRunner(IUser user, uint time, uint days)
			{
				if (days > _MaxDays)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("Days must be less than or equal to `{0}`.", _MaxDays)));
					return;
				}
				else if ((await Context.Guild.GetBansAsync()).Select(x => x.User.Id).Contains(user.Id))
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("That user is already banned."));
					return;
				}

				await PunishmentActions.ManualBan(Context.Guild, user.Id, FormattingActions.FormatUserReason(Context.User), (int)days, time, Context.Timers);
				await MessageActions.SendChannelMessage(Context, String.Format("Successfully banned `{0}`.", user.FormatUser()));
			}
		}

		[Group("unban"), Alias("ub")]
		[Usage("<User ID|\"Username#Discriminator\"> <True|False>")]
		[Summary("Unbans the user from the guild. If the reason argument is true it only says the reason without unbanning.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class Unban : MyModuleBase
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
					await MessageActions.SendChannelMessage(Context, String.Format("`{0}`'s ban reason is `{1}`.", ban.User.FormatUser(), ban.Reason ?? "Nothing"));
				}
				else
				{
					await PunishmentActions.ManualUnbanUser(Context.Guild, ban.User.Id, FormattingActions.FormatUserReason(Context.User));
					await MessageActions.SendChannelMessage(Context, String.Format("Successfully unbanned `{0}`", ban.User.FormatUser()));
				}
			}
		}

		[Group("kick"), Alias("k")]
		[Usage("[User] <Reason>")]
		[Summary("Kicks the user from the guild.")]
		[PermissionRequirement(new[] { GuildPermission.KickMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class Kick : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, Remainder] string reason)
			{
				await CommandRunner(user, reason);
			}

			private async Task CommandRunner(IGuildUser user, string reason)
			{
				await PunishmentActions.ManualKick(user, FormattingActions.FormatUserReason(Context.User));
				await MessageActions.SendChannelMessage(Context, String.Format("Successfully kicked `{0}`.", user.FormatUser()));
			}
		}

		[Group("displaycurrentbanlist"), Alias("dcbl")]
		[Usage("")]
		[Summary("Displays all the bans on the guild.")]
		[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
		[DefaultEnabled(true)]
		public sealed class DisplayCurrentBanList : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var bans = await Context.Guild.GetBansAsync();
				if (!bans.Any())
				{
					await MessageActions.SendChannelMessage(Context, "This guild has no bans.");
					return;
				}

				var desc = bans.FormatNumberedList("`{0}`", x => x.User.FormatUser());
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Current Bans", desc));
			}
		}

		[Group("removemessages"), Alias("rm")]
		[Usage("[Number] <User> <Channel>")]
		[Summary("Removes the provided number of messages from either the user, the channel, both, or, if neither is input, the current channel.")]
		[PermissionRequirement(new[] { GuildPermission.ManageMessages }, null)]
		[DefaultEnabled(true)]
		public sealed class RemoveMessages : MyModuleBase
		{
			[Command]
			public async Task Command(uint requestCount, [Optional] IGuildUser user, [Optional, VerifyObject(true, ObjectVerification.CanDeleteMessages)] ITextChannel channel)
			{
				await CommandRunner((int)requestCount, user, channel ?? Context.Channel as ITextChannel);
			}
			[Command]
			public async Task Command(uint requestCount, [Optional, VerifyObject(true, ObjectVerification.CanDeleteMessages)] ITextChannel channel, [Optional] IGuildUser user)
			{
				await CommandRunner((int)requestCount, user, channel ?? Context.Channel as ITextChannel);
			}

			private async Task CommandRunner(int requestCount, IGuildUser user, ITextChannel channel)
			{
				var serverLog = Context.GuildSettings.ServerLog?.Id == channel.Id;
				var modLog = Context.GuildSettings.ModLog?.Id == channel.Id;
				var imageLog = Context.GuildSettings.ImageLog?.Id == channel.Id;
				if (Context.User.Id != Context.Guild.OwnerId && (serverLog || modLog || imageLog))
				{
					var DMChannel = await (await Context.Guild.GetOwnerAsync()).GetOrCreateDMChannelAsync();
					await MessageActions.SendDMMessage(DMChannel, String.Format("`{0}` is trying to delete stuff from a log channel: `{1}`.", Context.User.FormatUser(), channel.FormatChannel()));
					return;
				}

				var deletedAmt = await MessageActions.RemoveMessages(channel, Context.Message, requestCount, user, FormattingActions.FormatUserReason(Context.User));

				var response = String.Format("Successfully deleted `{0}` message{1}", deletedAmt, GetActions.GetPlural(deletedAmt));
				var userResp = user != null ? String.Format(" from `{0}`", user.FormatUser()) : null;
				var chanResp = channel != null ? String.Format(" on `{0}`", channel.FormatChannel()) : null;
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.JoinNonNullStrings(" ", response, userResp, chanResp) + ".");
			}
		}

		[Group("modifyslowmode"), Alias("msm")]
		[Usage("[On|Off|Setup] <1 to 5> <1 to 30> <Role ...>")]
		[Summary("First arg is how many messages can be sent in a timeframe. Second arg is the timeframe. Third arg is guildwide; true means yes, false means no. " +
			"Fourth are the list of roles that are immune to slowmode.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(true)]
		public sealed class ModifySlowmode : MySavingModuleBase
		{
			[Command("on")]
			public async Task CommandOn()
			{
				await CommandRunner(true);
			}
			[Command("off")]
			public async Task CommandOff()
			{
				await CommandRunner(false);
			}
			[Command("setup")]
			public async Task CommandSetup(uint messages, uint interval, [Optional] params IRole[] immuneRoles)
			{
				await CommandRunner(messages, interval, immuneRoles);
			}

			private async Task CommandRunner(bool enable)
			{
				if (Context.GuildSettings.Slowmode == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("There must be a slowmode set up before one can be enabled or disabled."));
					return;
				}

				if (enable)
				{
					Context.GuildSettings.Slowmode.Enable();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled slowmode.\n{0}", Context.GuildSettings.Slowmode.ToString()));
				}
				else
				{
					Context.GuildSettings.Slowmode.Disable();
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled slowmode.");
				}
			}
			private async Task CommandRunner(uint messages, uint interval, IRole[] immuneRoles)
			{
				Context.GuildSettings.Slowmode = new Slowmode((int)messages, (int)interval, immuneRoles);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully setup slowmode.\n{0}", Context.GuildSettings.Slowmode.ToString()));
			}
		}
	}
	/*
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(Constants.ACTION_ERROR));
				return;
			}
			action = Actions.ClarifyFAWRType(action);

			if (action != FAWRType.Take_Nickname)
			{
				if (returnedArgs.ArgCount < 3)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(Constants.ARGUMENTS_ERROR));
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
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Cannot give the same role that is being gathered."));
						return;
					}
					break;
				}
				case FAWRType.Give_Nickname:
				{
					if (outputStr.Length > Constants.MAX_NICKNAME_LENGTH)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Nicknames cannot be longer than `{0}` charaters.", Constants.MAX_NICKNAME_LENGTH)));
						return;
					}
					else if (outputStr.Length < Constants.MIN_NICKNAME_LENGTH)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Nicknames cannot be less than `{0}` characters.", Constants.MIN_NICKNAME_LENGTH)));
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
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Unable to find any users with the input role that could be modified."));
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

			var msg = await MessageActions.SendChannelMessage(Context, String.Format("Attempted to edit `{0}` user{1}.", userCount, Actions.GetPlural(userCount))) as IUserMessage;
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
									await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.GiveRole(user, outputRole);
						}

						await MessageActions.SendChannelMessage(Context, String.Format("Successfully gave the role `{0}` to `{1}` users.", outputRole.FormatRole(), count));
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
									await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.TakeRole(user, outputRole);
						}

						await MessageActions.SendChannelMessage(Context, String.Format("Successfully took the role `{0}` from `{1}` users.", outputRole.FormatRole(), count));
						break;
					}
				}
				typing.Dispose();
				await msg.DeleteAsync();
			}).Forget();
		}
	}*/
}
