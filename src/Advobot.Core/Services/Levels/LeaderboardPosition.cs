using System;
using Advobot.Databases.Abstract;
using Discord;

namespace Advobot.Services.Levels
{
	/// <summary>
	/// Holds the user id and experience a user has.
	/// </summary>
	internal sealed class LeaderboardPosition : TimedDatabaseEntry<ulong>
	{
		/// <summary>
		/// The total experience of the user.
		/// </summary>
		public int Experience { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="LeaderboardPosition"/>.
		/// </summary>
		public LeaderboardPosition() : this(0, 0) { }
		/// <summary>
		/// Creates an instance of <see cref="LeaderboardPosition"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="experience"></param>
		public LeaderboardPosition(IUser user, int experience) : this(user.Id, experience) { }
		/// <summary>
		/// Creates an instance of <see cref="LeaderboardPosition"/>.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="experience"></param>
		public LeaderboardPosition(ulong userId, int experience)
			: base(userId, TimeSpan.FromSeconds(3))
		{
			Experience = experience;
		}
	}
}