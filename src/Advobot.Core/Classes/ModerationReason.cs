using Advobot.Core.Utilities.Formatting;
using Discord;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Explains why a mod is doing something.
	/// </summary>
	public struct ModerationReason
	{
		public readonly IUser User;
		public readonly string Reason;

		public ModerationReason(string reason)
		{
			User = null;
			Reason = (reason ?? "not specified").TrimEnd('.');
		}
		public ModerationReason(IUser user, string reason)
		{
			User = user ?? throw new ArgumentException("should not be null if passed in", nameof(user));
			Reason = (reason ?? "not specified").TrimEnd('.');
		}

		public RequestOptions CreateRequestOptions()
		{
			return new RequestOptions { AuditLogReason = ToString(), };
		}

		public override string ToString()
		{
			return User == null
? $"Automatic action. Trigger: {Reason}."
: $"Action by {User.FormatUser()}. Reason: {Reason}.";
		}
	}
}
