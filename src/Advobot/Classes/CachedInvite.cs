using Discord;
using System.Threading;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds the code and uses of an invite.
	/// </summary>
	public sealed class CachedInvite
	{
		/// <summary>
		/// The code of the invite.
		/// </summary>
		public string Code { get; }
		/// <summary>
		/// How many uses the invite has.
		/// </summary>
		public int Uses => _Uses;

		private int _Uses;

		/// <summary>
		/// Initializes a cached invite using the code and uses of the metadata.
		/// </summary>
		/// <param name="invite"></param>
		public CachedInvite(IInviteMetadata invite) : this(invite.Code, invite.Uses ?? 0) { }
		/// <summary>
		/// Initializes a cached invite using the code and uses.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="uses"></param>
		public CachedInvite(string code, int uses)
		{
			Code = code;
			_Uses = uses;
		}

		/// <summary>
		/// Increments the stored uses.
		/// </summary>
		public void IncrementUses()
		{
			Interlocked.Increment(ref _Uses);
		}
	}
}
