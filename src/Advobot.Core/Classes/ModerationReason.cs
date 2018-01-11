using Advobot.Core.Utilities.Formatting;
using Discord;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Explains why a mod is doing something.
	/// </summary>
	public class ModerationReason
	{
		public IUser User { get; protected set; }
		public string Reason { get; protected set; }

		public ModerationReason(string reason)
		{
			Reason = (reason ?? "not specified").TrimEnd('.');
		}
		public ModerationReason(IUser user, string reason)
		{
			User = user ?? throw new ArgumentException("should not be null if passed in", nameof(user));
			Reason = (reason ?? "not specified").TrimEnd('.');
		}

		public RequestOptions CreateRequestOptions() => new RequestOptions { AuditLogReason = ToString(), };

		public override string ToString() => User == null
			? $"Automatic action. Trigger: {Reason}."
			: $"Action by {User.FormatUser()}. Reason: {Reason}.";
	}
}
