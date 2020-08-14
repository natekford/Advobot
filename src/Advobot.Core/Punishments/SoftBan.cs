using System;
using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;

namespace Advobot.Punishments
{

	/// <summary>
	/// Softbans a user.
	/// </summary>
	public sealed class SoftBan : PunishmentBase
	{
		/// <summary>
		/// The amount of days worth of messages to delete.
		/// </summary>
		public int? Days { get; }

		/// <summary>
		/// Creates an instance of <see cref="Kick"/>.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		public SoftBan(IGuild guild, ulong userId) : base(guild, userId, true, PunishmentType.Kick)
		{
		}

		internal override async Task ExecuteAsync()
		{
			await Guild.AddBanAsync(UserId, Days ?? 1, Options?.AuditLogReason, Options).CAF();
			await Guild.RemoveBanAsync(UserId, Options).CAF();
		}
	}
}