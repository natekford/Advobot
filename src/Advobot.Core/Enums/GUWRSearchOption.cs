using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Options to search for a user with.
	/// </summary>
	[Flags]
	public enum GUWRSearchOption : uint
	{
		Count = (1U << 0),
		Nickname = (1U << 1),
		Exact = (1U << 2),
	}
}
