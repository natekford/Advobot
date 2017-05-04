using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
			var commandParts = input.Split(new[] { '[' }, 2);
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

						var guildInfo = Variables.Guilds[Context.Guild.Id];

						//Create a new list, remove all others the user has, add the new one to the guild's list, remove it and the message that goes along with it after five seconds
						var acHelp = new ActiveCloseHelp(Context.User as IGuildUser, closeHelps);
						Variables.ActiveCloseHelp.ThreadSafeRemoveAll(x => x.User == Context.User);
						Variables.ActiveCloseHelp.ThreadSafeAdd(acHelp);
						await Actions.MakeAndDeleteSecondaryMessage(Context, msg, Constants.ACTIVE_CLOSE);
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
		[Command("infoguild")]
		[Alias("infg")]
		[Usage("")]
		[Summary("Displays various information about the guild.")]
		[DefaultEnabled(true)]
		public async Task InfoGuild()
		{
			var sGuild = Context.Guild as SocketGuild;
			var sOwner = sGuild.Owner;
			var title = Actions.FormatGuild(sGuild);
			var age = String.Format("**Created:** `{0}` (`{1}` days ago)", sGuild.CreatedAt.UtcDateTime, DateTime.UtcNow.Subtract(sGuild.CreatedAt.UtcDateTime).Days);
			var owner = String.Format("**Owner:** `{0}`", Actions.FormatUser(sOwner, sOwner?.Id));
			var region = String.Format("**Region:** `{0}`\n", sGuild.VoiceRegionId);
			var userCount = String.Format("**User Count:** `{0}`", sGuild.MemberCount);
			var roleCount = String.Format("**Role Count:** `{0}`", sGuild.Roles.Count);
			var channels = String.Format("**Channel Count:** `{0}` (`{1}` text, `{2}` voice)", sGuild.Channels.Count, sGuild.TextChannels.Count, sGuild.VoiceChannels.Count);
			var all = String.Join("\n", new List<string>() { age, owner, region, userCount, roleCount, channels });

			var embed = Actions.MakeNewEmbed(title, all, thumbnailURL: sGuild.IconUrl);
			Actions.AddFooter(embed, "Guild Info");
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("infochannel")]
		[Alias("infc")]
		[Usage(Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Displays various information about the given channel.")]
		[DefaultEnabled(true)]
		public async Task InfoChannel([Remainder] string input)
		{
			var sChannel = await Actions.GetChannel(Context, input) as SocketGuildChannel;
			if (sChannel == null)
				return;

			var title = Actions.FormatChannel(sChannel);
			var age = String.Format("**Created:** `{0}` (`{1}` days ago)", sChannel.CreatedAt.UtcDateTime, DateTime.UtcNow.Subtract(sChannel.CreatedAt.UtcDateTime).Days);
			var users = String.Format("**User Count:** `{0}`", sChannel.Users.Count);
			var all = String.Join("\n", new List<string>() { age, users });

			var embed = Actions.MakeNewEmbed(title, all);
			Actions.AddFooter(embed, "Channel Info");
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("inforole")]
		[Alias("infr")]
		[Usage("[Role]")]
		[Summary("Displays various information about the given role.")]
		[DefaultEnabled(true)]
		public async Task InfoRole([Remainder] string input)
		{
			var sRole = await Actions.GetRole(Context, input) as SocketRole;
			if (sRole == null)
				return;

			var title = Actions.FormatRole(sRole);
			var age = String.Format("**Created:** `{0}` (`{1}` days ago)", sRole.CreatedAt.UtcDateTime, DateTime.UtcNow.Subtract(sRole.CreatedAt.UtcDateTime).Days);
			var position = String.Format("**Position:** `{0}`", sRole.Position);
			var users = String.Format("**User Count:** `{0}`", (await Context.Guild.GetUsersAsync()).Where(x => x.RoleIds.Contains(sRole.Id)).Count());
			var all = String.Join("\n", new List<string>() { age, position, users });

			var embed = Actions.MakeNewEmbed(title, all);
			Actions.AddFooter(embed, "Role Info");
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("infobot")]
		[Alias("infb")]
		[Usage("")]
		[Summary("Displays various information about the bot.")]
		[DefaultEnabled(true)]
		public async Task InfoBot()
		{
			var span = DateTime.UtcNow.Subtract(Variables.StartupTime);

			//Make the description
			var online = String.Format("**Online Since:** {0}", Variables.StartupTime);
			var uptime = String.Format("**Uptime:** {0}:{1}:{2}:{3}", span.Days, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"));
			var guildCount = String.Format("**Guild Count:** {0}", Variables.TotalGuilds);
			var memberCount = String.Format("**Cumulative Member Count:** {0}", Variables.TotalUsers);
			var currShard = String.Format("**Current Shard:** {0}", Variables.Client.GetShardFor(Context.Guild).ShardId);
			var description = String.Join("\n", new[] { online, uptime, guildCount, memberCount, currShard });

			//Make the embed
			var embed = Actions.MakeNewEmbed(null, description);
			Actions.AddAuthor(embed, Variables.Bot_Name, Context.Client.CurrentUser.GetAvatarUrl());
			Actions.AddFooter(embed, "Version " + Constants.BOT_VERSION);

			//First field
			var firstField = Actions.FormatLoggedThings();
			Actions.AddField(embed, "Logged Actions", firstField);

			//Second field
			var attempt = String.Format("**Attempted Commands:** {0}", Variables.AttemptedCommands);
			var successful = String.Format("**Successful Commands:** {0}", Variables.AttemptedCommands - Variables.FailedCommands);
			var failed = String.Format("**Failed Commands:** {0}", Variables.FailedCommands);
			var secondField = String.Join("\n", new[] { attempt, successful, failed });
			Actions.AddField(embed, "Commands", secondField);

			//Third field
			var latency = String.Format("**Latency:** {0}ms", Variables.Client.GetLatency());
			var memory = String.Format("**Memory Usage:** {0}MB", Actions.GetMemory().ToString("0.00"));
			var threads = String.Format("**Thread Count:** {0}", Process.GetCurrentProcess().Threads.Count);
			var thirdField = String.Join("\n", new[] { latency, memory, threads });
			Actions.AddField(embed, "Technical", thirdField);

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, embed);
		}

		[Command("infouser")]
		[Alias("infu")]
		[Usage("<@User|ID>")]
		[Summary("Displays various information about the user. Not 100% accurate. If an ID is provided then the bot can get information on users who aren't currently on the guild.")]
		[DefaultEnabled(true)]
		public async Task InfoUser([Optional, Remainder] string input)
		{
			//Get the user
			IUser user = await Actions.GetUser(Context.Guild, input);// ?? await Actions.GetUserGlobal(input);
			if (user == null)
			{
				if (!String.IsNullOrWhiteSpace(input) && ulong.TryParse(input, out ulong ID))
				{
					user = Variables.Client.GetUser(ID);
				}
				else
				{
					user = await Context.Guild.GetUserAsync(Context.User.Id);
				}
			}

			if (user is IGuildUser)
			{
				var guildUser = user as IGuildUser;

				//Get a list of roles
				var roles = guildUser.RoleIds.Where(x => x != Context.Guild.Id).Select(x => Context.Guild.GetRole(x));

				//Get a list of channels
				var channels = new List<string>();
				(await Context.Guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
				{
					if (roles.Any(y => x.GetPermissionOverwrite(y).HasValue && x.GetPermissionOverwrite(y).Value.ReadMessages == PermValue.Allow) || guildUser.GetPermissions(x).ReadMessages)
					{
						channels.Add(x.Name);
					}
				});
				(await Context.Guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList().ForEach(x =>
				{
					if (roles.Any(y => x.GetPermissionOverwrite(y).HasValue && x.GetPermissionOverwrite(y).Value.Connect == PermValue.Allow) || guildUser.GetPermissions(x).Connect)
					{
						channels.Add(x.Name + " (Voice)");
					}
				});

				//Get an ordered list of when users joined the guild
				var guildUsers = await Context.Guild.GetUsersAsync();
				var users = guildUsers.Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks).ToList();

				//Make the description
				var description = String.Format(
					"**ID:** `{0}`\n" +
					"**Created:** `{1} {2}, {3} at {4}`\n" +
					"**Joined:** `{5} {6}, {7} at {8}` (`{9}` to join the guild)\n" +
					"\n" +
					"**Current game:** `{10}`\n" +
					"**Online status:** `{11}`\n",
					user.Id,
					System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.CreatedAt.Month),
					user.CreatedAt.UtcDateTime.Day,
					user.CreatedAt.UtcDateTime.Year,
					user.CreatedAt.UtcDateTime.ToLongTimeString(),
					System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(guildUser.JoinedAt.Value.UtcDateTime.Month),
					guildUser.JoinedAt.Value.UtcDateTime.Day,
					guildUser.JoinedAt.Value.UtcDateTime.Year,
					guildUser.JoinedAt.Value.UtcDateTime.ToLongTimeString(),
					users.IndexOf(guildUser) + 1,
					user.Game == null ? "N/A" : user.Game.Value.Name.ToString(),
					user.Status);

				var embed = Actions.MakeNewEmbed(null, description, roles.FirstOrDefault(x => x.Color.RawValue != 0)?.Color, thumbnailURL: user.GetAvatarUrl());
				Actions.AddAuthor(embed, String.Format("{0}#{1} {2}", user.Username, user.Discriminator, (guildUser.Nickname == null ? "" : "(" + guildUser.Nickname + ")")), user.GetAvatarUrl(), user.GetAvatarUrl());
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
				if (guildUser.VoiceChannel != null)
				{
					var text = String.Format("Server mute: `{0}`\nServer deafen: `{1}`\nSelf mute: `{2}`\nSelf deafen: `{3}`",
						guildUser.IsMuted.ToString(), guildUser.IsDeafened.ToString(), guildUser.IsSelfMuted.ToString(), guildUser.IsSelfDeafened.ToString());

					Actions.AddField(embed, "Voice Channel: " + guildUser.VoiceChannel.Name, text);
				}
				await Actions.SendEmbedMessage(Context.Channel, embed);
			}
			else
			{
				//Make the description
				var description = String.Format(
					"**Created:** `{0} {1}, {2} at {3}`\n" +
					"\n" +
					"**Current game:** `{4}`\n" +
					"**Online status:** `{5}`\n",
					System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(user.CreatedAt.Month),
					user.CreatedAt.UtcDateTime.Day,
					user.CreatedAt.UtcDateTime.Year,
					user.CreatedAt.UtcDateTime.ToLongTimeString(),
					user.Game == null ? "N/A" : user.Game.Value.Name.ToString(),
					user.Status);

				var embed = Actions.MakeNewEmbed(null, description, null, thumbnailURL: user.GetAvatarUrl());
				Actions.AddAuthor(embed, Actions.FormatUser(user, user?.Id), user.GetAvatarUrl(), user.GetAvatarUrl());
				Actions.AddFooter(embed, "Userinfo");
				await Actions.SendEmbedMessage(Context.Channel, embed);
			}
		}

		[Command("infoemoji")]
		[Alias("infe")]
		[Usage("[Emoji]")]
		[Summary("Discplays various information about an emoji. Only global emojis where the bot is in a guild that gives them will have a 'From...' text.")]
		[DefaultEnabled(true)]
		public async Task InfoEmoji([Remainder] string input)
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
		public async Task InfoInvite([Remainder] string input)
		{
			var inv = (await Context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code == input);
			if (inv == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("That invite cannot be gotten."));
				return;
			}

			var user = inv.Inviter;
			var userStr = Actions.FormatUser(user, user?.Id);
			var channelStr = Actions.FormatChannel(await Context.Guild.GetChannelAsync(inv.ChannelId));
			var usesStr = inv.Uses;
			var timeStr = inv.CreatedAt.UtcDateTime.ToShortTimeString();
			var dateStr = inv.CreatedAt.UtcDateTime.ToShortDateString();

			var embed = Actions.MakeNewEmbed(inv.Code, String.Format("**Inviter:** `{0}`\n**Channel:** `{1}`\n**Uses:** `{2}`\n**Created At:** `{3}` on `{4}`",
				userStr, channelStr, usesStr, timeStr, dateStr));
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
			var inputArray = input?.Split(new[] { ' ' }, 2);
			var formatStr = Actions.GetVariableAndRemove(inputArray, "type");

			//Get the type of image
			var format = ImageFormat.Auto;
			if (formatStr != null && !Enum.TryParse(formatStr, true, out format))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid avatar format supplied."));
				return;
			}

			//Get the user
			var user = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				user = Context.User as IGuildUser;
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
				return String.Format("`{0}.` `{1}` joined at `{2}` on `{3}`.",
					count++.ToString().PadLeft(padLength, '0'), Actions.FormatUser(x, x?.Id), time.ToShortTimeString(), time.ToShortDateString());
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

		[Command("userswithrole")]
		[Alias("uwr")]
		[Usage("[Role]")]
		[Summary("Prints out a list of all users with the given role. File specifies a text document which can show more symbols. Upload specifies to use a text uploader.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task UsersWithRole([Remainder] string input)
		{
			var role = await Actions.GetRole(Context, input);
			if (role == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			var users = "";
			var count = 1;
			(await Context.Guild.GetUsersAsync()).Where(x => x.JoinedAt.HasValue).ToList().OrderBy(x => x.JoinedAt).ToList().ForEach(x =>
			{
				if (x.RoleIds.ToList().Contains(role.Id))
				{
					users += String.Format("`{0}.` `{1}`\n", count++.ToString("00"), Actions.FormatUser(x, x?.Id));
				}
			});

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
			var users = (await Context.Guild.GetUsersAsync()).Where(x => Actions.CaseInsIndexOf(x.Username, input)).OrderBy(x => x.JoinedAt).ToList();
			var description = "";
			var count = 1;
			users.ForEach(x =>
			{
				description += String.Format("`{0}.` `{1}`\n", count++.ToString("00"), Actions.FormatUser(x, x?.Id));
			});

			var title = String.Format("Users With Names Containing '{0}'", input);
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(title, description));
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

		[Command("getpermnamesfromvalue")]
		[Alias("getperms")]
		[Usage("[Number]")]
		[Summary("Lists all the perms that come from the given value.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task GetPermsFromValue([Remainder] string input)
		{
			if (!uint.TryParse(input, out uint num))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Input is not a number."));
				return;
			}

			var perms = Actions.GetPermissionNames(num);
			if (!perms.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The given number holds no permissions."));
				return;
			}
			await Actions.SendChannelMessage(Context.Channel, String.Format("The number `{0}` has the following permissions: `{1}`.", num, String.Join("`, `", perms)));
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
			if (returnedChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object;

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

			List<DeleteInvAction> inviteCriteria = new List<DeleteInvAction>();
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
					inviteCriteria.Add(DeleteInvAction.User);
				}
			}
			IGuildChannel channel = null;
			if (!String.IsNullOrWhiteSpace(chanStr))
			{
				var returnedChannel = await Actions.GetChannelPermability(Context, chanStr);
				if (returnedChannel.Reason != FailureReason.Not_Failure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}
				else
				{
					channel = returnedChannel.Object;
					inviteCriteria.Add(DeleteInvAction.Channel);
				}
			}
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
					inviteCriteria.Add(DeleteInvAction.Uses);
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
					inviteCriteria.Add(DeleteInvAction.Uses);
				}
			}
			//Have gone through every other check so it's an error at this point
			if (!inviteCriteria.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid target supplied."));
				return;
			}

			//Make a new list to store the invites that match the conditions in
			var invites = guildInvites.ToList();
			foreach (var action in inviteCriteria)
			{
				switch (action)
				{
					case DeleteInvAction.User:
					{
						invites = invites.Where(x => x.Inviter.Id == user.Id).ToList();
						break;
					}
					case DeleteInvAction.Channel:
					{
						invites = invites.Where(x => x.ChannelId == channel.Id).ToList();
						break;
					}
					case DeleteInvAction.Uses:
					{
						invites = invites.Where(x => x.Uses == uses).ToList();
						break;
					}
					case DeleteInvAction.Expiry:
					{
						if (expires)
						{
							invites = invites.Where(x => x.MaxAge != null).ToList();
						}
						else
						{
							invites = invites.Where(x => x.MaxAge == null).ToList();
						}
						break;
					}
				}
			}

			//Check if any invites were gotten
			if (!invites.Any())
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No invites satisfied the given condition."));
				return;
			}

			var t = Task.Run(async () =>
			{
				var typing = Context.Channel.EnterTypingState();
				await invites.ForEachAsync(async x => await x.DeleteAsync());
				typing.Dispose();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` instant invites on this guild.", invites.Count));
			});
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
			var inputArray = input.Split(new[] { '/' }, 2);
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
			var evaluatedRole = await Actions.GetRoleEditAbility(Context, roleStr);
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
			var user = Context.User;
			await Actions.SendChannelMessage(Context, String.Format("{0}, {1}:{2}", Actions.FormatUser(user, user?.Id), role.Mention, textStr));
			//Remove the mentionability
			await role.ModifyAsync(x => x.Mentionable = false);
		}

		[Command("messagebotowner")]
		[Alias("mbo")]
		[Usage("[Message]")]
		[Summary("Sends a message to the bot owner with the given text. Messages will be cut down to 250 characters.")]
		[UserHasAPermission]
		[DefaultEnabled(true)]
		public async Task MessageBotOwner([Remainder] string input)
		{
			var cutMsg = input.Substring(0, Math.Min(input.Length, 250));
			var user = Context.User;
			var fromMsg = String.Format("From `{0}` in `{1}`:", Actions.FormatUser(user, user?.Id), Actions.FormatGuild(Context.Guild));
			var newMsg = String.Format("{0}\n```{1}```", fromMsg, cutMsg);
			var owner = Variables.Client.GetUser(Properties.Settings.Default.BotOwner);
			if (owner == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The owner is unable to be gotten."));
				return;
			}
			await (await owner.CreateDMChannelAsync()).SendMessageAsync(newMsg);
		}

		[Command("test")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task Test([Optional, Remainder] string input)
		{
			var temp = new BaseSpamInformation(SpamType.Image);
			temp.SpamCount(10, 5);

			await Actions.MakeAndDeleteSecondaryMessage(Context, "test");
		}
		#endregion
	}
}