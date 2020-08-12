namespace Advobot.SQLite.Relationships
{
	/// <summary>
	/// Represents an object which belongs to a user.
	/// </summary>
	public interface IUserChild
	{
		/// <summary>
		/// The user's id.
		/// </summary>
		ulong UserId { get; }
	}
}