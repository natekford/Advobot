using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specify what punishment should be given.
	/// </summary>
	[Flags]
	public enum PunishmentType : uint
	{
		Kick = (1U << 0),
		Ban = (1U << 1),
		Deafen = (1U << 2),
		VoiceMute = (1U << 3),
		Softban = (1U << 4),
		RoleMute = (1U << 5),
	}
}
