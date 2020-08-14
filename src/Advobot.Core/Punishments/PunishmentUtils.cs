using System.Threading.Tasks;

using AdvorangesUtils;

using Discord;

namespace Advobot.Punishments
{
	/// <summary>
	/// Utilities for punishments.
	/// </summary>
	public static class PunishmentUtils
	{
		/// <summary>
		/// Dynamically gives and removes punishments.
		/// </summary>
		/// <param name="punisher"></param>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="type"></param>
		/// <param name="isGive"></param>
		/// <param name="roleId"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static async Task DynamicHandleAsync(
			this IPunisher punisher,
			IGuild guild,
			ulong userId,
			PunishmentType type,
			bool isGive,
			ulong? roleId = null,
			RequestOptions? options = null)
		{
			var context = await GetContextAsync(guild, userId, type, isGive, roleId).CAF();
			if (context == null)
			{
				return;
			}

			context.Options = options;
			await punisher.HandleAsync(context).CAF();
		}

		private static async Task<PunishmentBase?> GetContextAsync(
			IGuild guild,
			ulong userId,
			PunishmentType type,
			bool isGive,
			ulong? roleId)
		{
			switch (type)
			{
				case PunishmentType.Ban:
					return new Ban(guild, userId, isGive);

				case PunishmentType.Softban:
					return isGive ? new SoftBan(guild, userId) : null;
			}

			var user = await guild.GetUserAsync(userId).CAF();
			return type switch
			{
				PunishmentType.Deafen => new Deafen(user, isGive),
				PunishmentType.VoiceMute => new Mute(user, isGive),
				PunishmentType.Kick => isGive ? new Kick(user) : null,
				PunishmentType.RoleMute when roleId is ulong id && guild.GetRole(id) is IRole role
					=> new RoleMute(user, isGive, role),
				_ => null,
			};
		}
	}
}