using System;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Punishments
{
	/// <summary>
	/// Context for a punishment being given or removed.
	/// </summary>
	public abstract class PunishmentBase : IPunishmentContext
	{
		/// <inheritdoc/>
		public int Days { get; set; } = 1;
		/// <inheritdoc />
		public IGuild Guild { get; protected set; }
		/// <inheritdoc />
		public bool IsGive { get; protected set; }
		/// <inheritdoc />
		public RequestOptions? Options { get; set; }
		/// <inheritdoc />
		public IRole? Role { get; protected set; }
		/// <inheritdoc />
		public TimeSpan? Time { get; set; }
		/// <inheritdoc />
		public PunishmentType Type { get; protected set; }
		/// <inheritdoc />
		public ulong UserId { get; protected set; }

		/// <summary>
		/// Creates an instance of <see cref="PunishmentBase"/>.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="isGive"></param>
		/// <param name="type"></param>
		protected PunishmentBase(IGuild guild, ulong userId, bool isGive, PunishmentType type)
		{
			Guild = guild;
			UserId = userId;
			IsGive = isGive;
			Type = type;
		}

		/// <inheritdoc />
		Task IPunishmentContext.ExecuteAsync() => ExecuteAsync();

		/// <inheritdoc />
		protected internal abstract Task ExecuteAsync();
	}
}