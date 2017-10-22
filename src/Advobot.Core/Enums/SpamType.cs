using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies what spam prevention to modify/set up.
	/// </summary>
	[Flags]
	public enum SpamType : uint
	{
		Message = (1U << 0),
		LongMessage = (1U << 1),
		Link = (1U << 2),
		Image = (1U << 3),
		Mention = (1U << 4),
	}
}
