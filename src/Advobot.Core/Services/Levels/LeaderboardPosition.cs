using Advobot.Databases.Abstract;

namespace Advobot.Services.Levels
{
	/// <summary>
	/// Holds the user id and experience a user has.
	/// </summary>
	internal sealed class LeaderboardPosition : TimedDatabaseEntry
	{
		/// <summary>
		/// The id of the user.
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The total experience of the user.
		/// </summary>
		public int Experience { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="LeaderboardPosition"/>.
		/// </summary>
		public LeaderboardPosition() : base(default) { }
		/// <summary>
		/// Creates an instance of <see cref="LeaderboardPosition"/>.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="experience"></param>
		public LeaderboardPosition(ulong userId, int experience) : base(default)
		{
			UserId = userId;
			Experience = experience;
		}
	}
}