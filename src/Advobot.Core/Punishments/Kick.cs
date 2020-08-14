using System;
using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;

namespace Advobot.Punishments
{
	/// <summary>
	/// Kicks a user.
	/// </summary>
	public sealed class Kick : GuildUserPunishmentBase
	{
		/// <summary>
		/// Creates an instance of <see cref="Kick"/>.
		/// </summary>
		/// <param name="user"></param>
		public Kick(IGuildUser user) : base(user, true, PunishmentType.Kick)
		{
		}

		/// <inheritdoc />
		internal override Task ExecuteAsync()
			=> User.KickAsync(Options?.AuditLogReason, Options);
	}
}