﻿using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Gives a punishment to a user.
	/// </summary>
	public class PunishmentGiver : PunishmentBase
	{
		private TimeSpan _Time;

		public PunishmentGiver(int minutes, ITimersService timers) : base(timers)
		{
			_Time = TimeSpan.FromMinutes(minutes);
		}
		public PunishmentGiver(TimeSpan time, ITimersService timers) : base(timers)
		{
			_Time = time;
		}

		/// <summary>
		/// Bans a user from the guild. <paramref name="days"/> specifies how many days worth of messages to delete.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="reason"></param>
		/// <param name="days"></param>
		/// <returns></returns>
		public async Task BanAsync(IGuild guild, ulong userId, ModerationReason reason, int days = 1)
		{
			await guild.AddBanAsync(userId, days, null, reason.CreateRequestOptions()).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			After(PunishmentType.Ban, guild, ban.User, reason);
		}
		/// <summary>
		/// Bans then unbans a user from the guild. Deletes 1 days worth of messages.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task SoftbanAsync(IGuild guild, ulong userId, ModerationReason reason)
		{
			await guild.AddBanAsync(userId, 1, null, reason.CreateRequestOptions()).CAF();
			var ban = (await guild.GetBansAsync().CAF()).Single(x => x.User.Id == userId);
			await guild.RemoveBanAsync(userId, reason.CreateRequestOptions()).CAF();
			After(PunishmentType.Softban, guild, ban.User, reason);
		}
		/// <summary>
		/// Kicks a user from the guild.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task KickAsync(IGuildUser user, ModerationReason reason)
		{
			await user.KickAsync(null, reason.CreateRequestOptions()).CAF();
			After(PunishmentType.Kick, user.Guild, user, reason);
		}
		/// <summary>
		/// Gives a user the mute role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task RoleMuteAsync(IGuildUser user, IRole role, ModerationReason reason)
		{
			await RoleUtils.GiveRolesAsync(user, new[] { role }, reason).CAF();
			After(PunishmentType.RoleMute, user.Guild, user, reason);
		}
		/// <summary>
		/// Mutes a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task VoiceMuteAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Mute = true, reason.CreateRequestOptions()).CAF();
			After(PunishmentType.VoiceMute, user.Guild, user, reason);
		}
		/// <summary>
		/// Deafens a user from voice chat.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task DeafenAsync(IGuildUser user, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Deaf = true, reason.CreateRequestOptions()).CAF();
			After(PunishmentType.Deafen, user.Guild, user, reason);
		}
		/// <summary>
		/// Does an action on a user.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="user"></param>
		/// <param name="role"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public async Task PunishAsync(PunishmentType type, IGuildUser user, IRole role, ModerationReason reason)
		{
			var guild = user.GetGuild();
			var bot = guild.GetBot();
			if (!bot.GetIfCanModifyUser(user))
			{
				return;
			}

			switch (type)
			{
				case PunishmentType.Kick:
				{
					await KickAsync(user, reason).CAF();
					return;
				}
				case PunishmentType.Ban:
				{
					await BanAsync(guild, user.Id, reason).CAF();
					return;
				}
				case PunishmentType.Deafen:
				{
					await DeafenAsync(user, reason).CAF();
					return;
				}
				case PunishmentType.VoiceMute:
				{
					await VoiceMuteAsync(user, reason).CAF();
					return;
				}
				case PunishmentType.Softban:
				{
					await SoftbanAsync(guild, user.Id, reason).CAF();
					return;
				}
				case PunishmentType.RoleMute:
				{
					await RoleMuteAsync(user, role, reason).CAF();
					return;
				}
			}
		}

		protected override void After(PunishmentType type, IGuild guild, IUser user, ModerationReason reason)
		{
			var sb = new StringBuilder($"Successfully {_Given[type]} {user.Format()}. ");
			if (!_Time.Equals(default) && _Timers != null)
			{
				//Removing the punishments via the timers in whatever time is set
				_Timers.Add(new RemovablePunishment(type, guild, user, _Time));
				sb.Append($"They will be {_Removal[type]} in {_Time} minutes. ");
			}
			if (reason.Reason != null)
			{
				sb.Append($"The provided reason is `{reason.Reason.EscapeBackTicks()}`. ");
			}
			_Actions.Add(sb.ToString().Trim());
		}
	}
}
