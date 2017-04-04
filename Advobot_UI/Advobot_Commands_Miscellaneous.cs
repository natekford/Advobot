using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace Advobot
{
	//Miscellaneous commands are random commands that don't exactly fit the other groups
	[Name("Miscellaneous")]
	public class Advobot_Commands_Miscellaneous : ModuleBase
	{
		#region Help
		[Command("help")]
		[Alias("h", "info")]
		[Usage("<Command>")]
		[Summary("Prints out the aliases of the command, the usage of the command, and the description of the command. If left blank will print out a link to the documentation of this bot.")]
		[DefaultEnabled(true)]
		public async Task Help([Optional, Remainder] string input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				//Description string
				var text = String.Format("Type `{0}commands` for the list of commands.\nType `{0}help [Command]` for help with a command.", Properties.Settings.Default.Prefix);
				//Make the embed
			    var embed = Actions.MakeNewEmbed("Commands", text);
				Actions.AddField(embed, "Syntax", "[] means required.\n<> means optional.\n| means or.");
				Actions.AddField(embed, "Links", "[GitHub Repository](https://github.com/advorange/Advobot)\n[Discord Server](https://discord.gg/ad)");
				Actions.AddFooter(embed, "Help");
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}

			//Get the input command, if nothing then link to documentation
			var commandParts = input.Split(new char[] { '[' }, 2);
			if (input.IndexOf('[') == 0)
			{
				if (Actions.CaseInsEquals(commandParts[1], "command"))
				{
					var text = "If you do not know what commands this bot has, type `" + Constants.BOT_PREFIX + "commands` for a list of commands.";
					await Actions.MakeAndDeleteSecondaryMessage(Context, text, 10000);
					return;
				}
				await Actions.MakeAndDeleteSecondaryMessage(Context, "[] means required information. <> means optional information. | means or.", 10000);
				return;
			}

			//Send the message for that command
			var helpEntry = Variables.HelpList.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, input));
			if (helpEntry == null)
			{
				//Find the command based on its aliases
				Variables.HelpList.ForEach(x =>
				{
					if (x.Aliases.Contains(input))
					{
						helpEntry = x;
						return;
					}
				});
				if (helpEntry == null)
				{
					//Find close words
					var closeHelps = Actions.GetCommandsWithInputInName(Actions.GetCommandsWithSimilarName(input), input)?.Distinct().ToList();

					if (closeHelps != null && closeHelps.Any())
					{
						//Format a message to be said
						var count = 1;
						var msg = "Did you mean any of the following:\n" + String.Join("\n", closeHelps.Select(x => String.Format("`{0}.` {1}", count++.ToString("00"), x.Help.Name)));

						//Give the user a new list
						Variables.ActiveCloseHelp.RemoveAll(x => x.User == Context.User);
						var list = new ActiveCloseHelp(Context.User as IGuildUser, closeHelps);
						Variables.ActiveCloseHelp.Add(list);
						Actions.RemoveActiveCloseHelp(list);

						//Send the message
						await Actions.MakeAndDeleteSecondaryMessage(Context, msg, 5000);
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nonexistent command."));
					}
					return;
				}
			}

			var description = Actions.GetHelpString(helpEntry);
			var guildPrefix = Variables.Guilds[Context.Guild.Id].Prefix;
			if (!String.IsNullOrWhiteSpace(guildPrefix))
			{
				description = description.Replace(Properties.Settings.Default.Prefix, guildPrefix);
			}

			await Actions.SendEmbedMessage(Context.Channel, Actions.AddFooter(Actions.MakeNewEmbed(helpEntry.Name, description), "Help"));
		}

		[Command("commands")]
		[Alias("cmds")]
		[Usage("<Category|All>")]
		[Summary("Prints out the commands in that category of the command list.")]
		[DefaultEnabled(true)]
		public async Task Commands([Optional, Remainder] string input)
		{
			//See if it's empty
			if (String.IsNullOrWhiteSpace(input))
			{
				var embed = Actions.MakeNewEmbed("Categories",
					String.Format("Type `{0}commands [Category]` for commands from that category.\n\n{1}", Constants.BOT_PREFIX, String.Join("\n", Enum.GetNames(typeof(CommandCategory)))));
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}
			//Check if all
			else if (Actions.CaseInsEquals(input, "all"))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("ALL", String.Join("\n", Variables.CommandNames)));
				return;
			}

			//Check what category
			if (!Enum.TryParse(input, true, out CommandCategory category))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Category does not exist."));
				return;
			}

			//Send the message
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(Enum.GetName(typeof(CommandCategory), category), String.Join("\n", Actions.GetCommands(Context.Guild, (int)category))));
		}
		#endregion

		#region IDs
		[Command("idguild")]
		[Alias("idg")]
		[Usage("")]
		[Summary("Shows the ID of the guild.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task ServerID()
		{
			await Actions.SendChannelMessage(Context, String.Format("This guild has the ID `{0}`.", Context.Guild.Id) + " ");
		}

		[Command("idchannel")]
		[Alias("idc")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Shows the ID of the given channel.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task ChannelID([Remainder] string input)
		{
			var channel = await Actions.GetChannel(Context, input);
			if (channel == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}
			await Actions.SendChannelMessage(Context, String.Format("The {0} channel `{1}` has the ID `{2}`.", Actions.GetChannelType(channel), channel.Name, channel.Id));
		}

		[Command("idrole")]
		[Alias("idr")]
		[Usage("[Role]")]
		[Summary("Shows the ID of the given role.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task RoleID([Remainder] string input)
		{
			var role = await Actions.GetRole(Context, input);
			if (role == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}
			await Actions.SendChannelMessage(Context, String.Format("The role `{0}` has the ID `{1}`.", role.Name, role.Id));
		}

		[Command("iduser")]
		[Alias("idu")]
		[Usage("<@User>")]
		[Summary("Shows the ID of the given user.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task UserID([Optional, Remainder] string input)
		{
			var user = await Actions.GetUser(Context.Guild, input) ?? await Context.Guild.GetUserAsync(Context.User.Id);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			await Actions.SendChannelMessage(Context, String.Format("The user `{0}#{1}` has the ID `{2}`.", user.Username, user.Discriminator, user.Id));
		}
		#endregion

		#region Info
		[Command("infobot")]
		[Alias("infb")]
		[Usage("")]
		[Summary("Displays various information about the bot.")]
		[DefaultEnabled(true)]
		public async Task BotInfo()
		{
			var span = DateTime.UtcNow.Subtract(Variables.StartupTime);

			//Make the description
			var description = String.Format(
				"Online since: {0}\n" +
				"Uptime: {1}:{2}:{3}:{4}\n" +
				"Guild count: {5}\n" +
				"Cumulative member count: {6}\n" +
				"Current shard: {7}\n",
				Variables.StartupTime,
				span.Days, span.Hours.ToString("00"),
				span.Minutes.ToString("00"),
				span.Seconds.ToString("00"),
				Variables.TotalGuilds,
				Variables.TotalUsers,
				Variables.Client.GetShardFor(Context.Guild).ShardId);

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, description);
			Actions.AddAuthor(embed, Variables.Bot_Name, Context.Client.CurrentUser.GetAvatarUrl());
			Actions.AddFooter(embed, "Version " + Constants.BOT_VERSION);

			//First field
			var firstField = Actions.FormatLoggedThings();
			Actions.AddField(embed, "Logged Actions", firstField);

			//Second field
			var secondField = String.Format(
				"Attempted commands: {0}\n" +
				"Successful commands: {1}\n" +
				"Failed commands: {2}\n",
				Variables.SucceededCommands + Variables.FailedCommands,
				Variables.SucceededCommands,
				Variables.FailedCommands);
			Actions.AddField(embed, "Commands", secondField);

			//Third field
			var thirdField = String.Format(
				"Latency: {0}ms\n" +
				"Memory usage: {1:0.00}MB\n" +
				"Thread count: {2}\n",
				Variables.Client.GetLatency(),
				Process.GetCurrentProcess().WorkingSet64 / 1000000.0,
				Process.GetCurrentProcess().Threads.Count);
			Actions.AddField(embed, "Technical", thirdField);

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("infouser")]
		[Alias("infu")]
		[Usage("<@User>")]
		[Summary("Displays various information about the user. Not 100% accurate.")]
		[DefaultEnabled(true)]
		public async Task UserInfo([Optional, Remainder] string input)
		{
			//Get the user
			var user = await Actions.GetUser(Context.Guild, input) ?? await Context.Guild.GetUserAsync(Context.User.Id);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Get a list of roles
			var roles = user.RoleIds.Where(x => x != Context.Guild.Id).Select(x => Context.Guild.GetRole(x));

			//Get a list of channels
			var channels = new List<string>();
			(await Context.Guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
			{
				if (roles.Any(y => x.GetPermissionOverwrite(y).HasValue && x.GetPermissionOverwrite(y).Value.ReadMessages == PermValue.Allow) || user.GetPermissions(x).ReadMessages)
				{
					channels.Add(x.Name);
				}
			});
			(await Context.Guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
			{
				if (roles.Any(y => x.GetPermissionOverwrite(y).HasValue && x.GetPermissionOverwrite(y).Value.Connect == PermValue.Allow) || user.GetPermissions(x).Connect)
				{
					channels.Add(x.Name + " (Voice)");
				}
			});

			//Get an ordered list of when users joined the guild
			var guildUsers = await Context.Guild.GetUsersAsync();
			var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

			//Make the description
			var description = String.Format(
				"ID: {0}\n" +
				"Created: {1} {2}, {3} at {4}\n" +
				"Joined: {5} {6}, {7} at {8} (#{9} to join the guild)\n" +
				"\n" +
				"Current game: {10}\n" +
				"Online status: {11}\n",
				user.Id,
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.CreatedAt.Month),
				user.CreatedAt.UtcDateTime.Day,
				user.CreatedAt.UtcDateTime.Year,
				user.CreatedAt.UtcDateTime.ToLongTimeString(),
				System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.JoinedAt.Value.UtcDateTime.Month),
				user.JoinedAt.Value.UtcDateTime.Day,
				user.JoinedAt.Value.UtcDateTime.Year,
				user.JoinedAt.Value.UtcDateTime.ToLongTimeString(),
				users.IndexOf(user) + 1,
				user.Game == null ? "N/A" : user.Game.Value.Name.ToString(),
				user.Status);

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, description, roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color, thumbnailURL: user.GetAvatarUrl());
			Actions.AddAuthor(embed, String.Format("{0}#{1} {2}", user.Username, user.Discriminator, (user.Nickname == null ? "" : "(" + user.Nickname + ")")), user.GetAvatarUrl(), user.GetAvatarUrl());
			Actions.AddFooter(embed, "Userinfo");

			//Add the channels the user can access
			if (channels.Count() != 0)
			{
				Actions.AddField(embed, "Channels", String.Join(", ", channels));
			}
			//Add the roles the user has
			if (roles.Count() != 0)
			{
				Actions.AddField(embed, "Roles", String.Join(", ", roles.Select(x => x.Name)));
			}
			//Add the voice channel
			if (user.VoiceChannel != null)
			{
				var text = String.Format("Server mute: {0}\nServer deafen: {1}\nSelf mute: {2}\nSelf deafen: {3}",
					user.IsMuted.ToString(), user.IsDeafened.ToString(), user.IsSelfMuted.ToString(), user.IsSelfDeafened.ToString());

				Actions.AddField(embed, "Voice Channel: " + user.VoiceChannel.Name, text);
			}

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("infoemoji")]
		[Alias("infe")]
		[Usage("[Emoji]")]
		[Summary("Shows information about an emoji. Only global emojis where the bot is in a guild that gives them will have a 'From...' text.")]
		[DefaultEnabled(true)]
		public async Task EmojiInfo([Remainder] string input)
		{
			//Parse out the emoji
			if (!Emoji.TryParse(input, out Emoji emoji))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid emoji supplied."));
				return;
			}

			//Try to find the emoji if global
			var guilds = (await Context.Client.GetGuildsAsync()).Where(x =>
			{
				var placeholder = x.Emojis.FirstOrDefault(y => y.Id == emoji.Id);
				return placeholder.IsManaged && placeholder.RequireColons;
			});

			//Format a description
			var description = String.Format("**ID:** `{0}`\n", emoji.Id);
			if (guilds.Any())
			{
				description += String.Format("**From:** `{0}`", String.Join("`, `", guilds.Select(x => x.Name)));
			}

			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(emoji.Name, description, thumbnailURL: emoji.Url));
		}

		[Command("infoinvite")]
		[Alias("infi")]
		[Usage("[Invite Code]")]
		[Summary("Lists the user who created the invite, the channel it was created on, the uses, and the creation date/time.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task InviteInfo([Remainder] string input)
		{
			//Get the invite
			var inv = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == input);
			//Check if null
			if (inv == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That invite cannot be gotten."));
				return;
			}
			//Get all of the variables
			var user = Actions.FormatUser(inv.Inviter);
			var channel = Actions.FormatChannel(await Context.Guild.GetChannelAsync(inv.ChannelId));
			var uses = inv.Uses;
			var time = inv.CreatedAt.UtcDateTime.ToShortTimeString();
			var date = inv.CreatedAt.UtcDateTime.ToShortDateString();
			//Make the embed
			var embed = Actions.MakeNewEmbed(inv.Code, String.Format("**Inviter:** `{0}`\n**Channel:** `{1}`\n**Uses:** `{2}`\n**Created At:** `{3}` on `{4}`", user, channel, uses, time, date));
			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("useravatar")]
		[Alias("uav")]
		[Usage("<Type:[Gif|Png|Jpg|Webp]> <@user>")]
		[Summary("Shows the URL of the given user's avatar (no formatting in case people on mobile want it easily). Currently every avatar is displayed with an extension type of gif.")]
		[DefaultEnabled(true)]
		public async Task UserAvatar([Optional, Remainder] string input)
		{
			//Split the input
			var inputArray = input?.Split(new char[] { ' ' }, 2);
			var formatStr = Actions.GetVariable(inputArray, "type");

			//Get the type of image
			var format = ImageFormat.Auto;
			if (formatStr != null && !Enum.TryParse(formatStr, true, out format))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid avatar format supplied."));
				return;
			}

			//Get the user
			IGuildUser user;
			var mentions = Context.Message.MentionedUserIds;
			if (mentions.Count == 0)
			{
				user = Context.User as IGuildUser;
			}
			else if (mentions.Count == 1)
			{
				user = await Context.Guild.GetUserAsync(mentions.FirstOrDefault());
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Too many user mentions input."));
				return;
			}

			//Send a message with the URL
			await Context.Channel.SendMessageAsync(user.GetAvatarUrl(format));
		}

		[Command("userjoins")]
		[Alias("ujs")]
		[Usage("")]
		[Summary("Lists most of the users who have joined the guild. Not 100% accurate.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task UserJoins()
		{
			var users = (await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt.HasValue).OrderBy(x => x.JoinedAt);
			var count = 1;
			var padLength = users.Count().ToString().Length;
			var userMsg = String.Join("\n", users.Select(x =>
			{
				var time = x.JoinedAt.Value.UtcDateTime;
				return String.Format("`{0}.` `{1}` joined at `{2}` on `{3}`.", count++.ToString().PadLeft(padLength, '0'), Actions.FormatUser(x), time.ToShortTimeString(), time.ToShortDateString());
			}));

			await Actions.SendPotentiallyBigEmbed(Context.Guild, Context.Channel, Actions.MakeNewEmbed("Users", userMsg), userMsg, "User_Joins_");
		}

		[Command("userjoinedat")]
		[Alias("ujat")]
		[Usage("[Position]")]
		[Summary("Shows the user which joined the guild in that position. Not 100% accurate.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task UserJoinedAt([Remainder] string input)
		{
			if (Int32.TryParse(input, out int position))
			{
				var guildUsers = await Context.Guild.GetUsersAsync();
				var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();
				if (position >= 1 && position < users.Count)
				{
					var user = users[position - 1];
					await Actions.SendChannelMessage(Context, String.Format("`{0}#{1}` was #{2} to join the guild on `{3} {4}, {5}` at `{6}`.",
						user.Username,
						user.Discriminator,
						position,
						System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.JoinedAt.Value.UtcDateTime.Month),
						user.JoinedAt.Value.UtcDateTime.Day,
						user.JoinedAt.Value.UtcDateTime.Year,
						user.JoinedAt.Value.UtcDateTime.ToLongTimeString()));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Something besides a number was input."));
			}
		}

		[Command("userswithrole")]
		[Alias("uwr")]
		[Usage("[Role]")]
		[Summary("Prints out a list of all users with the given role. File specifies a text document which can show more symbols. Upload specifies to use a text uploader.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task UsersWithRole([Remainder] string input)
		{
			//Initializing input and variables
			var role = await Actions.GetRole(Context, input);
			if (role == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			//Grab each user
			var users = "";
			var count = 1;
			(await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt.HasValue).ToList().OrderBy(x => x.JoinedAt).ToList().ForEach(x =>
			{
				if (x.RoleIds.ToList().Contains(role.Id))
				{
					users += String.Format("`{0}.` `{1}`\n", count++.ToString("00"), Actions.FormatUser(x));
				}
			});

			//Checking if the message can fit in a single message
			var roleName = role.Name.Substring(0, 3) + Constants.ZERO_LENGTH_CHAR + role.Name.Substring(3);
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(roleName, users));
		}

		[Command("userswithname")]
		[Alias("uwn")]
		[Usage("[Name]")]
		[Summary("Lists all users where their username contains the given string.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task UsersWithName([Remainder] string input)
		{
			//Find the users
			var users = (await Context.Guild.GetUsersAsync()).Where(x => Actions.CaseInsIndexOf(x.Username, input)).OrderBy(x => x.JoinedAt).ToList();

			//Initialize the string
			var description = "";

			//Add them to the string
			var count = 1;
			users.ForEach(x =>
			{
				description += String.Format("`{0}.` `{1}`\n", count++.ToString("00"), Actions.FormatUser(x));
			});

			//Set the title
			var title = String.Format("Users With Names Containing '{0}'", input);

			//Make and send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, description));
		}

		[Command("currentmembercount")]
		[Alias("cmc")]
		[Usage("")]
		[Summary("Shows the current number of members in the guild.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task CurrentMemberCount()
		{
			await Actions.SendChannelMessage(Context, String.Format("The current member count is `{0}`.", (Context.Guild as SocketGuild).MemberCount));
		}


		[Command("listemojis")]
		[Alias("lemojis")]
		[Usage("[Global|Guild]")]
		[Summary("Lists the emoji in the guild. As of right now, with the current API wrapper version this bot uses, there's no way to upload or remove emojis yet; sorry.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageEmojis)]
		[DefaultEnabled(true)]
		public async Task ListEmojis([Remainder] string input)
		{
			//Make the string
			string description = null;

			//Add the emojis to the string
			int count = 1;
			if (Actions.CaseInsEquals(input, "guild"))
			{
				//Get all of the guild emojis
				Context.Guild.Emojis.Where(x => !x.IsManaged).ToList().ForEach(x =>
				{
					description += String.Format("`{0}.` <:{1}:{2}> `{3}`\n", count++.ToString("00"), x.Name, x.Id, x.Name);
				});
			}
			else if (Actions.CaseInsEquals(input, "global"))
			{
				//Get all of the global emojis
				Context.Guild.Emojis.Where(x => x.IsManaged).ToList().ForEach(x =>
				{
					description += String.Format("`{0}.` <:{1}:{2}> `{3}`\n", count++.ToString("00"), x.Name, x.Id, x.Name);
				});
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option."));
				return;
			}

			//Check if the description is still null
			description = description ?? String.Format("This guild has no {0} emojis.", input.ToLower());

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Emojis", description));
		}
		#endregion

		#region Instant Invites
		[Command("invitelist")]
		[Alias("invl")]
		[Usage("")]
		[Summary("Gives a list of all the instant invites on the guild.")]
		[UserHasAPermission]
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

		[Command("invitecreate")]
		[Alias("invc")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS + " [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]")]
		[Summary("The first argument is the channel. The second is how long the invite will last for. The third is how many users can use the invite. The fourth is the temporary membership option.")]
		[PermissionRequirement(1U << (int)GuildPermission.CreateInstantInvite)]
		[DefaultEnabled(true)]
		public async Task CreateInstantInvite([Remainder] string input)
		{
			//Split the input
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length != 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var channelInput = inputArray[0];
			var timeStr = inputArray[1];
			var usesStr = inputArray[2];
			var tempStr = inputArray[3];

			//Check validity of channel
			var returnedChannel = await Actions.GetChannelPermability(Context, channelInput);
			var channel = returnedChannel.Channel;
			if (returnedChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleChannelPermsLacked(Context, returnedChannel);
				return;
			}

			//Set the time in seconds
			int? nullableTime = null;
			int[] validTimes = { 1800, 3600, 21600, 43200, 86400 };
			if (int.TryParse(timeStr, out int time) && validTimes.Contains(time))
			{
				nullableTime = time;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time supplied."));
				return;
			}

			//Set the max amount of users
			int? nullableUsers = null;
			int[] validUsers = { 1, 5, 10, 25, 50, 100 };
			if (int.TryParse(usesStr, out int users) && validUsers.Contains(users))
			{
				nullableUsers = users;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid uses supplied."));
				return;
			}

			//Set tempmembership
			if (!bool.TryParse(tempStr, out bool tempMembership))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid uses supplied."));
				return;
			}

			//Make into valid invite link
			var inv = await channel.CreateInviteAsync(nullableTime, nullableUsers, tempMembership);

			await Actions.SendChannelMessage(Context, String.Format("Here is your invite for `{0}`: {1} \nIt will last for{2}, {3}{4}.",
				Actions.FormatChannel(channel),
				inv.Url,
				nullableTime == null ? "ever" : " " + time.ToString() + " seconds",
				nullableUsers == null ? (tempMembership ? "has no limit of users" : "and has no limit of users") :
										(tempMembership ? "has a limit of " + users.ToString() + " users" : " and has a limit of " + users.ToString() + " users"),
				tempMembership ? ", and users will only receive temporary membership" : ""));
		}

		[Command("invitedelete")]
		[Alias("invd")]
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

		[Command("invitedeletemultiple")]
		[Alias("invdm")]
		[Usage("[User:@User|Channel:" + Constants.CHANNEL_INSTRUCTIONS + "|Uses:Number|Expires:[True|False]]")]
		[Summary("Deletes all invites satisfying the given condition of either user, creation channel, uses, or expiry time.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageChannels)]
		[DefaultEnabled(true)]
		public async Task DeleteMultipleInvites([Remainder] string input)
		{
			//Get the guild's invites
			var guildInvites = await Context.Guild.GetInvitesAsync();
			if (!guildInvites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This guild has no invites."));
				return;
			}

			//Get the given variable out
			var userStr = Actions.GetVariable(input, "user");
			var chanStr = Actions.GetVariable(input, "channel");
			var usesStr = Actions.GetVariable(input, "uses");
			var exprStr = Actions.GetVariable(input, "expired");

			//Set the action telling what variable
			DeleteInvAction? action = null;
			//Check if user
			IGuildUser user = null;
			if (!String.IsNullOrWhiteSpace(userStr))
			{
				user = await Actions.GetUser(Context.Guild, userStr);
				if (user == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
					return;
				}
				else
				{
					action = DeleteInvAction.User;
				}
			}
			//Check if channel
			IGuildChannel channel = null;
			if (!String.IsNullOrWhiteSpace(chanStr))
			{
				var returnedChannel = await Actions.GetChannelPermability(Context, chanStr);
				channel = returnedChannel.Channel;
				if (returnedChannel.Reason != FailureReason.Not_Failure)
				{
					await Actions.HandleChannelPermsLacked(Context, returnedChannel);
					return;
				}
				else
				{
					action = DeleteInvAction.Channel;
				}
			}
			//Check if uses
			int uses = 0;
			if (!String.IsNullOrWhiteSpace(usesStr))
			{
				if (!int.TryParse(usesStr, out uses))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid number for uses."));
					return;
				}
				else
				{
					action = DeleteInvAction.Uses;
				}
			}
			//Check if expiry time
			bool expires = false;
			if (!String.IsNullOrWhiteSpace(exprStr))
			{
				if (!bool.TryParse(exprStr, out expires))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid boolean for expiry."));
					return;
				}
				else
				{
					action = DeleteInvAction.Uses;
				}
			}
			//Have gone through every other check so it's an error at this point
			if (action == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid target supplied."));
				return;
			}

			//Make a new list to store the invites that match the conditions in
			var invites = new List<IInvite>();
			switch (action)
			{
				case DeleteInvAction.User:
				{
					invites.AddRange(guildInvites.Where(x => x.Inviter.Id == user.Id));
					break;
				}
				case DeleteInvAction.Channel:
				{
					invites.AddRange(guildInvites.Where(x => x.ChannelId == channel.Id));
					break;
				}
				case DeleteInvAction.Uses:
				{
					invites.AddRange(guildInvites.Where(x => x.Uses == uses));
					break;
				}
				case DeleteInvAction.Expiry:
				{
					if (expires)
					{
						invites.AddRange(guildInvites.Where(x => x.MaxAge != null));
					}
					else
					{
						invites.AddRange(guildInvites.Where(x => x.MaxAge == null));
					}
					break;
				}
			}

			//Check if any invites were gotten
			if (!invites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invites satisfied the given condition."));
				return;
			}

			await invites.ForEachAsync(async x => await x.DeleteAsync());
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` instant invites on this guild.", invites.Count));
		}
		#endregion

		#region Miscellaneous
		[Command("makeanembed")]
		[Alias("mae")]
		[Usage("<\"Title:input\"> <\"Desc:input\"> <Img:url> <Url:url> <Thumb:url> <Color:int/int/int> <\"Author:input\"> <AuthorIcon:url> <AuthorUrl:url> <\"Foot:input\"> <FootIcon:url> " +
				"<\"Field[1-25]:input\"> <\"FieldText[1-25]:input\"> <FieldInline[1-25]:true|false>")]
		[Summary("Every single piece is optional. The stuff in quotes *must* be in quotes. URLs need the https:// in front. Fields need *both* Field and FieldText to work.")]
		[DefaultEnabled(true)]
		public async Task MakeAnEmbed([Remainder] string input)
		{
			//Split the input
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ').ToList();

			//Get the inputs
			var title = Actions.GetVariableAndRemove(inputArray, "title");
			var description = Actions.GetVariableAndRemove(inputArray, "desc");
			var imageURL = Actions.GetVariableAndRemove(inputArray, "Img");
			var URL = Actions.GetVariableAndRemove(inputArray, "url");
			var thumbnail = Actions.GetVariableAndRemove(inputArray, "thumb");
			var authorName = Actions.GetVariableAndRemove(inputArray, "author");
			var authorIcon = Actions.GetVariableAndRemove(inputArray, "authoricon");
			var authorURL = Actions.GetVariableAndRemove(inputArray, "authorurl");
			var footerText = Actions.GetVariableAndRemove(inputArray, "foot");
			var footerIcon = Actions.GetVariableAndRemove(inputArray, "footicon");

			//Get the color
			var color = Constants.BASE;
			var colorRGB = Actions.GetVariableAndRemove(inputArray, "color")?.Split('/');
			if (colorRGB != null && colorRGB.Length == 3)
			{
				const byte MAX_VAL = 255;
				if (byte.TryParse(colorRGB[0], out byte r) && byte.TryParse(colorRGB[1], out byte g) && byte.TryParse(colorRGB[2], out byte b))
				{
					color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
				}
			}

			//Make the embed
			var embed = Actions.MakeNewEmbed(title, description, color, imageURL, URL, thumbnail);
			//Add in the author
			Actions.AddAuthor(embed, authorName, authorIcon, authorURL);
			//Add in the footer
			Actions.AddFooter(embed, footerText, footerIcon);

			//Add in the fields and text
			for (int i = 1; i < 25; i++)
			{
				//Get the input for fields
				var field = Actions.GetVariableAndRemove(inputArray, "field" + i);
				var fieldText = Actions.GetVariableAndRemove(inputArray, "fieldtext" + i);
				//If either is null break out of this loop because they shouldn't be null
				if (field == null || fieldText == null)
					break;

				//Get the bool for the field
				bool.TryParse(Actions.GetVariableAndRemove(inputArray, "fieldinline" + i), out bool inlineBool);

				//Add in the field
				Actions.AddField(embed, field, fieldText, inlineBool);
			}

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("mentionrole")]
		[Alias("mnr")]
		[Usage("[Role]/[Message]")]
		[Summary("Mention an unmentionable role with the given message.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task MentionRole([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { '/' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var roleStr = inputArray[0];
			var textStr = inputArray[1];

			if (textStr.Length > Constants.MAX_MESSAGE_LENGTH_LONG)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Please keep the message to send under `{0}` characters.", Constants.MAX_MESSAGE_LENGTH_LONG)));
				return;
			}

			//Get the role and see if it can be changed
			var role = await Actions.GetRoleEditAbility(Context, roleStr);
			if (role == null)
				return;

			//See if people can already mention the role
			if (role.IsMentionable)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You can already mention this role."));
				return;
			}

			//Make the role mentionable
			await role.ModifyAsync(x => x.Mentionable = true);
			//Send the message
			await Actions.SendChannelMessage(Context, String.Format("{0}, {1}:{2}", Actions.FormatUser(Context.User), role.Mention, textStr));
			//Remove the mentionability
			await role.ModifyAsync(x => x.Mentionable = false);
		}

		[Command("test")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task Test([Optional, Remainder] string input)
		{
#if false
			var path = Actions.GetServerFilePath(Context.Guild.Id, "test.json");
			if (!String.IsNullOrWhiteSpace(input))
			{
				var bannedPhrase = new BannedPhrase<string>(input, PunishmentType.Nothing);
				var list = new List<BannedPhrase<string>>() { bannedPhrase };
				var ser = Actions.Serialize(bannedPhrase);
				var lser = Actions.Serialize(list);
				using (var writer = new System.IO.StreamWriter(path))
				{
					writer.Write(lser);
				}
			}
			else
			{
				string text;
				using (var reader = new System.IO.StreamReader(path))
				{
					text = reader.ReadToEnd();
				}
				var bannedPhrase = Actions.DeserializeBannedPhraseString(text);
			}
#else
			//Actions.SaveBannedStringRegexPunishments(Variables.Guilds[Context.Guild.Id], Context);
			Actions.SaveEverything(Variables.Guilds[Context.Guild.Id]);
#endif


			await Actions.MakeAndDeleteSecondaryMessage(Context, "test");
		}
#endregion
	}
}