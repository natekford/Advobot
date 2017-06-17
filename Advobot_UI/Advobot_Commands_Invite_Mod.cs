using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Invite_Moderation")]
	class Advobot_Commands_Invite_Mod : ModuleBase
	{
		[Command("displayinvites")]
		[Alias("dinvs")]
		[Usage("")]
		[Summary("Gives a list of all the instant invites on the guild.")]
		[OtherRequirement(1U << (int)Precondition.User_Has_A_Perm)]
		[DefaultEnabled(true)]
		public async Task ListInstantInvites()
		{
			//Get the invites
			var invites = (await Context.Guild.GetInvitesAsync()).OrderBy(x => x.Uses).Reverse().ToList();

			//Make sure there are some invites
			if (!invites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no invites."));
				return;
			}

			//Format the description
			var description = "";
			var count = 1;
			var lengthForCount = invites.Count.ToString().Length;
			var lengthForCode = invites.Max(x => x.Code.Length);
			var lengthForUses = invites.Max(x => x.Uses).ToString().Length;
			invites.ForEach(x =>
			{
				var cnt = count++.ToString().PadLeft(lengthForCount, '0');
				var code = x.Code.PadRight(lengthForCode);
				var uses = x.Uses.ToString().PadRight(lengthForUses);
				description += String.Format("`{0}.` `{1}` `{2}` `{3}`\n", cnt, code, uses, x.Inviter.Username);
			});

			//Send a success message
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Instant Invite List", description));
		}

		[Command("createinvite")]
		[Alias("cinv")]
		[Usage("[Channel] <Time:1800|3600|21600|43200|86400> <Uses:1|5|10|25|50|100> <TempMem:True|False>")]
		[Summary("Creates an invite on the given channel. No time specifies to not expire. No uses has no usage limit. Temp membership means when the user goes offline they get kicked.")]
		[PermissionRequirement(1U << (int)GuildPermission.CreateInstantInvite)]
		[DefaultEnabled(true)]
		public async Task CreateInstantInvite([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 4), new[] { "time", "uses", "tempmem" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var chanStr = returnedArgs.Arguments[0];
			var timeStr = returnedArgs.GetSpecifiedArg("time");
			var usesStr = returnedArgs.GetSpecifiedArg("uses");
			var tempStr = returnedArgs.GetSpecifiedArg("tempmem");

			//Check validity of channel
			var returnedChannel = Actions.GetChannel(Context, new[] { ChannelCheck.Can_Modify_Permissions }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

			int? nullableTime = null;
			int[] validTimes = { 1800, 3600, 21600, 43200, 86400 };
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (int.TryParse(timeStr, out int time) && validTimes.Contains(time))
				{
					nullableTime = time;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time supplied."));
					return;
				}
			}

			int? nullableUsers = null;
			int[] validUsers = { 1, 5, 10, 25, 50, 100 };
			if (!String.IsNullOrWhiteSpace(usesStr))
			{
				if (int.TryParse(usesStr, out int users) && validUsers.Contains(users))
				{
					nullableUsers = users;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid uses supplied."));
					return;
				}
			}

			var tempMembership = false;
			if (!String.IsNullOrWhiteSpace(tempStr))
			{
				if (!bool.TryParse(tempStr, out tempMembership))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid temp membership boolean supplied."));
					return;
				}
			}

			var inv = await channel.CreateInviteAsync(nullableTime, nullableUsers, tempMembership);

			//Format the response message
			var timeOutputStr = "";
			if (nullableTime == null)
			{
				timeOutputStr = "It will last until manually revoked.";
			}
			else
			{
				timeOutputStr = String.Format("It will last for this amount of time: `{0}`.", timeStr);
			}
			var usersOutputStr = "";
			if (nullableUsers == null)
			{
				usersOutputStr = "It has no usage limit.";
			}
			else
			{
				usersOutputStr = String.Format("It will last for this amount of uses: `{0}`.", usesStr);
			}
			var tempOutputStr = "";
			if (tempMembership)
			{
				tempOutputStr = "Users will be kicked when they go offline unless they get a role.";
			}

			await Actions.SendChannelMessage(Context, String.Format("Here is your invite for `{0}`: {1}\n{2}\n{3}\n{4}", channel.FormatChannel(), inv.Url, timeOutputStr, usersOutputStr, tempOutputStr));
		}

		[Command("deleteinvite")]
		[Alias("dinv")]
		[Usage("[Invite Code]")]
		[Summary("Deletes the invite with the given code.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task DeleteInstantInvite([Remainder] string input)
		{
			//Get the input
			var invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == input);
			if (invite == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That invite doesn't exist."));
				return;
			}

			//Delete the invite and send a success message
			await invite.DeleteAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the invite `{0}`.", invite.Code));
		}

		[Command("deletemultipleinvites")]
		[Alias("dminv")]
		[Usage("User:User|Role:Role|Uses:Number|Expires:True|False]")]
		[Summary("Deletes all invites satisfying the given condition of either user, creation channel, uses, or expiry time.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task DeleteMultipleInvites([Remainder] string input)
		{
			//Get the guild's invites
			var invites = (await Context.Guild.GetInvitesAsync()).ToList();
			if (!invites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no invites."));
				return;
			}

			//Get the given variable out
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 4), new[] { "user", "channel", "uses", "expires" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.GetSpecifiedArg("user");
			var chanStr = returnedArgs.GetSpecifiedArg("channel");
			var usesStr = returnedArgs.GetSpecifiedArg("uses");
			var exprStr = returnedArgs.GetSpecifiedArg("expires");

			if (String.IsNullOrWhiteSpace(userStr) && new[] { userStr, chanStr, usesStr, exprStr }.CaseInsEverythingSame())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("At least one of the arguments must be specified."));
				return;
			}

			//User
			if (!String.IsNullOrWhiteSpace(userStr))
			{
				if (ulong.TryParse(userStr, out ulong userID))
				{
					invites = invites.Where(x => x.Inviter.Id == userID).ToList();
				}
				else if (MentionUtils.TryParseUser(userStr, out userID))
				{
					invites = invites.Where(x => x.Inviter.Id == userID).ToList();
				}
				else
				{
					invites = invites.Where(x => Actions.CaseInsEquals(x.Inviter.Username, userStr)).ToList();
				}
			}
			//Channel
			if (!String.IsNullOrWhiteSpace(chanStr))
			{
				var returnedChannel = Actions.GetChannel(Context, new[] { ChannelCheck.Can_Modify_Permissions }, true, chanStr);
				if (returnedChannel.Reason == FailureReason.Not_Failure)
				{
					invites = invites.Where(x => x.ChannelId == returnedChannel.Object.Id).ToList();
				}
			}
			//Uses
			if (!String.IsNullOrWhiteSpace(usesStr))
			{
				if (int.TryParse(usesStr, out int uses))
				{
					invites = invites.Where(x => x.Uses == uses).ToList();
				}
			}
			//Expiry
			if (!String.IsNullOrWhiteSpace(exprStr))
			{
				if (bool.TryParse(exprStr, out bool expires))
				{
					if (expires)
					{
						invites = invites.Where(x => x.MaxAge != null).ToList();
					}
					else
					{
						invites = invites.Where(x => x.MaxAge == null).ToList();
					}
				}
			}

			if (!invites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invites satisfied the given conditions."));
				return;
			}

			Task.Run(async () =>
			{
				var typing = Context.Channel.EnterTypingState();
				await invites.ForEachAsync(async x => await x.DeleteAsync());
				typing.Dispose();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` instant invites on this guild.", invites.Count));
			}).Forget();
		}
	}
}
