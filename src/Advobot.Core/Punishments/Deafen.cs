
using Discord;

namespace Advobot.Punishments
{
	/// <summary>
	/// Deafens a user.
	/// </summary>
	public sealed class Deafen : GuildUserPunishmentBase
	{
		/// <summary>
		/// Creates an instance of <see cref="Deafen"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="isGive"></param>
		public Deafen(IGuildUser user, bool isGive) : base(user, isGive, PunishmentType.Deafen)
		{
		}

		/// <inheritdoc/>
		protected internal override Task ExecuteAsync()
			=> User.ModifyAsync(x => x.Deaf = IsGive, Options);
	}
}