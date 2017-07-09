using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Advobot
{
	namespace InviteModeration
	{
		[Usage("")]
		[Summary("Gives a list of all the instant invites on the guild.")]
		[OtherRequirement(Precondition.UserHasAPerm)]
		[DefaultEnabled(true)]
		public class DisplayInvites : ModuleBase<MyCommandContext>
		{
			[Command("displayinvites")]
			[Alias("dinvs")]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var invites = (await Context.Guild.GetInvitesAsync()).OrderByDescending(x => x.Uses);
				if (!invites.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no invites."));
					return;
				}

				var lenForCode = invites.Max(x => x.Code.Length);
				var lenForUses = invites.Max(x => x.Uses).ToString().Length;
				var desc = String.Join("\n", invites.FormatNumberedList("`{0}` `{1}` `{2}`", x => x.Code.PadRight(lenForCode), x => x.Uses.ToString().PadRight(lenForUses), x => x.Inviter.FormatUser()));
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Instant Invite List", desc));
			}
		}

		[Usage("[Channel] <1800|3600|21600|43200|86400> <1|5|10|25|50|100> <True|False>")]
		[Summary("Creates an invite on the given channel. No time specifies to not expire. No uses has no usage limit. Temp membership means when the user goes offline they get kicked.")]
		[PermissionRequirement(1U << (int)GuildPermission.CreateInstantInvite)]
		[DefaultEnabled(true)]
		public class CreateInvite : ModuleBase<MyCommandContext>
		{
			private static readonly int[] validTimes = { 1800, 3600, 21600, 43200, 86400 };
			private static readonly int[] validUses = { 1, 5, 10, 25, 50, 100 };

			[Command("createinvite")]
			[Alias("cinv")]
			public async Task Command(IGuildChannel channel, [Optional] int time, [Optional] int uses, [Optional] bool tempMem)
			{
				await CommandRunner(channel, time, uses, tempMem);
			}

			private async Task CommandRunner(IGuildChannel channel, int? nullableTime = 86400, int? nullableUses = null, bool tempMem = false)
			{
				var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanCreateInstantInvite }, channel);
				if (returnedChannel.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}
				else if (nullableTime.HasValue && !validTimes.Contains(nullableTime.Value))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid time supplied, must be one of the following: `{0]`.", String.Join("`, `", validTimes))));
					return;
				}
				else if (nullableUses.HasValue && !validUses.Contains(nullableUses.Value))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid uses supplied, must be one of the following: `{0}`", String.Join("`, `", validUses))));
					return;
				}

				var inv = await channel.CreateInviteAsync(nullableTime, nullableUses, tempMem);

				var timeOutputStr = nullableTime.HasValue ? String.Format("It will last for this amount of time: `{0}`.", nullableTime) : "It will last until manually revoked.";
				var usersOutputStr = nullableUses.HasValue ? String.Format("It will last for this amount of uses: `{0}`.", nullableUses) : "It has no usage limit.";
				var tempOutputStr = tempMem ? "Users will be kicked when they go offline unless they get a role." : "Users will not be kicked when they go offline and do not have a role.";
				await Actions.SendChannelMessage(Context, String.Format("Here is your invite for `{0}`: {1}",
					channel.FormatChannel(), 
					Actions.JoinNonNullStrings("\n", inv.Url, timeOutputStr, usersOutputStr, tempOutputStr)));
			}
		}

		[Usage("[Invite Code]")]
		[Summary("Deletes the invite with the given code.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public class DeleteInvite : ModuleBase<MyCommandContext>
		{
			[Command("deleteinvite")]
			[Alias("dinv")]
			public async Task Command([OverrideTypeReader(typeof(IInviteTypeReader))] IInvite invite)
			{
				await CommandRunner(invite);
			}

			private async Task CommandRunner(IInvite invite)
			{
				await invite.DeleteAsync();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the invite `{0}`.", invite.Code));
			}
		}
	}
	[Name("InviteModeration")]
	public class Advobot_Commands_Invite_Mod : ModuleBase
	{

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
			if (returnedArgs.Reason != FailureReason.NotFailure)
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
				var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions }, true, chanStr);
				if (returnedChannel.Reason == FailureReason.NotFailure)
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

			Actions.DontWaitForResultOfBigUnimportantFunction(Context.Channel, async () =>
			{
				await invites.ForEachAsync(async x => await x.DeleteAsync());
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` instant invites on this guild.", invites.Count));
			});
		}
	}
}
