using System;
using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;

namespace Advobot.Punishments
{
	/// <summary>
	/// Bans a user.
	/// </summary>
	public sealed class Ban : PunishmentBase
	{
		/// <summary>
		/// The amount of days worth of messages to delete.
		/// </summary>
		public int? Days { get; }

		/// <summary>
		/// Creates an instance of <see cref="Deafen"/>.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="isGive"></param>
		public Ban(IGuild guild, ulong userId, bool isGive) : base(guild, userId, isGive, PunishmentType.Ban)
		{
		}

		/// <inheritdoc/>
		internal override Task ExecuteAsync()
		{
			if (IsGive)
			{
				return Guild.AddBanAsync(UserId, Days ?? 1, Options?.AuditLogReason, Options);
			}
			return Guild.RemoveBanAsync(UserId, Options);
		}
	}
}