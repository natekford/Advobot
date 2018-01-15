namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds the code and uses of an invite. Used in <see cref="Utilities.InviteUtils.GetInviteUserJoinedOnAsync(Interfaces.IGuildSettings, Discord.IGuildUser)"/>.
	/// </summary>
	public class CachedInvite
	{
		public string Code { get; }
		public int Uses { get; private set; }

		public CachedInvite(string code, int uses)
		{
			Code = code;
			Uses = uses;
		}

		public void IncrementUses()
		{
			++Uses;
		}
	}
}
