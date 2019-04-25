using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Arguments used for creating invites.
	/// </summary>
	[NamedArgumentType]
	public sealed class CreateInviteArguments
	{
#warning disallow negatives
		/// <summary>
		/// How long to make the invite last for.
		/// </summary>
		public int? Time { get; set; }
		/// <summary>
		/// How many uses to let the invite last for.
		/// </summary>
		public int? Uses { get; set; }
		/// <summary>
		/// Whether the user only receives temporary membership from the invite.
		/// </summary>
		public bool TemporaryMembership { get; set; }
	}
}