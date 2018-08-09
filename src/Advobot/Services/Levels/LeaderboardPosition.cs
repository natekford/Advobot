using Advobot.Classes;
using LiteDB;

namespace Advobot.Services.Levels
{
	internal sealed class LeaderboardPosition : DatabaseEntry
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
			Id = new ObjectId(UserId.ToString().PadLeft(24, '0'));
		}
	}
}