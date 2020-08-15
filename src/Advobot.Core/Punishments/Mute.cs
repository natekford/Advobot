using System.Threading.Tasks;

using Discord;

namespace Advobot.Punishments
{
	/// <summary>
	/// Mutes a user.
	/// </summary>
	public sealed class Mute : GuildUserPunishmentBase
	{
		/// <summary>
		/// Creates an instance of <see cref="Mute"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="isGive"></param>
		public Mute(IGuildUser user, bool isGive) : base(user, isGive, PunishmentType.VoiceMute)
		{
		}

		/// <inheritdoc/>
		protected internal override Task ExecuteAsync()
			=> User.ModifyAsync(x => x.Mute = IsGive, Options);
	}
}