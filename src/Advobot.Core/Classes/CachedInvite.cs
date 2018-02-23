using Discord;
using System.Threading;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds the code and uses of an invite.
	/// </summary>
	public sealed class CachedInvite
	{
		private int _Uses;

		public string Code { get; }
		public int Uses => _Uses;

		public CachedInvite(IInviteMetadata invite) : this(invite.Code, invite.Uses) { }
		public CachedInvite(string code, int uses)
		{
			Code = code;
			_Uses = uses;
		}

		public void IncrementUses()
		{
			Interlocked.Increment(ref _Uses);
		}
	}
}
