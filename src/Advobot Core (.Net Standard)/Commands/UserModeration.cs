using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Punishments;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.UserModeration
{
	[Group(nameof(Mute)), TopLevelShortAlias(typeof(Mute))]
	[Summary("Prevents a user from typing and speaking in the guild. Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles, GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
	public sealed class Mute : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [Optional] uint time, [Optional, Remainder] string reason)
		{
			var muteRole = await RoleActions.GetMuteRole(Context, Context.GuildSettings);
			if (user.RoleIds.Contains(muteRole.Id))
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UnrolemuteAsync(user, muteRole, new ModerationReason(Context.User, reason));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, remover.ToString());
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.RoleMuteAsync(user, muteRole, new ModerationReason(Context.User, reason));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, giver.ToString());
		}
	}

	[Group(nameof(VoiceMute)), TopLevelShortAlias(typeof(VoiceMute))]
	[Summary("Prevents a user from speaking. Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.MuteMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class VoiceMute : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [Optional] uint time)
		{
			if (user.IsMuted)
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UnvoicemuteAsync(user, new ModerationReason(Context.User, null));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, remover.ToString());
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.VoiceMuteAsync(user, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, giver.ToString());
		}
	}

	[Group(nameof(Deafen)), TopLevelShortAlias(typeof(Deafen))]
	[Summary("Prevents a user from hearing. Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.DeafenMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Deafen : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [Optional] uint time)
		{
			if (user.IsDeafened)
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UndeafenAsync(user, new ModerationReason(Context.User, null));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, remover.ToString());
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.DeafenAsync(user, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, giver.ToString());
		}
	}

	[Group(nameof(MoveUser)), TopLevelShortAlias(typeof(MoveUser))]
	[Summary("Moves the user to the given voice channel.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUser : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel channel)
		{
			if (user.VoiceChannel == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("User is not in a voice channel."));
				return;
			}
			else if (user.VoiceChannel == channel)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("User is already in that channel."));
				return;
			}

			await UserActions.MoveUser(user, channel, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully moved `{user.FormatUser()}` to `{channel.FormatChannel()}`.");
		}
	}

	//TODO: put in cancel tokens for the commands that user bypass strings in case people need to cancel
	[Group(nameof(MoveUsers)), TopLevelShortAlias(typeof(MoveUsers))]
	[Summary("Moves all users from one channel to another. Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUsers : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel outputChannel,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = await inputChannel.GetUsersAsync().Flatten();
			await new MultiUserAction(Context, users, bypass).MoveManyUsers(outputChannel, new ModerationReason(Context.User, null));
		}
	}

	[Group(nameof(PruneUsers)), TopLevelShortAlias(typeof(PruneUsers))]
	[Summary("Removes users who have no roles and have not been seen in the given amount of days. If the optional argument is not typed exactly, then the bot will only give a number of how many people will be kicked.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class PruneUsers : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyNumber(1, 7, 30)] uint days, [Optional, OverrideTypeReader(typeof(PruneTypeReader))] bool simulate)
		{
			var amt = await GuildActions.PruneUsers(Context.Guild, (int)days, !simulate, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"`{amt}` members{(!simulate ? " would" : "")} have been pruned with a prune period of `{days}` days.");
		}
	}

	[Group(nameof(SoftBan)), TopLevelShortAlias(typeof(SoftBan))]
	[Summary("Bans then unbans a user, which removes all recent messages from them.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class SoftBan : AdvobotModuleBase
	{
		[Command, Priority(1)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, Remainder] string reason)
		{
			await CommandRunner(user.Id, reason);
		}
		[Command, Priority(0)]
		public async Task Command(ulong userId, [Optional, Remainder] string reason)
		{
			await CommandRunner(userId, reason);
		}

		private async Task CommandRunner(ulong userId, string reason)
		{
			var giver = new PunishmentGiver(0, Context.Timers);
			await giver.SoftbanAsync(Context.Guild, userId, new ModerationReason(Context.User, reason));
			await MessageActions.SendMessage(Context.Channel, giver.ToString());
		}
	}

	[Group(nameof(Ban)), TopLevelShortAlias(typeof(Ban))]
	[Summary("Bans the user from the guild. Time specifies how long and is in minutes.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Ban : AdvobotModuleBase
	{
		[Command]
		public async Task Command([OverrideTypeReader(typeof(UserIdTypeReader))] ulong user, [Optional] uint time, [Optional, Remainder] string reason)
		{
			await CommandRunner(user, time, reason);
		}
		[Command]
		public async Task Command([OverrideTypeReader(typeof(UserIdTypeReader))] ulong user, [Optional, Remainder] string reason)
		{
			await CommandRunner(user, 0, reason);
		}

		private async Task CommandRunner(ulong userId, uint time, string reason)
		{
			if ((await Context.Guild.GetBansAsync()).Select(x => x.User.Id).Contains(userId))
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("That user is already banned."));
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.BanAsync(Context.Guild, userId, new ModerationReason(Context.User, reason), 1);
			await MessageActions.SendMessage(Context.Channel, giver.ToString());
		}
	}

	[Group(nameof(Unban)), TopLevelShortAlias(typeof(Unban))]
	[Summary("Unbans the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Unban : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IBan ban, [Optional, Remainder] string reason)
		{
			var remover = new PunishmentRemover(Context.Timers);
			await remover.UnbanAsync(Context.Guild, ban.User.Id, new ModerationReason(Context.User, reason));
			await MessageActions.SendMessage(Context.Channel, remover.ToString());
		}
	}

	[Group(nameof(GetBanReason)), TopLevelShortAlias(typeof(GetBanReason))]
	[Summary("Lists the given reason for the ban.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers}, null)]
	[DefaultEnabled(true)]
	public sealed class GetBanReason : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IBan ban)
		{
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Ban reason for " + ban.User.FormatUser(), ban.Reason));
		}
	}

	[Group(nameof(Kick)), TopLevelShortAlias(typeof(Kick))]
	[Summary("Kicks the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.KickMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Kick : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, Remainder] string reason)
		{
			var giver = new PunishmentGiver(0, Context.Timers);
			await giver.KickAsync(user, new ModerationReason(Context.User, reason));
			await MessageActions.SendMessage(Context.Channel, giver.ToString());
		}
	}

	[Group(nameof(DisplayCurrentBanList)), TopLevelShortAlias(typeof(DisplayCurrentBanList))]
	[Summary("Displays all the bans on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayCurrentBanList : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var bans = await Context.Guild.GetBansAsync();
			if (!bans.Any())
			{
				await MessageActions.SendMessage(Context.Channel, "This guild has no bans.");
				return;
			}

			var desc = bans.FormatNumberedList("`{0}`", x => x.User.FormatUser());
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Current Bans", desc));
		}
	}

	[Group(nameof(RemoveMessages)), TopLevelShortAlias(typeof(RemoveMessages))]
	[Summary("Removes the provided number of messages from either the user, the channel, both, or, if neither is input, the current channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
	public sealed class RemoveMessages : AdvobotModuleBase
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
				var owner = await Context.Guild.GetOwnerAsync();
				await owner.SendMessageAsync($"`{Context.User.FormatUser()}` is trying to delete messages from a log channel: `{channel.FormatChannel()}`.");
				return;
			}

			//If not the context channel then get the first message in that channel
			var messageToStartAt = Context.Message.Channel.Id == channel.Id
				? Context.Message
				: (await channel.GetMessagesAsync(1).Flatten()).FirstOrDefault();

			//If there is a non null user then delete messages specifically from that user
			var deletedAmt = user == null
				? await MessageActions.RemoveMessages(channel, messageToStartAt, requestCount, new ModerationReason(Context.User, null))
				: await MessageActions.RemoveMessagesFromUser(channel, messageToStartAt, requestCount, user, new ModerationReason(Context.User, null));

			//If the context channel isn't the targetted channel then delete the start message and increase by one to account for it not being targetted.
			if (Context.Message.Channel.Id != channel.Id)
			{
				await MessageActions.DeleteMessage(messageToStartAt);
				deletedAmt++;
			}

			var response = $"Successfully deleted `{deletedAmt}` message{GetActions.GetPlural(deletedAmt)}";
			var userResp = user != null ? $" from `{user.FormatUser()}`" : null;
			var chanResp = channel != null ? $" on `{channel.FormatChannel()}`" : null;
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.JoinNonNullStrings(" ", response, userResp, chanResp) + ".");
		}
	}

	[Group(nameof(ModifySlowmode)), TopLevelShortAlias(typeof(ModifySlowmode))]
	[Summary("First arg is how many messages can be sent in a timeframe. Second arg is the timeframe. Third arg is guildwide; true means yes, false means no. " +
		"Fourth are the list of roles that are immune to slowmode.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class ModifySlowmode : SavingModuleBase
	{
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create([VerifyNumber(true, 1, 5)] uint messages, [VerifyNumber(true, 1, 30)] uint interval, [Optional] params IRole[] immuneRoles)
		{
			Context.GuildSettings.Slowmode = new Slowmode((int)messages, (int)interval, immuneRoles);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully setup slowmode.\n{Context.GuildSettings.Slowmode.ToString()}");
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable()
		{
			if (Context.GuildSettings.Slowmode == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There must be a slowmode set up before one can be enabled or disabled."));
				return;
			}

			Context.GuildSettings.Slowmode.Enable();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully enabled slowmode.\n{Context.GuildSettings.Slowmode.ToString()}");
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable()
		{
			if (Context.GuildSettings.Slowmode == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There must be a slowmode set up before one can be enabled or disabled."));
				return;
			}

			Context.GuildSettings.Slowmode.Disable();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully disabled slowmode.");
		}
	}

	[Group(nameof(ForAllWithRole)), TopLevelShortAlias(typeof(ForAllWithRole))]
	[Summary("All actions but `TakeNickame` require the output role/nickname. Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class ForAllWithRole : AdvobotModuleBase
	{
		[Command(nameof(GiveRole)), ShortAlias(nameof(GiveRole))]
		public async Task GiveRole(IRole targetRole, 
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole givenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			if (targetRole.Id == givenRole.Id)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("Cannot give the role being gathered."));
				return;
			}

			var users = (await UserActions.GetUsersTheBotAndUserCanEdit(Context)).Where(x => x.RoleIds.Contains(targetRole.Id));
			await new MultiUserAction(Context, users, bypass).GiveRoleToManyUsers(givenRole, new ModerationReason(Context.User, null));
		}
		[Command(nameof(TakeRole)), ShortAlias(nameof(TakeRole))]
		public async Task TakeRole(IRole targetRole,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole takenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await UserActions.GetUsersTheBotAndUserCanEdit(Context)).Where(x => x.RoleIds.Contains(targetRole.Id));
			await new MultiUserAction(Context, users, bypass).TakeRoleFromManyUsers(takenRole, new ModerationReason(Context.User, null));
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
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR($"Nicknames cannot be longer than `{0}` charaters.", Constants.MAX_NICKNAME_LENGTH)));
						return;
					}
					else if (outputStr.Length < Constants.MIN_NICKNAME_LENGTH)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR($"Nicknames cannot be less than `{0}` characters.", Constants.MIN_NICKNAME_LENGTH)));
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

			var msg = await MessageActions.SendMessage(Context, $"Attempted to edit `{0}` user{1}.", userCount, Actions.GetPlural(userCount))) as IUserMessage;
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
								await msg.ModifyAsync(x => x.Content = $"ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
								if (Context.Guild.GetRole(outputRole.Id) == null)
								{
									await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.GiveRole(user, outputRole);
						}

						await MessageActions.SendMessage(Context, $"Successfully gave the role `{0}` to `{1}` users.", outputRole.FormatRole(), count));
						break;
					}
					case FAWRType.Take_Role:
					{
						foreach (var user in users)
						{
							++count;
							if (count % 10 == 0)
							{
								await msg.ModifyAsync(x => x.Content = $"ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
								if (Context.Guild.GetRole(outputRole.Id) == null)
								{
									await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.TakeRole(user, outputRole);
						}

						await MessageActions.SendMessage(Context, $"Successfully took the role `{0}` from `{1}` users.", outputRole.FormatRole(), count));
						break;
					}
				}
				typing.Dispose();
				await msg.DeleteAsync();
			}).Forget();
		}
	}*/
}
