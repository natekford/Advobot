using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	namespace ChannelModeration
	{
		[Usage("[Text|Voice] [Name]")]
		[Summary("Adds a channel to the guild of the given type with the given name. Text channel names cannot contain any spaces.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public class CreateChannel : ModuleBase<MyCommandContext>
		{
			[Command("createchannel")]
			[Alias("cch")]
			public async Task Command(ChannelType channelType, [Remainder] string name)
			{
				await CommandRunner(channelType, name);
			}

			private async Task CommandRunner(ChannelType channelType, string name)
			{
				if (name.Length > Constants.MAX_CHANNEL_NAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be more than `{0}` characters.", Constants.MAX_CHANNEL_NAME_LENGTH)));
					return;
				}
				else if (name.Length < Constants.MIN_CHANNEL_NAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be less than `{0}` characters.", Constants.MIN_CHANNEL_NAME_LENGTH)));
					return;
				}

				IGuildChannel channel;
				switch (channelType)
				{
					case ChannelType.Text:
					{
						if (name.Contains(' '))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces are allowed in a text channel name."));
							return;
						}

						channel = await Context.Guild.CreateTextChannelAsync(name);
						break;
					}
					case ChannelType.Voice:
					{
						channel = await Context.Guild.CreateVoiceChannelAsync(name);
						break;
					}
					default:
					{
						return;
					}
				}

				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created `{0}`.", channel.FormatChannel()));
			}
		}

		[Usage("[Channel]")]
		[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		[DefaultEnabled(true)]
		public class SoftDeleteChannel : ModuleBase<MyCommandContext>
		{
			[Command("softdeletechannel")]
			[Alias("sdch")]
			public async Task Command([VerifyObject(ObjectVerification.CanBeManaged)] ITextChannel channel)
			{
				await CommandRunner(channel);
			}

			public async Task CommandRunner(ITextChannel channel)
			{
				if (channel.Id == Context.Guild.DefaultChannelId)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to softdelete the base channel."));
					return;
				}

				foreach (var overwrite in channel.PermissionOverwrites)
				{
					dynamic obj;
					switch (overwrite.TargetType)
					{
						case PermissionTarget.Role:
						{
							obj = Context.Guild.GetRole(overwrite.TargetId);
							break;
						}
						case PermissionTarget.User:
						{
							obj = await Context.Guild.GetUserAsync(overwrite.TargetId);
							break;
						}
						default:
						{
							continue;
						}
					}

					var allowBits = Actions.RemoveChannelPermissions(Actions.GetOverwriteAllowBits(channel, obj), ChannelPermission.ReadMessages);
					var denyBits = Actions.AddChannelPermissions(Actions.GetOverwriteDenyBits(channel, obj), ChannelPermission.ReadMessages);
					await Actions.ModifyOverwrite(channel, obj, allowBits, denyBits);
				}

				//Double check the everyone role has the correct perms
				if (!channel.PermissionOverwrites.Any(x => x.TargetId == Context.Guild.EveryoneRole.Id))
				{
					await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessages: PermValue.Deny));
				}

				//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
				await Actions.ModifyChannelPosition(channel, (await Context.Guild.GetTextChannelsAsync()).Max(x => x.Position));
				await Actions.SendChannelMessage(Context, "Successfully softdeleted this channel. Only admins and the owner will be able to read anything in this channel.");
			}
		}

		[Usage("[Channel]")]
		[Summary("Deletes the channel.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public class DeleteChannel : ModuleBase<MyCommandContext>
		{
			[Command("deletechannel")]
			[Alias("dch")]
			public async Task Command()
			{

			}

			public async Task CommandRunner(IGuildChannel channel)
			{
				//Check if tried on the base channel
				if (channel.Id == Context.Guild.DefaultChannelId)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to delete the base channel."));
					return;
				}

				await channel.DeleteAsync();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}`.", channel.FormatChannel()));
			}
		}
		/*
		public class Temp : ModuleBase<MyCommandContext>
		{
			public async Task Command()
			{

			}

			public async Task CommandRunner()
			{

			}
		}

		public class Temp : ModuleBase<MyCommandContext>
		{
			public async Task Command()
			{

			}

			public async Task CommandRunner()
			{

			}
		}

		public class Temp : ModuleBase<MyCommandContext>
		{
			public async Task Command()
			{

			}

			public async Task CommandRunner()
			{

			}
		}

		public class Temp : ModuleBase<MyCommandContext>
		{
			public async Task Command()
			{

			}

			public async Task CommandRunner()
			{

			}
		}

		public class Temp : ModuleBase<MyCommandContext>
		{
			public async Task Command()
			{

			}

			public async Task CommandRunner()
			{

			}
		}

		public class Temp : ModuleBase<MyCommandContext>
		{
			public async Task Command()
			{

			}

			public async Task CommandRunner()
			{

			}
		}

		public class Temp : ModuleBase<MyCommandContext>
		{
			public async Task Command()
			{

			}

			public async Task CommandRunner()
			{

			}
		}*/
	}
	[Name("ChannelModeration")]
	public class Advobot_Commands_Channel_Mod : ModuleBase
	{

		[Command("changechannelposition")]
		[Alias("cchpo")]
		[Usage("[Channel] <Number>")]
		[Summary("If only the channel is input the channel's position will be listed. Else, gives the channel the given position. Position zero is the top most position.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task ChannelPosition([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var chanStr = returnedArgs.Arguments[0];
			var posStr = returnedArgs.Arguments[1];

			//Get the channel
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeReordered }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

			//Argument count checking
			if (String.IsNullOrWhiteSpace(posStr))
			{
				await Actions.SendChannelMessage(Context, String.Format("`{0}` has a position of `{1}`.", channel.FormatChannel(), channel.Position));
				return;
			}

			if (!int.TryParse(posStr, out int position))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				return;
			}
			else if (position < 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a channel to a position lower than zero."));
				return;
			}

			//Modify the channel's position
			await Actions.ModifyChannelPosition(channel, position);
			await Actions.SendChannelMessage(Context, String.Format("Successfully moved `{0}` to position `{1}`.", channel.FormatChannel(), position));
		}

		[Command("displaychannelpositions")]
		[Alias("dchp")]
		[Usage("[Text|Voice]")]
		[Summary("Lists the positions of each text or voice channel on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task ListChannelPositions([Remainder] string input)
		{
			//Check if valid type
			var text = Actions.CaseInsEquals(input, Constants.TEXT_TYPE);
			var voice = Actions.CaseInsEquals(input, Constants.VOICE_TYPE);
			if (!voice && !text)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid channel type."));
				return;
			}

			var channels = (text ? (await Context.Guild.GetTextChannelsAsync()).Cast<IGuildChannel>() : (await Context.Guild.GetVoiceChannelsAsync()).Cast<IGuildChannel>()).OrderBy(x => x.Position).ToList();
			var title = String.Format("{0} Channel Positions", text ? "Text" : "Voice");
			var desc = String.Join("\n", channels.Select(x => String.Format("`{0}.` `{1}`", x.Position.ToString("00"), x.Name)));
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, desc));
		}

		[Command("changechannelperms")]
		[Alias("cchpe")]
		[Usage("[Show|Allow|Inherit|Deny] [Channel] [User|Role] [Permission/...]")]
		[Summary("Permissions must be separated by a `/`. Type `" + Constants.BOT_PREFIX + "chp [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel]` to see all permissions on a channel. " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		[DefaultEnabled(true)]
		public async Task ChannelPermissions([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 4));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var chanStr = returnedArgs.Arguments[1];
			var targStr = returnedArgs.Arguments[2];
			var permStr = returnedArgs.Arguments[3];

			//Get the action
			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Show, ActionType.Allow, ActionType.Inherit, ActionType.Deny });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			//If only show, take that as a person wanting to see the permission types
			if (returnedArgs.ArgCount == 1)
			{
				if (action == ActionType.Show)
				{
					//Embed showing the channel permission types
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Channel Permission Types", String.Format("`{0}`", String.Join("`, `", Variables.ChannelPermissions.Select(x => x.Name)))));
					return;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
			}

			//Get the channel
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

			//Get the role or user
			IGuildUser user = null;
			IRole role = null;
			user = Actions.GetGuildUser(Context, new[] { ObjectVerification.None }, true, targStr).Object;
			if (user == null)
			{
				role = Actions.GetRole(Context, new[] { ObjectVerification.None }, true, targStr).Object;
				if (role == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid target supplied."));
					return;
				}
			}

			var permissions = new List<string>();
			switch (action)
			{
				case ActionType.Show:
				{
					//Say the overwrites on a channel
					if (returnedArgs.ArgCount == 2)
					{
						var roleOverwrites = new List<string>();
						var userOverwrites = new List<string>();
						await channel.PermissionOverwrites.ToList().ForEachAsync(async x =>
						{
							if (x.TargetType == PermissionTarget.Role)
							{
								roleOverwrites.Add(Context.Guild.GetRole(x.TargetId).Name);
							}
							else
							{
								userOverwrites.Add((await Context.Guild.GetUserAsync(x.TargetId)).Username);
							}
						});

						//Make an embed saying the overwrites
						var embed = Actions.MakeNewEmbed(channel.FormatChannel());
						Actions.AddField(embed, "Role", String.Format("`{0}`", roleOverwrites.Any() ? String.Join("`, `", roleOverwrites) : "NONE"));
						Actions.AddField(embed, "User", String.Format("`{0}`", userOverwrites.Any() ? String.Join("`, `", userOverwrites) : "NONE"));
						await Actions.SendEmbedMessage(Context.Channel, embed);
						return;
					}

					//Check to see if there are any overwrites
					if (!channel.PermissionOverwrites.Any())
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Unable to show permissions for `{0}` on `{1}`.", targStr, channel.FormatChannel())));
						return;
					}

					//Say the permissions of the overwrite
					await channel.PermissionOverwrites.ToList().ForEachAsync(async overwrite =>
					{
						if (role != null && overwrite.TargetId.Equals(role.Id))
						{
							var perms = Actions.GetFilteredChannelOverwritePermissions(overwrite, channel);
							var maxLen = perms.Keys.Max(x => x.Length);

							//Embed showing the perm overwrites on a role
							var formattedPerms = String.Join("\n", perms.Select(x => String.Format("{0} {1}", x.Key.PadRight(maxLen), x.Value)));
							var description = String.Format("**Channel:** `{0}`\n**Role:** `{1}`\n```{2}```", channel.FormatChannel(), role.FormatRole(), formattedPerms);
							var embed = Actions.MakeNewEmbed("Channel Permissions On Role", description);
							await Actions.SendEmbedMessage(Context.Channel, embed);
						}
						else if (user != null && overwrite.TargetId.Equals(user.Id))
						{
							var perms = Actions.GetFilteredChannelOverwritePermissions(overwrite, channel);
							var maxLen = perms.Keys.Max(x => x.Length);

							//Embed showing the perm overwrites on a user
							var formattedPerms = String.Join("\n", perms.Select(x => String.Format("{0} {1}", x.Key.PadRight(maxLen), x.Value)));
							var description = String.Format("**Channel:** `{0}`\n**User:** `{1}`\n```{2}```", channel.FormatChannel(), user.FormatUser(), formattedPerms);
							var embed = Actions.MakeNewEmbed("Channel Permissions On User", description);
							await Actions.SendEmbedMessage(Context.Channel, embed);
						}
					});
					return;
				}
				case ActionType.Allow:
				case ActionType.Inherit:
				case ActionType.Deny:
				{
					permissions = permStr.Split('/').ToList();
					break;
				}
			}

			if (user != null)
			{
				var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.CanBeEdited }, user);
				if (returnedUser.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedUser);
					return;
				}
			}
			else if (role != null)
			{
				var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited }, role);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
			}

			//Check if valid permissions
			var validPerms = permissions.Intersect(Variables.ChannelPermissions.Select(x => x.Name).ToList(), StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				var invalidPerms = permissions.Where(x => !validPerms.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList();
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid permission{0} supplied: `{1}`.",
					Actions.GetPlural(invalidPerms.Count),
					String.Join("`, `", invalidPerms))));
				return;
			}

			//Remove any attempt to change readmessages on the base channel because nothing can change that
			if (channel.Id == Context.Guild.DefaultChannelId)
			{
				permissions.RemoveAll(x => Actions.CaseInsIndexOf(x, "readmessages"));
			}

			//Get the permissions
			uint changeValue = 0;
			uint allowBits = 0;
			uint denyBits = 0;
			if (role != null)
			{
				if (channel.GetPermissionOverwrite(role).HasValue)
				{
					allowBits = (uint)channel.GetPermissionOverwrite(role).Value.AllowValue;
					denyBits = (uint)channel.GetPermissionOverwrite(role).Value.DenyValue;
				}
			}
			else
			{
				if (channel.GetPermissionOverwrite(user).HasValue)
				{
					allowBits = (uint)channel.GetPermissionOverwrite(user).Value.AllowValue;
					denyBits = (uint)channel.GetPermissionOverwrite(user).Value.DenyValue;
				}
			}

			//Changing the bit values
			permissions.ToList().ForEach(x => changeValue = Actions.GetBit(Context, x, changeValue));
			switch (action)
			{
				case ActionType.Allow:
				{
					allowBits |= changeValue;
					denyBits &= ~changeValue;
					actionStr = "allowed";
					break;
				}
				case ActionType.Inherit:
				{
					allowBits &= ~changeValue;
					denyBits &= ~changeValue;
					actionStr = "inherited";
					break;
				}
				case ActionType.Deny:
				{
					allowBits &= ~changeValue;
					denyBits |= changeValue;
					actionStr = "denied";
					break;
				}
			}

			//Change the permissions
			var roleNameOrUsername = "";
			if (role != null)
			{
				await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allowBits, denyBits));
				roleNameOrUsername = role.FormatRole();
			}
			else
			{
				await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allowBits, denyBits));
				roleNameOrUsername = user.FormatUser();
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` for `{2}` on `{3}`",
				actionStr, String.Join("`, `", permissions), roleNameOrUsername, channel.FormatChannel()));
		}

		[Command("copychannelperms")]
		[Alias("cochp")]
		[Usage("[Channel] [Channel] [User|Role|All]")]
		[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		[DefaultEnabled(true)]
		public async Task CopyChannelPermissions([Remainder] string input)
		{
			//Get arguments
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(3, 3));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var firstChanStr = returnedArgs.Arguments[0];
			var secondChanStr = returnedArgs.Arguments[1];
			var targetStr = returnedArgs.Arguments[2];

			//Separating the channels
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions }, false, firstChanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var inputChannel = returnedChannel.Object;

			//See if the user can see and thus edit that channel
			var returnedChannelTwo = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions }, false, secondChanStr);
			if (returnedChannelTwo.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannelTwo);
				return;
			}
			var outputChannel = returnedChannelTwo.Object;

			//Make sure channels are the same type
			if (inputChannel.GetType() != outputChannel.GetType())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Channels must be the same type."));
				return;
			}

			//Copy the selected target
			if (Actions.CaseInsEquals(targetStr, "all"))
			{
				targetStr = "ALL";
				await inputChannel.PermissionOverwrites.ToList().ForEachAsync(async permissionOverwrite =>
				{
					if (permissionOverwrite.TargetType == PermissionTarget.Role)
					{
						var role = Context.Guild.GetRole(permissionOverwrite.TargetId);
						await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(permissionOverwrite.Permissions.AllowValue, permissionOverwrite.Permissions.DenyValue));
					}
					else
					{
						var user = await Context.Guild.GetUserAsync(permissionOverwrite.TargetId);
						await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(permissionOverwrite.Permissions.AllowValue, permissionOverwrite.Permissions.DenyValue));
					}
				});
			}
			else
			{
				if (Context.Message.MentionedUserIds.Any())
				{
					var evaluatedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.CanBeEdited }, true, targetStr);
					if (evaluatedUser.Reason != FailureReason.NotFailure)
					{
						await Actions.HandleObjectGettingErrors(Context, evaluatedUser);
						return;
					}
					else
					{
						var user = evaluatedUser.Object;
						var currOver = inputChannel.GetPermissionOverwrite(user);
						if (!currOver.HasValue)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A permission overwrite for that user does not exist to copy over."));
							return;
						}

						targetStr = user.Username;
						await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(currOver.Value.AllowValue, currOver.Value.DenyValue));
					}
				}
				else
				{
					var evaluatedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited }, true, targetStr);
					if (evaluatedRole.Reason != FailureReason.NotFailure)
					{
						await Actions.HandleObjectGettingErrors(Context, evaluatedRole);
						return;
					}
					else
					{
						var role = evaluatedRole.Object;
						var currOver = inputChannel.GetPermissionOverwrite(role);
						if (!currOver.HasValue)
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A permission overwrite for that role does not exist to copy over."));
							return;
						}

						targetStr = role.Name;
						await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(currOver.Value.AllowValue, currOver.Value.DenyValue));
					}
				}
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` from `{1}` to `{2}`",
				targetStr,
				inputChannel.FormatChannel(),
				outputChannel.FormatChannel()));
		}

		[Command("clearchannelperms")]
		[Alias("clchp")]
		[Usage("[Channel]")]
		[Summary("Removes all permissions set on a channel.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		[DefaultEnabled(true)]
		public async Task ClearChannelPermissions([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanModifyPermissions }, true, input);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

			//Check if channel has permissions to clear
			if (!channel.PermissionOverwrites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Channel has no permissions to clear."));
				return;
			}

			//Remove all the permission overwrites
			await channel.PermissionOverwrites.ToList().ForEachAsync(async x =>
			{
				if (x.TargetType == PermissionTarget.Role)
				{
					await channel.RemovePermissionOverwriteAsync(Context.Guild.GetRole(x.TargetId));
				}
				else
				{
					await channel.RemovePermissionOverwriteAsync(await Context.Guild.GetUserAsync(x.TargetId));
				}
			});
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all channel permissions from `{0}`.", channel.FormatChannel()));
		}

		[Command("changechannelnsfw")]
		[Alias("cchnsfw")]
		[Usage("[Channel]")]
		[Summary("Toggles the NSFW option on a channel.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task ChannelNSFW([Remainder] string input)
		{
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeManaged, ObjectVerification.IsText }, true, input);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

			var name = channel.Name;
			var response = "";
			if (channel.IsNsfw)
			{
				name = name.Substring("nsfw-".Length);
				response = String.Format("Successfully removed the NSFW prefix from `{0}`.", channel.FormatChannel());
			}
			else
			{
				name = ("nsfw-" + name);
				name = name.Substring(0, Math.Min(name.Length, 32));
				response = String.Format("Successfully added the NSFW prefix to `{0}`.", channel.FormatChannel());
			}

			await channel.ModifyAsync(x => x.Name = name);
			await Actions.MakeAndDeleteSecondaryMessage(Context, response);
		}

		[Command("changechannelname")]
		[Alias("cchn")]
		[Usage("[Channel|Position:Number Type:Text|Voice] [\"New Name\"]")]
		[Summary("Changes the name of the channel. This is *extremely* useful for when multiple channels have the same name but you want to edit things.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task ChangeChannelName([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3), new[] { "position", "type" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var chanStr = returnedArgs.Arguments[0];
			var nameStr = returnedArgs.Arguments[1];
			var positionStr = returnedArgs.GetSpecifiedArg("position");
			var typeStr = returnedArgs.GetSpecifiedArg("type");

			IGuildChannel channel = null;
			if (!String.IsNullOrWhiteSpace(positionStr))
			{
				if (!int.TryParse(positionStr, out int position))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position supplied."));
					return;
				}

				var channels = new List<IGuildChannel>();
				if (Actions.CaseInsEquals(typeStr, Constants.TEXT_TYPE))
				{
					channels = (await Context.Guild.GetTextChannelsAsync()).Where(x => x.Position == position).Cast<IGuildChannel>().ToList();
				}
				else if (Actions.CaseInsEquals(typeStr, Constants.VOICE_TYPE))
				{
					channels = (await Context.Guild.GetVoiceChannelsAsync()).Where(x => x.Position == position).Cast<IGuildChannel>().ToList();
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid channel type supplied."));
					return;
				}

				//Check the count now
				if (!channels.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No {0} channel has the position `{1}`.", typeStr.ToLower(), position)));
					return;
				}
				else if (channels.Count == 1)
				{
					var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeManaged }, channel);
					if (returnedChannel.Reason != FailureReason.NotFailure)
					{
						await Actions.HandleObjectGettingErrors(Context, returnedChannel);
						return;
					}
					channel = returnedChannel.Object;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` {1} channels have the position `{2}`.", channels.Count, typeStr.ToLower(), position));
					return;
				}
			}
			else
			{
				var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeManaged }, true, chanStr);
				if (returnedChannel.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}
				channel = returnedChannel.Object;
			}

			//Checking if valid name
			if (Actions.CaseInsEquals(Actions.GetChannelType(channel), Constants.TEXT_TYPE) && nameStr.Contains(' '))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces are allowed in a text channel name."));
				return;
			}
			else if (nameStr.Length > Constants.MAX_CHANNEL_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be more than `{0}` characters.", Constants.MAX_CHANNEL_NAME_LENGTH)));
				return;
			}
			else if (nameStr.Length < Constants.MIN_CHANNEL_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be less than `{0}` characters.", Constants.MIN_CHANNEL_NAME_LENGTH)));
				return;
			}

			var previousName = channel.Name;
			await channel.ModifyAsync(x => x.Name = nameStr);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed channel `{0}` to `{1}`.", previousName, nameStr));
		}

		[Command("changechanneltopic")]
		[Alias("ccht")]
		[Usage("[Channel] [New Topic]")]
		[Summary("Changes the subtext of a channel to whatever is input.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task ChangeChannelTopic([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var chanStr = returnedArgs.Arguments[0];
			var newTopic = returnedArgs.Arguments[1];

			//See if valid length
			if (newTopic.Length > Constants.MAX_TITLE_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Topics cannot be longer than `{0}` characters in length.", Constants.MAX_TITLE_LENGTH)));
				return;
			}

			//Test if valid channel
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeManaged, ObjectVerification.IsText }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var tc = returnedChannel.Object as ITextChannel;
			if (tc == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only text channels can have their topic set."));
				return;
			}

			//See what current topic is
			var currentTopic = tc.Topic;
			if (String.IsNullOrWhiteSpace(currentTopic))
			{
				currentTopic = "NOTHING";
			}
			if (String.IsNullOrWhiteSpace(newTopic))
			{
				newTopic = "NOTHING";
			}

			await tc.ModifyAsync(x => x.Topic = newTopic);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.", tc.FormatChannel(), currentTopic, newTopic));
		}

		[Command("changechannellimit")]
		[Alias("cchl")]
		[Usage("[Channel] [New Limit]")]
		[Summary("Changes the limit to how many users can be in a voice channel. The limit ranges from 0 (no limit) to 99.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task ChangeChannelLimit([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var chanStr = returnedArgs.Arguments[0];
			var limtStr = returnedArgs.Arguments[1];

			//Check if valid number
			if (!int.TryParse(limtStr, out int limit))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}
			else if (limit > 99 || limit < 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Number must be between 0 and 99 inclusive."));
				return;
			}

			//Check if valid channel that the user can edit
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeManaged, ObjectVerification.IsVoice }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var vc = returnedChannel.Object as IVoiceChannel;
			if (vc == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command will not work on a text channel."));
				return;
			}

			//Change it and send a success message
			await vc.ModifyAsync(x => x.UserLimit = limit);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}`.", vc.FormatChannel(), limit));
		}

		[Command("changechannelbitrate")]
		[Alias("cchbr")]
		[Usage("[Channel] [8 to 96]")]
		[Summary("Changes the bit rate (in kbps) on the selected channel to the given value. The default value is 64. The bitrate can go up to 128 on a partnered guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task ChangeChannelBitRate([Remainder] string input)
		{
			//Split the input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var chanStr = returnedArgs.Arguments[0];
			var bitrateStr = returnedArgs.Arguments[1];

			//Check if valid number
			if (!int.TryParse(bitrateStr, out int bitRate))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}
			//Check if number between 8 and 96
			else if (bitRate < Constants.MIN_BITRATE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bitrate must be above or equal to `{0}`.", Constants.MIN_BITRATE)));
				return;
			}
			else if (!Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && bitRate > Constants.MAX_BITRATE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bitrate must be below or equal to `{0}`.", Constants.MAX_BITRATE)));
				return;
			}
			else if (bitRate > Constants.VIP_BITRATE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bitrate must be below or equal to `{0}`.", Constants.VIP_BITRATE)));
				return;
			}

			//Check if valid channel that the user can edit
			var returnedChannel = Actions.GetChannel(Context, new[] { ObjectVerification.CanBeManaged, ObjectVerification.IsVoice }, true, chanStr);
			if (returnedChannel.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var vc = returnedChannel.Object as IVoiceChannel;
			if (vc == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command will not work on a text channel."));
				return;
			}

			//Change it and send a success message
			await vc.ModifyAsync(x => x.Bitrate = bitRate * 1000);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}kbps`.", vc.FormatChannel(), bitRate));
		}
	}
}
