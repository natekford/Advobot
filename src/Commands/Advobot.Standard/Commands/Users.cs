using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Services.Time;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using Advobot.TypeReaders;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands
{
	[Category(nameof(Users))]
	public sealed class Users : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.Ban))]
		[LocalizedAlias(nameof(Aliases.Ban))]
		[LocalizedSummary(nameof(Summaries.Ban))]
		[Meta("b798e679-3ca7-4af1-9544-585672ec9936", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.BanMembers)]
		public sealed class Ban : AdvobotModuleBase
		{
			[Command]
			[Priority(1)]
			public Task Command(
				[CanModifyUser]
				IGuildUser user,
				[Remainder]
				ModerationReason reason = default
			) => Command(user.Id, reason);

			[Command]
			[Priority(0)]
			public async Task<RuntimeResult> Command(
				[NotBanned]
				ulong userId,
				[Remainder]
				ModerationReason reason = default
			)
			{
				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = reason.ToPunishmentArgs(this);

				var user = new AmbiguousUser(userId);
				await punisher.BanAsync(user, args).CAF();
				return Responses.Users.Banned(true, await user.GetAsync(Context.Client).CAF(), args);
			}

			[LocalizedCommand(nameof(Groups.Many))]
			[LocalizedAlias(nameof(Aliases.Many))]
			[Priority(3)]
			public Task<RuntimeResult> Many(params IGuildUser[] users)
				=> Many(users.Select(x => x.Id).ToArray());

			[LocalizedCommand(nameof(Groups.Many))]
			[LocalizedAlias(nameof(Aliases.Many))]
			[Priority(2)]
			public async Task<RuntimeResult> Many(params ulong[] userIds)
			{
				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = new PunishmentArgs
				{
					Options = GenerateRequestOptions(),
					Days = 7,
				};

				var users = new List<IUser>();
				foreach (var id in userIds)
				{
					var user = new AmbiguousUser(id);
					await punisher.BanAsync(user, args).CAF();
					users.Add(await user.GetAsync(Context.Client).CAF());
				}
				return Responses.Users.BannedMany(users, args);
			}
		}

		[LocalizedGroup(nameof(Groups.Deafen))]
		[LocalizedAlias(nameof(Aliases.Deafen))]
		[LocalizedSummary(nameof(Summaries.Deafen))]
		[Meta("99aa7f17-5710-41ce-ba12-291c2971c0a4", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.DeafenMembers)]
		public sealed class Deafen : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[Remainder]
				ModerationReason reason = default
			)
			{
				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = reason.ToPunishmentArgs(this);

				var shouldPunish = !user.IsDeafened;
				if (shouldPunish)
				{
					await punisher.DeafenAsync(user.AsAmbiguous(), args).CAF();
				}
				else
				{
					await punisher.RemoveDeafenAsync(user.AsAmbiguous(), args).CAF();
				}
				return Responses.Users.Deafened(shouldPunish, user, args);
			}
		}

		[LocalizedGroup(nameof(Groups.DisplayCurrentBanList))]
		[LocalizedAlias(nameof(Aliases.DisplayCurrentBanList))]
		[LocalizedSummary(nameof(Summaries.DisplayCurrentBanList))]
		[Meta("419c1846-b232-475e-aa19-45cb282dc9e0", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.BanMembers)]
		public sealed class DisplayCurrentBanList : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
			{
				var bans = await Context.Guild.GetBansAsync().CAF();
				return Responses.Users.DisplayBans(bans);
			}
		}

		[LocalizedGroup(nameof(Groups.ForAllWithRole))]
		[LocalizedAlias(nameof(Aliases.ForAllWithRole))]
		[LocalizedSummary(nameof(Summaries.ForAllWithRole))]
		[Meta("0dd92f6d-e4ad-4c80-82f0-da6c3e02743c", IsEnabled = true)]
		[RequireGuildPermissions]
		public sealed class ForAllWithRole : MultiUserActionModule
		{
			[LocalizedCommand(nameof(Groups.ClearNickname), RunMode = RunMode.Async)]
			[LocalizedAlias(nameof(Aliases.ClearNickname))]
			public Task<RuntimeResult> ClearNickname(
				IRole target,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
				bool bypass = false
			)
			{
				Task UpdateAsync(IGuildUser user, RequestOptions options)
				{
					if (user.Nickname != null)
					{
						return user.ModifyAsync(x => x.Nickname = user.Username, options);
					}
					return Task.CompletedTask;
				}
				return CommandRunner(target, bypass, UpdateAsync);
			}

			[LocalizedCommand(nameof(Groups.GiveNickname), RunMode = RunMode.Async)]
			[LocalizedAlias(nameof(Aliases.GiveNickname))]
			public Task<RuntimeResult> GiveNickname(
				IRole target,
				[Nickname]
				string nickname,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
				bool bypass = false
			)
			{
				Task UpdateAsync(IGuildUser user, RequestOptions options)
				{
					if (user.Nickname != nickname)
					{
						return user.ModifyAsync(x => x.Nickname = nickname, options);
					}
					return Task.CompletedTask;
				}
				return CommandRunner(target, bypass, UpdateAsync);
			}

			[LocalizedCommand(nameof(Groups.GiveRole), RunMode = RunMode.Async)]
			[LocalizedAlias(nameof(Aliases.GiveRole))]
			public Task<RuntimeResult> GiveRole(
				IRole target,
				[CanModifyRole, NotEveryone, NotManaged]
				IRole give,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
				bool bypass = false
			)
			{
				if (target.Id == give.Id)
				{
					return Responses.Users.CannotGiveGatheredRole();
				}

				Task UpdateAsync(IGuildUser user, RequestOptions options)
				{
					if (!user.RoleIds.Contains(give.Id))
					{
						return user.AddRoleAsync(give, options);
					}
					return Task.CompletedTask;
				}
				return CommandRunner(target, bypass, UpdateAsync);
			}

			[LocalizedCommand(nameof(Groups.TakeRole), RunMode = RunMode.Async)]
			[LocalizedAlias(nameof(Aliases.TakeRole))]
			public Task<RuntimeResult> TakeRole(
				IRole target,
				[CanModifyRole, NotEveryone, NotManaged]
				IRole take,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
				bool bypass = false
			)
			{
				Task UpdateAsync(IGuildUser user, RequestOptions options)
				{
					if (user.RoleIds.Contains(take.Id))
					{
						return user.RemoveRoleAsync(take, options);
					}
					return Task.CompletedTask;
				}
				return CommandRunner(target, bypass, UpdateAsync);
			}

			private async Task<RuntimeResult> CommandRunner(
				IRole role,
				bool bypass,
				Func<IGuildUser, RequestOptions, Task> update)
			{
				var options = GenerateRequestOptions();
				ProgressLogger = new MultiUserActionProgressLogger(
					Context.Channel,
					i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
					options
				);

				var amountChanged = await ProcessAsync(
					bypass,
					u => u.RoleIds.Contains(role.Id),
					update,
					options
				).CAF();
				return Responses.Users.MultiUserActionSuccess(amountChanged);
			}
		}

		[LocalizedGroup(nameof(Groups.GetBanReason))]
		[LocalizedAlias(nameof(Aliases.GetBanReason))]
		[LocalizedSummary(nameof(Summaries.GetBanReason))]
		[Meta("5ba658c2-c689-4b3a-b7f1-77329dd6e971", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.BanMembers)]
		public sealed class GetBanReason : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(IBan ban)
				=> Responses.Users.DisplayBanReason(ban);
		}

		[LocalizedGroup(nameof(Groups.Kick))]
		[LocalizedAlias(nameof(Aliases.Kick))]
		[LocalizedSummary(nameof(Summaries.Kick))]
		[Meta("1d86aa7d-da06-478c-861b-a62ca279523b", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.KickMembers)]
		public sealed class Kick : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyUser]
				IGuildUser user,
				[Remainder]
				ModerationReason reason = default
			)
			{
				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = reason.ToPunishmentArgs(this);

				await punisher.KickAsync(user.AsAmbiguous(), args).CAF();
				return Responses.Users.Kicked(user);
			}
		}

		[LocalizedGroup(nameof(Groups.MoveUser))]
		[LocalizedAlias(nameof(Aliases.MoveUser))]
		[LocalizedSummary(nameof(Summaries.MoveUser))]
		[Meta("158dca6d-fc89-43b3-a6b5-d055f6672547", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.MoveMembers)]
		public sealed class MoveUser : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanBeMoved]
				IGuildUser user,
				[CanModifyChannel(MoveMembers)]
				IVoiceChannel channel
			)
			{
				if (user.VoiceChannel?.Id == channel.Id)
				{
					return Responses.Users.AlreadyInChannel(user, channel);
				}

				await user.ModifyAsync(x => x.Channel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Users.Moved(user, channel);
			}
		}

		[LocalizedGroup(nameof(Groups.MoveUsers))]
		[LocalizedAlias(nameof(Aliases.MoveUsers))]
		[LocalizedSummary(nameof(Summaries.MoveUsers))]
		[Meta("4e8439fa-cc29-4acb-9049-89865be825c8", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.MoveMembers)]
		public sealed class MoveUsers : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(
				[CanModifyChannel(MoveMembers)]
				IVoiceChannel input,
				[CanModifyChannel(MoveMembers)]
				IVoiceChannel output,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
				bool bypass
			)
			{
				var options = GenerateRequestOptions();
				ProgressLogger = new MultiUserActionProgressLogger(
					Context.Channel,
					i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
					options
				);

				var users = await input.GetUsersAsync().FlattenAsync().CAF();
				var amountChanged = await ProcessAsync(
					users,
					bypass,
					_ => true,
					(u, o) => u.ModifyAsync(x => x.Channel = Optional.Create(output), o),
					options
				).CAF();
				return Responses.Users.MultiUserActionSuccess(amountChanged);
			}
		}

		[LocalizedGroup(nameof(Groups.Mute))]
		[LocalizedAlias(nameof(Aliases.Mute))]
		[LocalizedSummary(nameof(Summaries.Mute))]
		[Meta("b9f305d4-d343-4350-a140-c54a42af8d8d", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles, GuildPermission.ManageMessages)]
		public sealed class Mute : AdvobotModuleBase
		{
			private static readonly OverwritePermissions TextPerms = new OverwritePermissions(0, (ulong)(0
				| CreateInstantInvite
				| ManageChannels
				| ManageRoles
				| ManageWebhooks
				| SendMessages
				| ManageMessages
				| AddReactions));

			private static readonly OverwritePermissions VoicePerms = new OverwritePermissions(0, (ulong)(0
				| CreateInstantInvite
				| ManageChannels
				| ManageRoles
				| ManageWebhooks
				| Speak
				| MuteMembers
				| DeafenMembers
				| MoveMembers));

			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[Remainder]
				ModerationReason reason = default
			)
			{
				var role = await GetOrCreateMuteRoleAsync().CAF();

				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = reason.ToPunishmentArgs(this);
				args.Role = role;

				var shouldPunish = !user.RoleIds.Contains(role.Id);
				if (shouldPunish)
				{
					await punisher.RoleMuteAsync(user.AsAmbiguous(), args).CAF();
				}
				else
				{
					await punisher.RemoveRoleMuteAsync(user.AsAmbiguous(), args).CAF();
				}
				return Responses.Users.Muted(shouldPunish, user, args);
			}

			private async Task<IRole> GetOrCreateMuteRoleAsync()
			{
				IRole muteRole = Context.Guild.GetRole(Context.Settings.MuteRoleId);
				var result = await Context.User.ValidateRole(muteRole).CAF();
				if (!result.IsSuccess)
				{
					muteRole = await Context.Guild.CreateRoleAsync("Advobot Mute", new GuildPermissions(0), null, false, false, null).CAF();
					Context.Settings.MuteRoleId = muteRole.Id;
					Context.Settings.Save();
				}

				foreach (var textChannel in Context.Guild.TextChannels)
				{
					if (textChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await textChannel.AddPermissionOverwriteAsync(muteRole, TextPerms).CAF();
					}
				}
				foreach (var voiceChannel in Context.Guild.VoiceChannels)
				{
					if (voiceChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await voiceChannel.AddPermissionOverwriteAsync(muteRole, VoicePerms).CAF();
					}
				}
				return muteRole;
			}
		}

		[LocalizedGroup(nameof(Groups.PruneUsers))]
		[LocalizedAlias(nameof(Aliases.PruneUsers))]
		[LocalizedSummary(nameof(Summaries.PruneUsers))]
		[Meta("89abd319-c597-4e1a-9397-4f7d079f4e0e", IsEnabled = true)]
		[RequireGuildPermissions]
		public sealed class PruneUsers : AdvobotModuleBase
		{
			[LocalizedCommand(nameof(Groups.Fake))]
			[LocalizedAlias(nameof(Aliases.Fake))]
			public async Task<RuntimeResult> Fake(
				[PruneDays]
				int days
			)
			{
				var amt = await Context.Guild.PruneUsersAsync(days, simulate: true, GenerateRequestOptions()).CAF();
				return Responses.Users.FakePruned(days, amt);
			}

			[LocalizedCommand(nameof(Groups.Real))]
			public async Task<RuntimeResult> Real(
				[PruneDays]
				int days
			)
			{
				var amt = await Context.Guild.PruneUsersAsync(days, simulate: false, GenerateRequestOptions()).CAF();
				return Responses.Users.Pruned(days, amt);
			}
		}

		[LocalizedGroup(nameof(Groups.RemoveMessages))]
		[LocalizedAlias(nameof(Aliases.RemoveMessages))]
		[LocalizedSummary(nameof(Summaries.RemoveMessages))]
		[Meta("a4f3959e-1f56-4bf0-b377-dc98ef017906", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageMessages)]
		public sealed class RemoveMessages : AdvobotModuleBase
		{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
			public ITime Time { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

			[Command]
			[RequireChannelPermissions(ManageMessages)]
			public Task<RuntimeResult> Command(
				[Positive]
				int requestCount
			) => CommandRunner(requestCount, Context.Channel, null);

			[Command]
			[RequireChannelPermissions(ManageMessages)]
			public Task<RuntimeResult> Command(
				[Positive]
				int requestCount,
				IUser user
			) => CommandRunner(requestCount, Context.Channel, user);

			[Command]
			public Task<RuntimeResult> Command(
				[Positive]
				int requestCount,
				[CanModifyChannel(ManageMessages)]
				ITextChannel channel
			) => CommandRunner(requestCount, channel, null);

			[Command]
			public Task<RuntimeResult> Command(
				[Positive]
				int requestCount,
				IUser user,
				[CanModifyChannel(ManageMessages)]
				ITextChannel channel
			) => CommandRunner(requestCount, channel, user);

			[Command]
			public Task<RuntimeResult> Command(
				[Positive]
				int requestCount,
				[CanModifyChannel(ManageMessages)]
				ITextChannel channel,
				IUser user
			) => CommandRunner(requestCount, channel, user);

			private async Task<RuntimeResult> CommandRunner(
				int req,
				ITextChannel channel,
				IUser? user)
			{
				var options = GenerateRequestOptions();
				//If not the context channel then get the first message in that channel
				var thisChannel = Context.Message.Channel.Id == channel.Id;
				IMessage start = Context.Message;
				if (!thisChannel)
				{
					var msgs = await channel.GetMessagesAsync(1).FlattenAsync().CAF();
					start = msgs.FirstOrDefault();
				}

				//If there is a non null user then delete messages specifically from that user
				Func<IMessage, bool>? predicate = null;
				if (user != null)
				{
					predicate = x => x.Author.Id == user?.Id;
				}
				var now = Time.UtcNow;
				var deleted = await MessageUtils.DeleteMessagesAsync(channel, start, req, now, options, predicate).CAF();

				//If the context channel isn't the targetted channel then delete the start message
				//Increase by one to account for it not being targetted.
				if (!thisChannel)
				{
					await start.DeleteAsync(options).CAF();
					++deleted;
				}

				return Responses.Users.RemovedMessages(channel, user, deleted);
			}
		}

		[LocalizedGroup(nameof(Groups.SoftBan))]
		[LocalizedAlias(nameof(Aliases.SoftBan))]
		[LocalizedSummary(nameof(Summaries.SoftBan))]
		[Meta("a6084728-77bf-469c-af09-41e53ac021d9", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.BanMembers, GuildPermission.KickMembers)]
		public sealed class SoftBan : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[CanModifyUser]
				IGuildUser user,
				[Remainder]
				ModerationReason reason = default
			) => Command(user.Id, reason);

			[Command]
			public async Task<RuntimeResult> Command(
				[NotBanned]
				ulong userId,
				[Remainder]
				ModerationReason reason = default
			)
			{
				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = reason.ToPunishmentArgs(this);

				var user = new AmbiguousUser(userId);
				await punisher.SoftbanAsync(user, args).CAF();
				return Responses.Users.SoftBanned(await user.GetAsync(Context.Client).CAF());
			}
		}

		[LocalizedGroup(nameof(Groups.Unban))]
		[LocalizedAlias(nameof(Aliases.Unban))]
		[LocalizedSummary(nameof(Summaries.Unban))]
		[Meta("417e9dd0-306b-4d1f-8b62-0427f01f921a", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.BanMembers)]
		public sealed class Unban : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IBan ban,
				[Remainder]
				ModerationReason reason = default
			)
			{
				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = reason.ToPunishmentArgs(this);

				await punisher.UnbanAsync(ban.User.AsAmbiguous(), args).CAF();
				return Responses.Users.Banned(false, ban.User, args);
			}
		}

		[LocalizedGroup(nameof(Groups.VoiceMute))]
		[LocalizedAlias(nameof(Aliases.VoiceMute))]
		[LocalizedSummary(nameof(Summaries.VoiceMute))]
		[Meta("a51ea911-10be-4e40-8995-a507015a7e57", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.MuteMembers)]
		public sealed class VoiceMute : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[Remainder]
				ModerationReason reason = default
			)
			{
				var punisher = new PunishmentManager(Context.Guild, Timers);
				var args = reason.ToPunishmentArgs(this);

				var shouldPunish = !user.IsMuted;
				if (shouldPunish)
				{
					await punisher.VoiceMuteAsync(user.AsAmbiguous(), args).CAF();
				}
				else
				{
					await punisher.RemoveVoiceMuteAsync(user.AsAmbiguous(), args).CAF();
				}
				return Responses.Users.VoiceMuted(shouldPunish, user, args);
			}
		}
	}
}