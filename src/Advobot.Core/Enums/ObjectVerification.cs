using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// For use in <see cref="Classes.Attributes.VerifyObjectAttribute"/> inheriting classes to determine what to check.
	/// </summary>
	[Flags]
	public enum ObjectVerification : uint
	{
		//Generic
		None = (1U << 0),
		CanBeEdited = (1U << 1),

		//User
		CanBeMovedFromChannel = (1U << 2),

		//Role
		IsNotEveryone = (1U << 2),
		IsManaged = (1U << 3),

		//Channel
		CanBeReordered = (1U << 5),
		CanModifyPermissions = (1U << 6),
		CanBeManaged = (1U << 7),
		CanMoveUsers = (1U << 8),
		CanDeleteMessages = (1U << 9),
		CanBeRead = (1U << 10),
		CanCreateInstantInvite = (1U << 11)
	}
}
