using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	//Channel Moderation commands are commands that affect the channels in a guild
	[Name("Channel Moderation")]
	public class Advobot_Commands_Channel_Mod : ModuleBase
	{
		[Command("channelcreate")]
		[Alias("chc")]
		[Usage("[Name] [Text|Voice]")]
		[Summary("Adds a channel to the guild of the given type with the given name. The name CANNOT contain any spaces: use underscores or dashes instead.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task CreateChannel([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(' ');
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test for args
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test for name validity
			var name = inputArray[0];
			if (name.Contains(' '))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."));
				return;
			}
			else if (name.Length > Constants.CHANNEL_NAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be more than {0} characters.", Constants.CHANNEL_NAME_MAX_LENGTH)));
				return;
			}
			else if (name.Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be less than {0} characters.", Constants.CHANNEL_NAME_MIN_LENGTH)));
				return;
			}
			else if (Actions.CaseInsEquals(name, Variables.Bot_Channel) && await Actions.GetDuplicateBotChan(Context.Guild))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Please don't try to make a second bot channel. All that does is confuse the bot.");
				return;
			}

			//Test for text
			IGuildChannel channel;
			var type = inputArray[1];
			if (Actions.CaseInsEquals(type, Constants.TEXT_TYPE))
			{
				channel = await Context.Guild.CreateTextChannelAsync(inputArray[0]);
			}
			//Test for voice
			else if (Actions.CaseInsEquals(type, Constants.VOICE_TYPE))
			{
				channel = await Context.Guild.CreateVoiceChannelAsync(inputArray[0]);
			}
			//Give an error if not text/voice
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid channel type."));
				return;
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created `{0}`.", Actions.FormatChannel(channel)));
		}

		[Command("channelsoftdelete")]
		[Alias("chsd")]
		[Usage("[#Channel]")]
		[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task SoftDeleteChannel([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			var channel = await Actions.GetChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//See if not attempted on a text channel
			if (Actions.GetChannelType(channel) != Constants.TEXT_TYPE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Softdelete only works on text channels inside a guild."));
				return;
			}
			//Check if tried on the base channel
			else if (channel.Id == Context.Guild.DefaultChannelId)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to softdelete the base channel."));
				return;
			}

			//Make it so only admins/the owner can read the channel
			await channel.PermissionOverwrites.ToList().ForEachAsync(async overwrite =>
			{
				if (overwrite.TargetType == PermissionTarget.Role)
				{
					var role = Context.Guild.GetRole(overwrite.TargetId);
					var allowBits = (uint)channel.GetPermissionOverwrite(role).Value.AllowValue & ~(1U << (int)ChannelPermission.ReadMessages);
					var denyBits = (uint)channel.GetPermissionOverwrite(role).Value.DenyValue | (1U << (int)ChannelPermission.ReadMessages);
					await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allowBits, denyBits));
				}
				else
				{
					var user = await Context.Guild.GetUserAsync(overwrite.TargetId);
					var allowBits = (uint)channel.GetPermissionOverwrite(user).Value.AllowValue & ~(1U << (int)ChannelPermission.ReadMessages);
					var denyBits = (uint)channel.GetPermissionOverwrite(user).Value.DenyValue | (1U << (int)ChannelPermission.ReadMessages);
					await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allowBits, denyBits));
				}
			});

			//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
			var highestPosition = 0;
			(await Context.Guild.GetTextChannelsAsync()).ToList().ForEach(x => highestPosition = Math.Max(x.Position, highestPosition));

			await channel.ModifyAsync(x => x.Position = highestPosition);
			await Actions.SendChannelMessage(channel as ITextChannel, "Successfully softdeleted this channel. Only admins and the owner will be able to read anything on this channel.");
		}

		[Command("channeldelete")]
		[Alias("chd")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Deletes the channel.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task DeleteChannel([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			var channel = await Actions.GetChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//Check if tried on the base channel
			if (channel.Id == Context.Guild.DefaultChannelId)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to delete the base channels."));
				return;
			}

			await channel.DeleteAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}`.", Actions.FormatChannel(channel)));
		}

		[Command("channelposition")]
		[Alias("chpos")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS + " [New Position]")]
		[Summary("Gives the channel the given position. Position one is the top most position and counting starts at zero. This command is extremely buggy!")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChannelPosition([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { ' ' }, 2);

			//Get the channel
			var channel = await Actions.GetChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			//Argument count checking
			if (inputArray.Length != 2)
			{
				await Actions.SendChannelMessage(Context, String.Format("`{0}` has a position of `{1}`.", Actions.FormatChannel(channel), channel.Position));
				return;
			}

			//Get the position as an int
			var position = 0;
			if (!int.TryParse(input.Substring(input.LastIndexOf(' ')), out position))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				return;
			}

			//Check the min against the current position
			if (position < 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a channel to a position lower than or equal to zero."));
				return;
			}

			//Placeholder list
			var channelAndPositions = new List<IGuildChannel>();
			//Grab either the text or voice channels
			if (Actions.GetChannelType(channel) == Constants.TEXT_TYPE)
			{
				//Grab all text channels that aren't the targeted one and sort the list by position
				channelAndPositions = (await Context.Guild.GetTextChannelsAsync()).Where(x => x != channel).Select(x => x as IGuildChannel).OrderBy(x => x.Position).ToList();
			}
			else
			{
				//Grab all the voice channels that aren't the targeted one and sort the list by position
				channelAndPositions = (await Context.Guild.GetVoiceChannelsAsync()).Where(x => x != channel).Select(x => x as IGuildChannel).OrderBy(x => x.Position).ToList();
			}
			//Add in the targeted channel with the given position
			channelAndPositions.Insert(Math.Min(channelAndPositions.Count(), position), channel);
			//Mass modify the channels with the list having the correct positions
			await Context.Guild.ModifyChannelsAsync(channelAndPositions.Select(x => new BulkGuildChannelProperties(x.Id, channelAndPositions.IndexOf(x))) as IEnumerable<BulkGuildChannelProperties>);

			//Send a message stating what position the channel was sent to
			await Actions.SendChannelMessage(Context, String.Format("Successfully moved `{0}` to position `{1}`.", Actions.FormatChannel(channel), channel.Position));
		}

		[Command("channelpositions")]
		[Alias("chposs")]
		[Usage("[Text|Voice]")]
		[Summary("Lists the positions of each text or voice channel on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task ListChannelPositions([Remainder] string input)
		{
			//Check if valid type
			if (!(Actions.CaseInsEquals(input, Constants.VOICE_TYPE) || Actions.CaseInsEquals(input, Constants.TEXT_TYPE)))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid type."));
				return;
			}

			//Initialize the string
			var title = "";
			var description = "";
			if (input.Equals(Constants.VOICE_TYPE))
			{
				title = "Voice Channels Positions";

				//Put the positions into the string
				(await Context.Guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
				{
					description += "`" + x.Position.ToString("00") + ".` " + x.Name + "\n";
				});
			}
			else
			{
				title = "Text Channels Positions";

				//Put the positions into the string
				(await Context.Guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
				{
					description += "`" + x.Position.ToString("00") + ".` " + x.Name + "\n";
				});
			}

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, description));
		}

		[Command("channelperms")]
		[Alias("chp")]
		[Usage("[Show|Allow|Inherit|Deny] " + Constants.OPTIONAL_CHANNEL_INSTRUCTIONS + " <Role|User> <Permission/...>")]
		[Summary("Type `" + Constants.BOT_PREFIX + "chp [Show]` to see the available permissions. Permissions must be separated by a `/`! " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel]` to see all permissions on a channel. " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task ChannelPermissions([Remainder] string input)
		{
			//Set the variables
			var permissions = new List<string>();
			IGuildChannel channel;
			IGuildUser user;
			IRole role;

			//Split the input
			var inputArray = input.Trim().Split(new char[] { ' ' }, 2);
			if (inputArray.Length > 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Determine what action it is
			var actionName = inputArray[0];
			if (Actions.CaseInsEquals(actionName, "show"))
			{
				//If only show, take that as a person wanting to see the permission types
				if (inputArray.Length == 1)
				{
					//Embed showing the channel permission types
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Channel Permission Types", String.Join("\n", Variables.ChannelPermissions.Select(x => x.Name))));
					return;
				}

				//Check for valid channel
				inputArray = inputArray[1].Split(new char[] { ' ' }, 2);
				//See if the user can see and thus edit that channel
				channel = await Actions.GetChannelEditAbility(Context, inputArray[0]);
				if (channel == null)
					return;

				//Say the overwrites on a channel
				if (inputArray.Length == 1)
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
					var embed = Actions.MakeNewEmbed(null, Actions.FormatChannel(channel));
					Actions.AddField(embed, "Role", roleOverwrites.Any() ? String.Join("\n", roleOverwrites) : "None");
					Actions.AddField(embed, "User", userOverwrites.Any() ? String.Join("\n", userOverwrites) : "None");
					await Actions.SendEmbedMessage(Context.Channel, embed);
					return;
				}

				//Check if valid role or user
				role = await Actions.GetRole(Context, inputArray[1]);
				user = await Actions.GetUser(Context.Guild, inputArray[1]);
				if (role == null && user == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."));
					return;
				}

				//Check to see if there are any overwrites
				if (!channel.PermissionOverwrites.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Unable to show permissions for `{0}` on `{1}`.", inputArray[1], Actions.FormatChannel(channel))));
					return;
				}

				//Say the permissions of the overwrite
				await channel.PermissionOverwrites.ToList().ForEachAsync(async overwrite =>
				{
					if (role != null && overwrite.TargetId.Equals(role.Id))
					{
						//Embed showing the perm overwrites on a role
						var embed = Actions.MakeNewEmbed(title: String.Format("{0} on {1}", role.Name, Actions.FormatChannel(channel)));
						Actions.AddField(embed, "Permission", String.Join("\n", Actions.GetPerms(overwrite, channel).Keys));
						Actions.AddField(embed, "Value", String.Join("\n", Actions.GetPerms(overwrite, channel).Values));
						await Actions.SendEmbedMessage(Context.Channel, embed);
					}
					else if (user != null && overwrite.TargetId.Equals(user.Id))
					{
						//Embed showing the perm overwrites on a user
						var embed = Actions.MakeNewEmbed(title: String.Format("{0}#{1} on {2}", user.Username, user.Discriminator, Actions.FormatChannel(channel)));
						Actions.AddField(embed, "Permission", String.Join("\n", Actions.GetPerms(overwrite, channel).Keys));
						Actions.AddField(embed, "Value", String.Join("\n", Actions.GetPerms(overwrite, channel).Values));
						await Actions.SendEmbedMessage(Context.Channel, embed);
					}
				});
			}
			else if (new string[] { "allow", "deny", "inherit" }.Contains(actionName, StringComparer.OrdinalIgnoreCase))
			{
				inputArray = inputArray[1].Split(new char[] { ' ' }, 2);

				//Check if valid number of arguments
				if (inputArray.Length == 1)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}

				//See if the user can see and thus edit that channel
				channel = await Actions.GetChannelEditAbility(Context, inputArray[0]);
				if (channel == null)
					return;

				//Check if valid perms and potential role/user
				var potentialRoleOrUser = "";
				if (Actions.GetStringAndPermissions(inputArray[1], out potentialRoleOrUser, out permissions))
				{
					//See if valid role or user
					role = Actions.GetRole(Context.Guild, potentialRoleOrUser);
					user = await Actions.GetUser(Context.Guild, potentialRoleOrUser);
					if (role == null && user == null)
					{
						//Give error if no user or role that's valid
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No permissions supplied."));
					return;
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the generic permissions
			var genericPerms = Variables.ChannelPermissions.Select(x => x.Name).ToList();
			//Check if valid permissions
			var validPerms = permissions.Intersect(genericPerms, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				var invalidPerms = new List<string>();
				permissions.ForEach(x =>
				{
					if (!validPerms.Contains(x, StringComparer.OrdinalIgnoreCase))
					{
						invalidPerms.Add(x);
					}
				});
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid {0} supplied: `{1}`.",
					invalidPerms.Count == 1 ? "permission" : "permissions",
					String.Join("`, `", invalidPerms))), 7500);
				return;
			}

			//Remove any attempt to change readmessages on the base channel because nothing can change that
			if (channel.Id == Context.Guild.DefaultChannelId && permissions.Contains("readmessages"))
			{
				permissions.RemoveAll(x => x.StartsWith("readmessages"));
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
			await permissions.ToList().ForEachAsync(async x => changeValue = await Actions.GetBit(Context, x, changeValue));
			if (Actions.CaseInsEquals(actionName, "allow"))
			{
				allowBits |= changeValue;
				denyBits &= ~changeValue;
				actionName = "allowed";
			}
			else if (Actions.CaseInsEquals(actionName, "inherit"))
			{
				allowBits &= ~changeValue;
				denyBits &= ~changeValue;
				actionName = "inherited";
			}
			else
			{
				allowBits &= ~changeValue;
				denyBits |= changeValue;
				actionName = "denied";
			}

			//Change the permissions
			var roleNameOrUsername = "";
			if (role != null)
			{
				await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allowBits, denyBits));
				roleNameOrUsername = Actions.FormatRole(role);
			}
			else
			{
				await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allowBits, denyBits));
				roleNameOrUsername = Actions.FormatUser(user);
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` for `{2}` on `{3}`",
				actionName, String.Join("`, `", permissions), roleNameOrUsername, Actions.FormatChannel(channel)), 7500);
		}

		[Command("channelpermscopy")]
		[Alias("chpc")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS + " " + Constants.CHANNEL_INSTRUCTIONS + " [Role|User|All]")]
		[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task CopyChannelPermissions([Remainder] string input)
		{
			//Get arguments
			var inputArray = input.Split(new char[] { ' ' }, 3);

			//Check if the correct number of args
			if (inputArray.Length < 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Separating the channels
			var inputChannel = await Actions.GetChannel(Context, inputArray[0]);
			if (inputChannel == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}

			//See if the user can see and thus edit that channel
			var outputChannel = await Actions.GetChannelEditAbility(Context, inputArray[1]);
			if (outputChannel == null)
				return;

			//Trim the third arg
			var target = inputArray[2].Trim();

			//Copy the selected target
			if (Actions.CaseInsEquals(target, "all"))
			{
				target = "ALL";
				await inputChannel.PermissionOverwrites.ToList().ForEachAsync(async permissionOverwrite =>
				{
					if (permissionOverwrite.TargetType == PermissionTarget.Role)
					{
						var role = Context.Guild.GetRole(permissionOverwrite.TargetId);
						await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(inputChannel.GetPermissionOverwrite(role).Value.AllowValue,
							inputChannel.GetPermissionOverwrite(role).Value.DenyValue));
					}
					else
					{
						var user = await Context.Guild.GetUserAsync(permissionOverwrite.TargetId);
						await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(inputChannel.GetPermissionOverwrite(user).Value.AllowValue,
							inputChannel.GetPermissionOverwrite(user).Value.DenyValue));
					}
				});
			}
			else
			{
				var role = await Actions.GetRole(Context, target);
				if (role != null)
				{
					target = role.Name;
					await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(inputChannel.GetPermissionOverwrite(role).Value.AllowValue,
						inputChannel.GetPermissionOverwrite(role).Value.DenyValue));
				}
				else
				{
					var user = await Actions.GetUser(Context.Guild, target);
					if (user != null)
					{
						target = user.Username;
						await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(inputChannel.GetPermissionOverwrite(user).Value.AllowValue,
							inputChannel.GetPermissionOverwrite(user).Value.DenyValue));
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid role/user or all input."));
						return;
					}
				}
			}

			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` from `{1}` to `{2}`",
				target, Actions.FormatChannel(inputChannel), Actions.FormatChannel(outputChannel)), 7500);
		}

		[Command("channelpermsclear")]
		[Alias("chpcl")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Removes all permissions set on a channel.")]
		[PermissionRequirement(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task ClearChannelPermissions([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			var channel = await Actions.GetChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//Check if channel has permissions to clear
			if (channel.PermissionOverwrites.Count < 1)
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
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all channel permissions from `{0}`.", Actions.FormatChannel(channel)));
		}

		[Command("channelname")]
		[Alias("chn")]
		[Usage("[#Channel|[Channel|Position{x}/Text|Voice]] [New Name]")]
		[Summary("Changes the name of the channel. This is *extremely* useful for when multiple channels have the same name but you want to edit things.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelName([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Checking if valid name
			var name = inputArray[1];
			if (name.Contains(' '))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."));
				return;
			}
			else if (name.Length > Constants.CHANNEL_NAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be more than {0} characters.", Constants.CHANNEL_NAME_MAX_LENGTH)));
				return;
			}
			else if (name.Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be less than {0} characters.", Constants.CHANNEL_NAME_MIN_LENGTH)));
				return;
			}
			else if (Actions.CaseInsEquals(name, Variables.Bot_Channel) && await Actions.GetDuplicateBotChan(Context.Guild))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Please don't try to rename a channel to the bot channel if one alreay exists. All that does is confuse the bot.");
				return;
			}

			//See if it's a position trying to be gotten instead
			var channelInput = inputArray[0];
			var channel = await Actions.GetChannelEditAbility(Context, channelInput);
			if (channel == null)
			{
				if (Actions.CaseInsIndexOf(channelInput, "position{"))
				{
					//Get the position
					int position;
					var leftBracePos = channelInput.IndexOf('{');
					var rightBracePos = channelInput.IndexOf('}');
					if (!int.TryParse(channelInput.Substring(leftBracePos, rightBracePos), out position))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position supplied."));
						return;
					}

					//Split the input
					var splitInputArray = channelInput.Split(new char[] { '/' }, 2);
					//Give the channeltype
					var channelType = splitInputArray[1];

					//Initialize the channels list
					var textChannels = new List<ITextChannel>();
					var voiceChannels = new List<IVoiceChannel>();

					if (Actions.CaseInsEquals(channelType, Constants.TEXT_TYPE))
					{
						textChannels = (await Context.Guild.GetTextChannelsAsync()).Where(x => x.Position == position).ToList();
					}
					else if (Actions.CaseInsEquals(channelType, Constants.VOICE_TYPE))
					{
						voiceChannels = (await Context.Guild.GetVoiceChannelsAsync()).Where(x => x.Position == position).ToList();
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid channel type."));
						return;
					}

					//Check the count now
					if (textChannels.Count == 0 && voiceChannels.Count == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No {0} channel has a position of `{1}`.", channelType, position)));
						return;
					}
					else if (textChannels.Count == 1 || voiceChannels.Count == 1)
					{
						//Get the channel
						var chan = textChannels.Count == 1 ? textChannels.First() as IGuildChannel : voiceChannels.First() as IGuildChannel;
						channel = await Actions.GetChannelEditAbility(chan, Context.User as IGuildUser);
					}
					else
					{
						//Get the count
						var count = textChannels.Any() ? textChannels.Count : voiceChannels.Count;
						await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` {1} channels have the position `{2}`.", count, channelType, position));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
					return;
				}
			}

			var previousName = channel.Name;
			await channel.ModifyAsync(x => x.Name = inputArray[1]);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed channel `{0}` to `{1}`.", previousName, inputArray[1]));
		}

		[Command("channeltopic")]
		[Alias("cht")]
		[Usage("[#Channel] [New Topic]")]
		[Summary("Changes the subtext of a channel to whatever is input.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelTopic([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid channel
			var channel = await Actions.GetChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;
			//See if not a text channel
			else if (Actions.GetChannelType(channel) != Constants.TEXT_TYPE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only text channels can have their topic set."));
				return;
			}

			//See if valid length
			var newTopic = inputArray[1];
			if (newTopic.Length > Constants.TOPIC_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Topics cannot be longer than 1024 characters in length."));
				return;
			}

			//See what current topic is
			var currentTopic = (channel as ITextChannel).Topic;
			if (String.IsNullOrWhiteSpace(currentTopic))
			{
				currentTopic = "NOTHING";
			}

			await (channel as ITextChannel).ModifyAsync(x => x.Topic = newTopic);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.",
				Actions.FormatChannel(channel), currentTopic, String.IsNullOrWhiteSpace(newTopic) ? "NOTHING" : newTopic));
		}

		[Command("channellimit")]
		[Alias("chl")]
		[Usage("[Channel Name] [New Limit]")]
		[Summary("Changes the limit to how many users can be in a voice channel. The limit ranges from 0 (no limit) to 99.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelLimit([Remainder] string input)
		{
			//Separate the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var channelName = inputArray[0];
			var newLimit = inputArray[1];

			//Check if valid channel that the user can edit
			var channel = await Actions.GetChannelEditAbility(Context, channelName + "/voice");
			if (channel == null)
				return;
			//See if not a voice channel
			else if (Actions.GetChannelType(channel) != Constants.VOICE_TYPE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only a voice channel can have a limit set on it."));
				return;
			}

			//Check if valid number
			var limit = 0;
			if (!int.TryParse(newLimit, out limit))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}

			//Check if number between 0 and 99
			if (limit > 99 || limit < 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Number must be between 0 and 99 inclusive."));
				return;
			}

			//Change it and send a success message
			await (channel as IVoiceChannel).ModifyAsync(x => x.UserLimit = limit);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}`.", Actions.FormatChannel(channel), limit));
		}

		[Command("channelbitrate")]
		[Alias("chbr")]
		[Usage("[Channel Name] [8 to 96]")]
		[Summary("Changes the bit rate (in kbps) on the selected channel to the given value. The default value is 64.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelBitRate([Remainder] string input)
		{
			//Separate the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var channelName = inputArray[0];
			var newBitRate = inputArray[1];

			//Check if valid channel that the user can edit
			var channel = await Actions.GetChannelEditAbility(Context, channelName + "/voice");
			if (channel == null)
				return;
			//See if not a voice channel
			else if (Actions.GetChannelType(channel) != Constants.VOICE_TYPE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only a voice channel can have its bit rate changed."));
				return;
			}

			//Check if valid number
			var bitRate = 0;
			if (!int.TryParse(newBitRate, out bitRate))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}

			//Check if number between 8 and 96
			if (bitRate < Constants.MIN_BITRATE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bitrate must be above or equal to {0}.", Constants.MIN_BITRATE)));
				return;
			}
			else if (!Context.Guild.Features.Contains(Constants.VIP_REGIONS, StringComparer.OrdinalIgnoreCase) && bitRate > Constants.MAX_BITRATE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bitrate must be below or equal to {0}.", Constants.MAX_BITRATE)));
				return;
			}
			else if (bitRate > Constants.VIP_BITRATE)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bitrate must be below or equal to {0}.", Constants.VIP_BITRATE)));
				return;
			}

			//Change it and send a success message
			await (channel as IVoiceChannel).ModifyAsync(x => x.Bitrate = bitRate * 1000);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}kbps`.", Actions.FormatChannel(channel), bitRate));
		}
	}
}
