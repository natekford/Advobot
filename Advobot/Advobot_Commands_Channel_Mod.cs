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
	class Advobot_Commands_Channel_Mod : ModuleBase
	{
		[Command("channelcreate")]
		[Alias("chc")]
		[Usage("channelcreate [Name] [Text|Voice]")]
		[Summary("Adds a channel to the guild of the given type with the given name. The name CANNOT contain any spaces: use underscores or dashes instead.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task CreateChannel([Remainder] string input)
		{
			var values = input.Split(' ');
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var type = values[1];

			//Test for args
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test for name validity
			if (values[0].Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."));
				return;
			}
			else if (values[0].Length > Constants.CHANNEL_NAME_MAX_LENGTH || values[0].Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name must be between 2 and 100 characters long."));
				return;
			}
			else if (values[0].Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase) && await Actions.getDuplicateBotChan(Context.Guild))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Please don't try to make a second bot channel. All that does is confuse the bot.");
				return;
			}

			//Test for text
			if (type.Equals(ChannelType.Text.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				await Context.Guild.CreateTextChannelAsync(values[0]);
			}
			//Test for voice
			else if (type.Equals(ChannelType.Voice.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				await Context.Guild.CreateVoiceChannelAsync(values[0]);
			}
			//Give an error if not text/voice
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid channel type."));
				return;
			}

			await Actions.makeAndDeleteSecondaryMessage(Context,
				String.Format("Successfully created `{0} ({1})`.", values[0], char.ToUpper(type[0]) + type.Substring(1)));
		}

		[Command("channelsoftdelete")]
		[Alias("chsd")]
		[Usage("channelsoftdelete [#Channel]")]
		[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task SoftDeleteChannel([Remainder] string input)
		{
			//See if the input name has spaces
			if (input.Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}

			//See if the user can see and thus edit that channel
			var channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//See if not attempted on a text channel
			if (Actions.getChannelType(channel) != Constants.TEXT_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Softdelete only works on text channels inside a guild."));
				return;
			}
			//Check if tried on the base channel
			else if (channel.Id == Context.Guild.DefaultChannelId)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to softdelete the base channel."));
				return;
			}

			//Make it so only admins/the owner can read the channel
			foreach (var overwrite in channel.PermissionOverwrites)
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
			}

			//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
			var highestPosition = 0;
			foreach (IGuildChannel channelOnGuild in await Context.Guild.GetChannelsAsync())
			{
				if (channelOnGuild.Position > highestPosition)
				{
					highestPosition = channelOnGuild.Position;
				}
			}

			await channel.ModifyAsync(x => x.Position = highestPosition);
			await Actions.sendChannelMessage(channel as ITextChannel,
				"Successfully softdeleted this channel. Only admins and the owner will be able to read anything on this channel.");
		}

		[Command("channeldelete")]
		[Alias("chd")]
		[Usage("channeldelete " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Deletes the channel.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task DeleteChannel([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			var channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//Check if tried on the base channel
			if (channel.Id == Context.Guild.DefaultChannelId)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to delete the base channels."));
				return;
			}

			await channel.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0} ({1})`.", channel.Name, Actions.getChannelType(channel)));
		}

		[Command("channelposition")]
		[Alias("chpos")]
		[Usage("channelposition " + Constants.CHANNEL_INSTRUCTIONS + " [New Position]")]
		[Summary("Gives the channel the given position. Position one is the top most position and counting starts at zero. This command is extremely buggy!")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChannelPosition([Remainder] string input)
		{
			var values = input.Split(new char[] { ' ' }, 2);

			//Get the channel
			var channel = await Actions.getChannelEditAbility(Context, values[0]);
			if (channel == null)
				return;

			//Argument count checking
			if (values.Count() != 2)
			{
				await Actions.sendChannelMessage(Context, String.Format("The `{0} ({1})` channel has a position of `{2}`.",
					channel.Name, Actions.getChannelType(channel), channel.Position));
				return;
			}

			//Get the position as an int
			var position = 0;
			if (!int.TryParse(input.Substring(input.LastIndexOf(' ')), out position))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				return;
			}

			//Check the min against the current position
			if (position < 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a channel to a position lower than or equal to zero."));
				return;
			}

			//Initialize a new list of IGuildChannel
			var channelAndPositions = new List<IGuildChannel>();
			//Grab either the text or voice channels
			if (Actions.getChannelType(channel) == Constants.TEXT_TYPE)
			{
				//Grab all text channels that aren't the targeted one
				(await Context.Guild.GetTextChannelsAsync()).Where(x => x != channel).ToList().ForEach(x => channelAndPositions.Add(x));
			}
			else
			{
				//Grab all the voice channels that aren't the targeted one
				(await Context.Guild.GetVoiceChannelsAsync()).Where(x => x != channel).ToList().ForEach(x => channelAndPositions.Add(x));
			}
			//Sort the list by position
			channelAndPositions = channelAndPositions.OrderBy(x => x.Position).ToList();
			//Add in the targetted channel with the given position
			channelAndPositions.Insert(Math.Min(channelAndPositions.Count(), position), channel);

			//Initialize a new list of BulkGuildChannelProperties
			var listOfBulk = new List<BulkGuildChannelProperties>();
			//Add the channel's IDs and positions into it
			channelAndPositions.ForEach(cap => listOfBulk.Add(new BulkGuildChannelProperties(cap.Id, channelAndPositions.IndexOf(cap))));
			//Mass modify the channels with the list having the correct positions as an IEnum
			await Context.Guild.ModifyChannelsAsync(listOfBulk as IEnumerable<BulkGuildChannelProperties>);

			//Send a message stating what position the channel was sent to
			await Actions.sendChannelMessage(Context, String.Format("Successfully moved `{0} ({1})` to position `{2}`.",
				channel.Name, Actions.getChannelType(channel), channelAndPositions.IndexOf(channel)));
		}

		[Command("channelpostitions")]
		[Alias("chposs")]
		[Usage("channelpostitions [Text|Voice]")]
		[Summary("Lists the positions of each text or voice channel on the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ListChannelPositions([Remainder] string input)
		{
			//Check if valid type
			if (!input.Equals(Constants.VOICE_TYPE) && !input.Equals(Constants.TEXT_TYPE))
				return;

			//Initialize the string
			var title = "";
			var description = "";
			if (input.Equals(Constants.VOICE_TYPE))
			{
				title = "Voice Channels Positions";

				//Put the positions into the string
				var list = (await Context.Guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList();
				foreach (var channel in list)
				{
					description += "`" + channel.Position.ToString("00") + ".` " + channel.Name + "\n";
				}
			}
			else
			{
				title = "Text Channels Positions";

				//Put the positions into the string
				var list = (await Context.Guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList();
				foreach (var channel in list)
				{
					description += "`" + channel.Position.ToString("00") + ".` " + channel.Name + "\n";
				}
			}

			//Check the length to see if the message can be sent
			if (description.Length > Constants.SHORT_LENGTH_CHECK)
			{
				description = Actions.uploadToHastebin(Actions.replaceMessageCharacters(description));
			}

			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(title, description));
		}

		[Command("channelperms")]
		[Alias("chp")]
		[Usage("channelperms [Show|Allow|Inherit|Deny] " + Constants.OPTIONAL_CHANNEL_INSTRUCTIONS + " <Role|User> <Permission/...>")]
		[Summary("Type `" + Constants.BOT_PREFIX + "chp [Show]` to see the available permissions. Permissions must be separated by a `/`! " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel]` to see all permissions on a channel. " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task ChannelPermissions([Remainder] string input)
		{
			//Set the variables
			var permissions = new List<string>();
			IGuildChannel channel = null;
			IGuildUser user = null;
			IRole role = null;

			//Split the input
			var values = input.ToLower().Trim().Split(new char[] { ' ' }, 2);
			if (values.Length == 0)
				return;
			var actionName = values[0];

			if (actionName.Equals("show"))
			{
				//If only show, take that as a person wanting to see the permission types
				if (values.Length == 1)
				{
					//Embed showing the channel permission types
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Channel Permission Types", String.Join("\n", Variables.ChannelPermissions.Select(x => x.Name))));
					return;
				}

				//Check for valid channel
				values = values[1].Split(new char[] { ' ' }, 2);
				//See if the user can see and thus edit that channel
				channel = await Actions.getChannelEditAbility(Context, values[0]);
				if (channel == null)
					return;

				//Say the overwrites on a channel
				if (values.Length == 1)
				{
					var roleOverwrites = new List<string>();
					var userOverwrites = new List<string>();
					foreach (Overwrite overwrite in channel.PermissionOverwrites)
					{
						if (overwrite.TargetType == PermissionTarget.Role)
						{
							roleOverwrites.Add(Context.Guild.GetRole(overwrite.TargetId).Name);
						}
						else
						{
							userOverwrites.Add((await Context.Guild.GetUserAsync(overwrite.TargetId)).Username);
						}
					}
					//Embed saying the overwrites
					var embed = Actions.makeNewEmbed(null, channel.Name);
					Actions.addField(embed, "Role", String.Join("\n", roleOverwrites));
					Actions.addField(embed, "User", String.Join("\n", userOverwrites));
					await Actions.sendEmbedMessage(Context.Channel, embed);
					return;
				}

				//Check if valid role or user
				role = await Actions.getRole(Context, values[1]);
				if (role == null)
				{
					user = await Actions.getUser(Context.Guild, values[1]);
					if (user == null)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."));
						return;
					}
				}

				//Say the permissions of the overwrite
				foreach (Overwrite overwrite in channel.PermissionOverwrites)
				{
					if (role != null && overwrite.TargetId.Equals(role.Id))
					{
						//Embed showing the perm overwrites on a role
						var embed = Actions.makeNewEmbed(title: String.Format("{0} on {1} ({2})", role.Name, channel.Name, Actions.getChannelType(channel)));
						Actions.addField(embed, "Permission", String.Join("\n", Actions.getPerms(overwrite, channel).Keys));
						Actions.addField(embed, "Value", String.Join("\n", Actions.getPerms(overwrite, channel).Values));
						await Actions.sendEmbedMessage(Context.Channel, embed);
						return;
					}
					else if (user != null && overwrite.TargetId.Equals(user.Id))
					{
						//Embed showing the perm overwrites on a user
						var embed = Actions.makeNewEmbed(title: String.Format("{0}#{1} on {2} ({3})", user.Username, user.Discriminator, channel.Name, Actions.getChannelType(channel)));
						Actions.addField(embed, "Permission", String.Join("\n", Actions.getPerms(overwrite, channel).Keys));
						Actions.addField(embed, "Value", String.Join("\n", Actions.getPerms(overwrite, channel).Values));
						await Actions.sendEmbedMessage(Context.Channel, embed);
						return;
					}
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Unable to show permissions for `{0}` on `{1} ({2})`.",
					values[1], channel.Name, Actions.getChannelType(channel))));
				return;
			}
			else if (actionName.Equals("allow") || actionName.Equals("deny") || actionName.Equals("inherit"))
			{
				values = values[1].Split(new char[] { ' ' }, 2);

				//Check if valid number of arguments
				if (values.Length == 1)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}

				//See if the user can see and thus edit that channel
				channel = await Actions.getChannelEditAbility(Context, values[0]);
				if (channel == null)
					return;

				//Check if valid perms and potential role/user
				var potentialRoleOrUser = "";
				if (Actions.getStringAndPermissions(values[1], out potentialRoleOrUser, out permissions))
				{
					//See if valid role
					role = Actions.getRole(Context.Guild, potentialRoleOrUser);
					if (role == null)
					{
						//See if valid user
						user = await Actions.getUser(Context.Guild, potentialRoleOrUser);
						if (user == null)
						{
							//Give error if no user or role that's valid
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."));
							return;
						}
					}
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No permissions supplied."));
					return;
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the generic permissions
			var genericPerms = Variables.ChannelPermissions.Select(x => x.Name).ToList();
			//Check if valid permissions
			var validPerms = permissions.Intersect(genericPerms, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				var invalidPerms = new List<string>();
				foreach (string permission in permissions)
				{
					if (!validPerms.Contains(permission, StringComparer.OrdinalIgnoreCase))
					{
						invalidPerms.Add(permission);
					}
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid {0} supplied: `{1}`.",
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
			uint? allowBits = 0;
			uint? denyBits = 0;
			if (role != null)
			{
				if (channel.GetPermissionOverwrite(role).HasValue)
				{
					allowBits = (uint?)channel.GetPermissionOverwrite(role).Value.AllowValue;
					denyBits = (uint?)channel.GetPermissionOverwrite(role).Value.DenyValue;
				}
			}
			else
			{
				if (channel.GetPermissionOverwrite(user).HasValue)
				{
					allowBits = (uint?)channel.GetPermissionOverwrite(user).Value.AllowValue;
					denyBits = (uint?)channel.GetPermissionOverwrite(user).Value.DenyValue;
				}
			}

			//Changing the bit values
			foreach (string permission in permissions)
			{
				changeValue = await Actions.getBit(Context, permission, changeValue);
			}
			if (actionName.Equals("allow"))
			{
				allowBits |= changeValue;
				denyBits &= ~changeValue;
				actionName = "allowed";
			}
			else if (actionName.Equals("inherit"))
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
				await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions((uint)allowBits, (uint)denyBits));
				roleNameOrUsername = role.Name;
			}
			else
			{
				await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions((uint)allowBits, (uint)denyBits));
				roleNameOrUsername = user.Username + "#" + user.Discriminator;
			}

			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` for `{2}` on `{3} ({4})`",
				actionName, String.Join("`, `", permissions), roleNameOrUsername, channel.Name, Actions.getChannelType(channel)), 7500);
		}

		[Command("channelpermscopy")]
		[Alias("chpc")]
		[Usage("channelpermscopy " + Constants.CHANNEL_INSTRUCTIONS + " " + Constants.CHANNEL_INSTRUCTIONS + " [Role|User|All]")]
		[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task CopyChannelPermissions([Remainder] string input)
		{
			//Get arguments
			var inputArray = input.ToLower().Split(new char[] { ' ' }, 3);

			//Separating the channels
			var inputChannel = await Actions.getChannel(Context, inputArray[0]);
			if (inputChannel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}

			//See if the user can see and thus edit that channel
			var outputChannel = await Actions.getChannelEditAbility(Context, inputArray[1]);
			if (outputChannel == null)
				return;

			//Trim the third arg
			inputArray[2] = inputArray[2].Trim();

			//Copy the selected target
			var target = "";
			if (inputArray[2].Equals("all"))
			{
				target = "ALL";
				foreach (Overwrite permissionOverwrite in inputChannel.PermissionOverwrites)
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
				}
			}
			else
			{
				var role = await Actions.getRole(Context, inputArray[2]);
				if (role != null)
				{
					target = role.Name;
					await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(inputChannel.GetPermissionOverwrite(role).Value.AllowValue,
						inputChannel.GetPermissionOverwrite(role).Value.DenyValue));
				}
				else
				{
					var user = await Actions.getUser(Context.Guild, inputArray[2]);
					if (user != null)
					{
						target = user.Username;
						await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(inputChannel.GetPermissionOverwrite(user).Value.AllowValue,
							inputChannel.GetPermissionOverwrite(user).Value.DenyValue));
					}
					else
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid role/user or all input."));
						return;
					}
				}
			}

			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` from `{1} ({2})` to `{3} ({4})`",
				target, inputChannel.Name, Actions.getChannelType(inputChannel), outputChannel.Name, Actions.getChannelType(outputChannel)), 7500);
		}

		[Command("channelpermsclear")]
		[Alias("chpcl")]
		[Usage("channelpermsclear " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Removes all permissions set on a channel.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task ClearChannelPermissions([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			var channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//Check if channel has permissions to clear
			if (channel.PermissionOverwrites.Count < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Channel has no permissions to clear."));
				return;
			}

			//Remove all the permission overwrites
			channel.PermissionOverwrites.ToList().ForEach(async x =>
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
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all channel permissions from `{0} ({1})`.",
				channel.Name, Actions.getChannelType(channel)), 7500);
		}

		[Command("channelname")]
		[Alias("chn")]
		[Usage("channelname [#Channel|[Channel|Position{x}/Text|Voice]] [New Name]")]
		[Summary("Changes the name of the channel. This is *extremely* useful for when multiple channels have the same name but you want to edit things.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelName([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Checking if valid name
			if (inputArray[1].Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."));
				return;
			}
			else if (inputArray[1].Length > Constants.CHANNEL_NAME_MAX_LENGTH || inputArray[1].Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name must be between 2 and 100 characters long."));
				return;
			}
			else if (inputArray[1].Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase) && await Actions.getDuplicateBotChan(Context.Guild))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Please don't try to rename a channel to the bot channel if one alreay exists. All that does is confuse the bot.");
				return;
			}

			//Initialize the channel
			IGuildChannel channel = null;

			//See if it's a position trying to be gotten instead
			int position;
			if (inputArray[0].ToLower().Contains("position{") && int.TryParse(inputArray[0].Substring(9, 1), out position))
			{
				//Split the input
				var splitInputArray = inputArray[0].Split(new char[] { '/' }, 2);
				//Give the channeltype
				var channelType = splitInputArray[1].ToLower();

				//Initialize the channels list
				var textChannels = new List<ITextChannel>();
				var voiceChannels = new List<IVoiceChannel>();

				if (channelType == Constants.TEXT_TYPE)
				{
					textChannels = (await Context.Guild.GetTextChannelsAsync()).Where(x => x.Position == position).ToList();
				}
				else if (channelType == Constants.VOICE_TYPE)
				{
					voiceChannels = (await Context.Guild.GetVoiceChannelsAsync()).Where(x => x.Position == position).ToList();
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid channel type."));
					return;
				}

				//Check the count now
				if (textChannels.Count == 0 && voiceChannels.Count == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No {0} channel has a position of `{1}`.", channelType, position)));
					return;
				}
				else if (textChannels.Count == 1 || voiceChannels.Count == 1)
				{
					//Get the channel
					var chan = textChannels.Count == 1 ? textChannels.First() as IGuildChannel : voiceChannels.First() as IGuildChannel;
					channel = await Actions.getChannelEditAbility(chan, Context.User as IGuildUser);
				}
				else
				{
					//Get the count
					var count = textChannels.Any() ? textChannels.Count : voiceChannels.Count;
					await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("`{0}` {1} channels have the position `{2}`.", count, channelType, position));
					return;
				}
			}

			//Check if valid channel
			channel = channel ?? await Actions.getChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			var previousName = channel.Name;
			await channel.ModifyAsync(x => x.Name = inputArray[1]);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed channel `{0}` to `{1}`.", previousName, inputArray[1]), 5000);
		}

		[Command("channeltopic")]
		[Alias("cht")]
		[Usage("channeltopic [#Channel] [New Topic]")]
		[Summary("Changes the subtext of a channel to whatever is input.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelTopic([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var newTopic = inputArray[1];

			//See if valid length
			if (newTopic.Length > Constants.TOPIC_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Topics cannot be longer than 1024 characters in length."));
				return;
			}

			//Test if valid channel
			var channel = await Actions.getChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			//See if not a text channel
			if (Actions.getChannelType(channel) != Constants.TEXT_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only text channels can have their topic set."));
				return;
			}

			//See what current topic is
			var currentTopic = (channel as ITextChannel).Topic;
			if (String.IsNullOrWhiteSpace(currentTopic))
			{
				currentTopic = "NOTHING";
			}

			await (channel as ITextChannel).ModifyAsync(x => x.Topic = newTopic);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.",
				channel.Name, currentTopic, newTopic == "" ? "NOTHING" : newTopic));
		}

		[Command("channellimit")]
		[Alias("chl")]
		[Usage("channellimit [Channel Name] [New Limit]")]
		[Summary("Changes the limit to how many users can be in a voice channel. The limit ranges from 0 (no limit) to 99.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelLimit([Remainder] string input)
		{
			//Separate the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var channelName = inputArray[0];
			var newLimit = inputArray[1];

			//Check if valid channel that the user can edit
			var channel = await Actions.getChannelEditAbility(Context, channelName + "/voice");
			if (channel == null)
				return;
			else if (Actions.getChannelType(channel) != Constants.VOICE_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only a voice channel can have a limit set on it."));
				return;
			}

			//Check if valid number
			var limit = 0;
			if (!int.TryParse(newLimit, out limit))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}

			//Check if number between 0 and 99
			if (limit > 99 || limit < 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Number must be between 0 and 99 inclusive."));
				return;
			}

			//Change it and send a success message
			await (channel as IVoiceChannel).ModifyAsync(x => x.UserLimit = limit);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}`.", channel.Name, limit));
		}

		[Command("channelbitrate")]
		[Alias("chbr")]
		[Usage("channelbitrate [Channel Name] [8 to 96]")]
		[Summary("Changes the bit rate (in kbps) on the selected channel to the given value. The default value is 64.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelBitRate([Remainder] string input)
		{
			//Separate the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var channelName = inputArray[0];
			var newBitRate = inputArray[1];

			//Check if valid channel that the user can edit
			var channel = await Actions.getChannelEditAbility(Context, channelName + "/voice");
			if (channel == null)
				return;
			else if (Actions.getChannelType(channel) != Constants.VOICE_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only a voice channel can have its bit rate changed."));
				return;
			}

			//Check if valid number
			var bitRate = 0;
			if (!int.TryParse(newBitRate, out bitRate))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}

			//Check if number between 8 and 96 
			if (bitRate > 96 || bitRate < 8)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Number must be between 8 and 96 inclusive."));
				return;
			}

			//Change it and send a success message
			await (channel as IVoiceChannel).ModifyAsync(x => x.Bitrate = bitRate * 1000);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}kbps`.", channel.Name, bitRate));
		}
	}
}
