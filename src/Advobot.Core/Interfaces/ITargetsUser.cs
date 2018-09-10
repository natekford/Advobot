namespace Advobot.Interfaces
{
	/// <summary>
	/// Indicates this object targets a user.
	/// </summary>
	public interface ITargetsUser
	{
		/// <summary>
		/// The targeted user.
		/// </summary>
		ulong UserId { get; }
	}
}