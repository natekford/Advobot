using Advobot.Core.Actions.Formatting;
using Discord;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Explains why the bot is doing something.
	/// </summary>
	public class AutomaticModerationReason : ModerationReason
	{
		public AutomaticModerationReason(string reason) : base(null, reason)
		{
			User = null;
			Reason = reason == null ? "not specified" : reason.TrimEnd('.');
			HasReason = !String.IsNullOrWhiteSpace(reason);
			IsAutomatic = true;
		}

		public override string ToString() => $"Automatic action. Trigger: {Reason}.";
	}

	/// <summary>
	/// Explains why a mod is doing something.
	/// </summary>
	public class ModerationReason
	{
		public IUser User { get; protected set; }
		public string Reason { get; protected set; }
		public bool HasReason { get; protected set; }
		public bool IsAutomatic { get; protected set; }

		public ModerationReason(IUser user, string reason)
		{
			User = user;
			Reason = reason == null ? "not specified" : reason.TrimEnd('.');
			HasReason = !String.IsNullOrWhiteSpace(reason);
			IsAutomatic = false;
		}

		public RequestOptions CreateRequestOptions() => new RequestOptions { AuditLogReason = this.ToString(), };

		public override string ToString() => $"Action by {User.FormatUser()}. Reason: {Reason}.";
	}
}
