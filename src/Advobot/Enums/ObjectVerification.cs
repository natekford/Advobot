using System;

namespace Advobot.Enums
{
	/// <summary>
	/// For use in <see cref="Classes.Attributes.VerifyObjectAttribute"/> inheriting classes to determine what to check.
	/// </summary>
	[Flags]
	public enum Verif : uint
	{
		/// <summary>
		/// No verification. This is generic.
		/// </summary>
		None = (1U << 0),
		/// <summary>
		/// Check if a user can edit it. This is generic.
		/// </summary>
		CanBeEdited = (1U << 1),
		/// <summary>
		/// Check if a user can move another from a channel. This is for users.
		/// </summary>
		CanBeMovedFromChannel = (1U << 2),
		/// <summary>
		/// Make sure the role is not everyone. This is for roles.
		/// </summary>
		IsNotEveryone = (1U << 2),
		/// <summary>
		/// Make sure the role is not managed. This is for roles.
		/// </summary>
		IsNotManaged = (1U << 3),
		/// <summary>
		/// Make sure the user can reorder this channel. This is for channels.
		/// </summary>
		CanBeReordered = (1U << 5),
		/// <summary>
		/// Make sure the user can modify permissions for this channel. This is for channels.
		/// </summary>
		CanModifyPermissions = (1U << 6),
		/// <summary>
		/// Make sure the user can manage this channel. This is for channels.
		/// </summary>
		CanBeManaged = (1U << 7),
		/// <summary>
		/// Make sure the user can move users from this channel. This is for channels.
		/// </summary>
		CanMoveUsers = (1U << 8),
		/// <summary>
		/// Make sure the user can delete messages from this channel. This is for channels.
		/// </summary>
		CanDeleteMessages = (1U << 9),
		/// <summary>
		/// Make sure the user can look at this channel. This is for channels.
		/// </summary>
		CanBeViewed = (1U << 10),
		/// <summary>
		/// Make sure the user can create invites on this channel. This is for channels.
		/// </summary>
		CanCreateInstantInvite = (1U << 11),
		/// <summary>
		/// Make sure the user can manage webhooks on this channel. This is for channels.
		/// </summary>
		CanManageWebhooks = (1U << 12),
	}
}
